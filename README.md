# OREL.Net
An expresson language that can be used to transform and reshape .net object easily.  OREL is short for Object Reshape Expression Language.

## Expression Syntax
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

> If the property name contains the reserved words of Orel such as +,-,.,like etc , you can use ` to wrap the property name to help Orel recognize the real propery name. 
example: \`Size[cm*cm]\`
