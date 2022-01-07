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


