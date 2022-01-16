# OREL.Net
An expresson language that can be used to transform and reshape .net object easily.  OREL is short for Object Reshape Expression Language.

## Quick Start

### Scenario 1
Suppose we have such data with json format which retrived from a remote api
``` json
{
  "code": 0,
  "success": true,
  "data": [
    {
      "note_list": [
        {
          "id": "61614cd40000000001025937",
          "title": "零食测评｜芝士就是力量🤗我猜你都没吃过",
          "desc": "第二个真的是满满纯芝士[皱眉R]零食测评  可爱零食  零食推荐  芝士 ",
          "images_list": [
            {
              "url": "http://sns-img-hw.somecdn.com/1b95e440-a3dd-3a55-8f68-789b08443541?imageView2/2/w/1080/format/webp"
            }
          ],
          "user": {
            "id": "5dcf533c000000000100729a",
            "name": "白白美食屋",
            "gender": 1
          },
          "time": 1633766612,
          "view_count": "8k",
          "topics": "{\"id\":\"59b75338e39faf37176829d9\",\"name\":\"零食测评\"}"
        }
      ]
    }
  ]
}
```

We need iterate each item from note_list array, convert them to appropriate format for storage or downstream data processing.

|  column  | type | how to set |
| -- | -- | -- |
| id | string | the id property |
| title | string | the title property  |
| desc | string | the desc property  |
| firstImageUrl | string | the url of first image in image list  |
| userId | string | id of user |
| userName | string | name of user|
| gender | string | "male" or "female" |
| createTime | dateTime | ISO date format of time which is unix timestamp now |
| viewCount | number | the number value of view_count |
| topic | string | name property of json text "topics" |
| tags | array | split text of desc by space and take from the second segment |

you can do it by orel like this: 
``` csharp
//suppose variable jsonText represents the raw json content
var obj = JsonConvert.DeserializeObject(jsonText); 

//generate schema info from raw object, the schema is used to help orel to check if usage of property reference in expression is correct.
var schema = SchemaProvider.FromObject(obj); 

//use static Compile method to compile the expression to a ORELExecutable instance.
ORELExecutable exe = OREL.Compile(@"data.note_list=>
{
    id,
    title,
    desc,
    firstImageUrl: images_list[1].url,
    userId: user.Id,
    userName: user.Name,
    gender: if(user.gender=1, 'female', 'male'),
    createTime: date(time),
    viewCount: num2(view_count),
    topic: j2o(topics).name,
    tags: split(desc,'  ')[2..]
}", schema);   

//call Execute method to do the conversion, result is a dynamic object that has the same schema with our target definition.
var result = exe.Execute(obj); 

//assume the obj2 is another raw data object. the Orel Expression need only be compiled once, and be executed multiple times.
var result2 = exe.Execute(obj2); 
```

Now we can convert final result to json format text
``` csharp
  //use ORELJsonWriter to serialize orel result object
  var settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ORELJsonWriter() },
                Formatting = Formatting.Indented,
            };
  var resultJson = JsonConvert.SerializeObject(result, settings);
```
Here is the content of json text
``` json
[
  {
    "id": "61614cd40000000001025937",
    "title": "零食测评｜芝士就是力量🤗我猜你都没吃过",
    "desc": "第二个真的是满满纯芝士[皱眉R]零食测评  可爱零食  零食推荐  芝士 ",
    "firstImageUrl": "http://sns-img-hw.xhscdn.com/1b95e440-a3dd-3a55-8f68-789b08443541?imageView2/2/w/1080/format/webp",
    "userId": "5dcf533c000000000100729a",
    "userName": "白白美食屋",
    "gender": "female",
    "createTime": "2021-10-09T16:03:32+08:00",
    "viewCount": 8000,
    "topic": "零食测评",
    "tags": [
      "可爱零食",
      "零食推荐",
      "芝士 "
    ]
  }
]
```

### Scenario 2
Suppose we hava such data which represents some comments from user

``` json
{
  "Comments": [
    {
      "LikedCount": "10",
      "Content": "外观方面，XT5新车型将与现款XT5基本保持一致",
      "Author": "韦石泉",
      "CreateTime": "2017年 09月 16日 10时 45分"
    },
    {
      "LikedCount": "20",
      "Content": "斯威7自动挡预售价发布 成都车展上市",
      "Author": "全是石头路啊",
      "CreateTime": "2017年 09月 16日 00时 00分"
    },
    {
      "LikedCount": "22",
      "Content": "动力不足",
      "Author": "宜之城",
      "CreateTime": "2017年 09月 15日 20时 45分"
    },
    {
      "LikedCount": "31",
      "Content": "不是要让低端人群买得起车，最关键看车质量，你这车怎么也属于轿车，还没面包贵，。。。。。",
      "Author": "矮了敷药",
      "CreateTime": "2017年 09月 15日 20时 30分"
    },
    {
      "LikedCount": "42",
      "Content": "这个配置真的神级车了",
      "Author": "铁甲依然宅",
      "CreateTime": "2017年 09月 14日 13时 48分"
    }
  ]
}
```

We want to choose the comments which published from 2017-9-15 20:30 (include) to 2017-9-16 (exclude), and LikedCount must exceed 20.
Then we can do it like this:

``` csharp
//create the schema manually
var schema = new MemberDefinition[] {
                new MemberDefinition("Comments",  DataType.List),
                new MemberDefinition("LikedCount",  DataType.Number, "Comments"),
                new MemberDefinition("CreateTime",  DataType.DateTime, "Comments"),
            };

//compile the expression of filtering
var exe = OREL.Compile("Comments[LikedCount > 20 and CreateTime between ['2017-9-15 20:30:00','2017-9-16')]", schema);

//run it
var result = exe.Execute(obj);

```

Here is the json of result

``` json
[
  {
    "LikedCount": "22",
    "Content": "动力不足",
    "Author": "宜之城",
    "CreateTime": "2017年 09月 15日 20时 45分"
  },
  {
    "LikedCount": "31",
    "Content": "不是要让低端人群买得起车，最关键看车质量，你这车怎么也属于轿车，还没面包贵，。。。。。",
    "Author": "矮了敷药",
    "CreateTime": "2017年 09月 15日 20时 30分"
  }
]
```

## Expression Syntax (updating...)
### Operand
There are two types of operand in orel, Property and Constant

**Property:**
Indicates the proptery from .net object or json field. 

All basic clr types or jtoken types (using newtonsoft.json to process json data) are mapped to 6 orel data types:

| Orel DataType | CLR Type  | JToken Type |
|---|---|----|
|Number| all numeric types including long,int,byte,double,fload etc and their wrapped form of Nullable<>| Integer,Float  | 
|Text| string | string, Uri, Guid  | 
|DateTime| DateTime,DateTimeOffset and their wrapped form of Nullable<> | Date |
| List | derived types from IList , IList<> | Array |
| Boolean | bool, bool? | Boolean | 
| Object | dynamic type object or normal .net object | Object | 

**Constant:**
The constant value for operation

| Orel Type | Description | Sample |
| -- | -- | -- |
| Number | Any literal value of int type or float type| 1, 3.14|
| Text | text content that enclosed by quotation marks | 'a',"orel" | 
| DateTime | text which presents the date and time  | '2020-1-1', '2020-11-01T12:00:00Z' |
| Boolean | two literal values represents true and false | true, false |
| Null | represents null, can be compared to any type | null, NULL |

### Arithmetic Operations
> Arithmetic operations must be applied to two operands of the same type. When one operand is type of DateTime and another is Text, the Text type Operand must be a value with legal format of time interval.

| Operator | Operand Type1 | Operand Type2 | Function | Sample |
| -- | -- | -- | --| --|
| + | Number | Number | Arithmetic addition | 1+1=2 |
| + | Text | Text | Text concat | '1'+'1'='11' |
| + | DateTime | Text | DateTime addition | date('2018-1-1')+'1d'='2018-2-1' |
| + | DateTime | Text | DateTime addition | date('2018-1-1')+'1d'='2018-2-1' |
| - | Number | Number | Arithmetic subtraction | 1-1=0 |
| - | DateTime | Text | DateTime subtraction | date('2018-1-1')-'1d'='2017-12-31' |
| * | Number | Number | Arithmetic multiplication | 1+2*3=7 |
| / | Number | Number | Arithmetic division | 3/3=1 |

### Comparison Operations
> Comparison operations must be applied to two operands of the same type. When one operand is type of DateTime and another is Text, the Text type Operand must be a value with legal format of DateTime.

| Operator | Funtion | Support Type  | Sample |
| -- | -- | -- | --|
| = | Equal | Number, Text, DateTime  | Title = 'ABC' |
| > | Larger than | Number, DateTime  | ReadCount > 1|
| >= | Larger than or equal | Number, DateTime | PublishTime >= '2018-8-31'  |
| < | Less than | Number, DateTime | len(Comments) < 1 |
| <= | Less than or equal | Number, DateTime | PublishTime <= '2018-8-31' |
| != | not equal |  Number, Text, DateTime  | PublishTime <= '2018-8-31' |
| like | contains | Text | 'abc' like '%b%' |
| between | between |  Number, Text, DateTime  | PublishTime between ['2018-1-1','2018-2-1') |

### Logical Operations
> Logical operations must be applied to two expression of the boolean type

| Operator | Function |
| -- | -- |
| And | Logical and |
| Or | Logical or |
| ! | Logical not |

### Property Access Operation

| Operator | Function | Sample |
| -- | -- | -- |
| . | access the property of one object of data | Comments.Author |

> When the operation is applied to a list type, it will project a new list of which each item will be a object that has the specified property.

> If the property name contains the reserved words of Orel such as +,-,.,like etc , you can use \` to wrap the property name to help Orel recognize the real propery name. 
example: \`Size[cm*cm]\`
