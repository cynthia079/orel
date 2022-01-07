using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orel;
using Orel.Schema;

namespace Orel.Test
{
    [TestClass]
    public class ORELTest
    {
        [TestMethod]
        public void TestTokenization()
        {
            string sql = @" EntranceUrl like null and (Array[1..num((2+3)*3)] = 1) and Array3[4].Field =5 
and ABC between [1,2*(2+3))";
            System.Collections.Generic.List<Token> tokens = Tokenizer.Scan(sql).First();
            TreeBuilder tb = new TreeBuilder();
            tb.AppendRange(tokens);
        }

        [TestMethod]
        public void TestDateTime()
        {
            ORELExecutable exe;
            object result;

            exe = OREL.Compile("now(12)");
            result = exe.Execute();
            Assert.AreEqual(DateTime.UtcNow.Hour + 12, ((DateTimeOffset)result).Hour);

            exe = OREL.Compile("now()");
            result = exe.Execute();
            Assert.AreEqual(DateTime.UtcNow.Hour + 8, ((DateTimeOffset)result).Hour);

            exe = OREL.Compile("date('2018-08-27 12:00:00')");
            result = exe.Execute();
            Assert.AreEqual(12, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(8, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("date('2018-08-27 12:00:00',0)");
            result = exe.Execute();
            Assert.AreEqual(12, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(0, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("today()");
            result = exe.Execute();
            Assert.AreEqual(0, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(0, ((DateTimeOffset)result).Minute);
            Assert.AreEqual(0, ((DateTimeOffset)result).Second);
            Assert.AreEqual(0, ((DateTimeOffset)result).Millisecond);

            exe = OREL.Compile("date2('50分钟前')");
            result = exe.Execute();
            Assert.AreEqual(DateTime.Now.AddMinutes(-50).Hour, ((DateTimeOffset)result).Hour);

            exe = OREL.Compile("date2('30秒前')");
            result = exe.Execute();
            Assert.AreEqual(DateTime.Now.AddSeconds(-30).Minute, ((DateTimeOffset)result).Minute);

            exe = OREL.Compile("date2('昨天')");
            result = exe.Execute();
            Assert.AreEqual(DateTime.Now.AddDays(-1).Day, ((DateTimeOffset)result).Day);
            Assert.AreEqual(8, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("date2('昨天',0)");
            result = exe.Execute();
            Assert.AreEqual(DateTime.UtcNow.AddDays(-1).Day, ((DateTimeOffset)result).Day);
            Assert.AreEqual(0, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("date2('前日5时')");
            result = exe.Execute();
            Assert.AreEqual(DateTime.Now.AddDays(-2).Day, ((DateTimeOffset)result).Day);
            Assert.AreEqual(5, ((DateTimeOffset)result).Hour);

            exe = OREL.Compile("date2('3小时前',0)");
            result = exe.Execute();
            Assert.AreEqual(DateTime.UtcNow.AddHours(-3).Hour, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(0, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("date2('频道主2017年01月25日')");
            result = exe.Execute();
            Assert.AreEqual(25, ((DateTimeOffset)result).Day);
            Assert.AreEqual(0, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(8, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("date2('梨视频[09-09 07:06]')");
            result = exe.Execute();
            Assert.AreEqual(9, ((DateTimeOffset)result).Day);
            Assert.AreEqual(7, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(6, ((DateTimeOffset)result).Minute);
            Assert.AreEqual(8, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("date2('梨视频[9/9 7:6]')");
            result = exe.Execute();
            Assert.AreEqual(9, ((DateTimeOffset)result).Day);
            Assert.AreEqual(7, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(6, ((DateTimeOffset)result).Minute);
            Assert.AreEqual(8, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("date2('梨视频[9-09 17:6:1]')");
            result = exe.Execute();
            Assert.AreEqual(9, ((DateTimeOffset)result).Day);
            Assert.AreEqual(17, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(6, ((DateTimeOffset)result).Minute);
            Assert.AreEqual(8, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("date2('梨视频[9月09日 17：6：1]')");
            result = exe.Execute();
            Assert.AreEqual(9, ((DateTimeOffset)result).Day);
            Assert.AreEqual(17, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(6, ((DateTimeOffset)result).Minute);
            Assert.AreEqual(8, ((DateTimeOffset)result).Offset.Hours);

            exe = OREL.Compile("date2('梨视频[18年9月09日 17：6：1]')");
            result = exe.Execute();
            Assert.AreEqual(9, ((DateTimeOffset)result).Day);
            Assert.AreEqual(17, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(6, ((DateTimeOffset)result).Minute);
            Assert.AreEqual(8, ((DateTimeOffset)result).Offset.Hours);
        }

        [TestMethod]
        public void TestMoreDateTime()
        {
            var exe = OREL.Compile("date2('Sat Jul 06 23:55:35 +0800 2019')");
            var result = exe.Execute();
            Assert.AreEqual(6, ((DateTimeOffset)result).Day);
            Assert.AreEqual(23, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(2019, ((DateTimeOffset)result).Year);
            Assert.AreEqual(8, ((DateTimeOffset)result).Offset.Hours);
            Assert.AreEqual(7, ((DateTimeOffset)result).Month);

            exe = OREL.Compile("date('Sat Jul 06 23:55:35 +08:00 2019', 7)");
            result = exe.Execute();
            Assert.AreEqual(6, ((DateTimeOffset)result).Day);
            Assert.AreEqual(22, ((DateTimeOffset)result).Hour);
            Assert.AreEqual(2019, ((DateTimeOffset)result).Year);
            Assert.AreEqual(7, ((DateTimeOffset)result).Offset.Hours);
            Assert.AreEqual(7, ((DateTimeOffset)result).Month);

            exe = OREL.Compile("date('2018-03-01T06:41:24Z')");
            result = exe.Execute();
        }

        [TestMethod]
        public void TestArithmetic()
        {
            LambdaExpression lamda = BuildQuery("3*4*5*6*7+1.5");
            Delegate func = lamda.Compile();
            object result = func.DynamicInvoke();
            Assert.AreEqual(2521.5m, result);

            lamda = BuildQuery("3.1415926-4*5+6");
            func = lamda.Compile();
            result = func.DynamicInvoke();
            Assert.AreEqual(-10.8584074m, result);

            lamda = BuildQuery("3.1-4.2*5.3+6.4");
            func = lamda.Compile();
            result = func.DynamicInvoke();
            Assert.AreEqual(-12.76m, result);

            lamda = BuildQuery("-3.1");
            func = lamda.Compile();
            result = func.DynamicInvoke();
            Assert.AreEqual(-3.1m, result);

            try
            {
                lamda = BuildQuery("+3.1");
                Assert.Fail();
            }
            catch (InvalidOperatorException)
            {
            }

            try
            {
                lamda = BuildQuery("*3.1");
                Assert.Fail();
            }
            catch (InvalidOperatorException)
            {
            }

            try
            {
                lamda = BuildQuery("/3.1");
                Assert.Fail();
            }
            catch (InvalidOperatorException)
            {
            }


            try
            {
                lamda = BuildQuery("-3.1.2");
                Assert.Fail();
            }
            catch (InvalidTokenException e)
            {
                Assert.AreEqual("3.1.2 是无效的符号", e.Message);
            }

            try
            {
                lamda = BuildQuery("-3.a");
                Assert.Fail();
            }
            catch (InvalidTokenException e)
            {
                Assert.AreEqual("a 是无效的符号", e.Message);
            }

            lamda = BuildQuery("3*4*5-6*2");
            func = lamda.Compile();
            result = func.DynamicInvoke();
            Assert.AreEqual(48m, result);

            lamda = BuildQuery("3+4*5+6*7+10");
            func = lamda.Compile();
            result = func.DynamicInvoke();
            Assert.AreEqual(75m, result);
        }

        [TestMethod]
        public void TestComparer()
        {
            Delegate d = CompileQuery("3+4=7");
            object result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("3*4+4*2=2+3*6");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("3*4-4*2>2+3*6");
            result = d.DynamicInvoke();
            Assert.AreEqual(false, result);

            d = CompileQuery("3*4-4*2<=4");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("3+4-4*2<=4/1+2");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

        }

        [TestMethod]
        public void TestLogic()
        {
            Delegate d = CompileQuery("3+4=7 and 3*4-4*2<=4");
            object result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("3+4=7 and 3*4-4*2<=4 and 1=0");
            result = d.DynamicInvoke();
            Assert.AreEqual(false, result);

            d = CompileQuery("3+4=7 and 3*4-4*2<=4 or 1=0");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("3+4>7 or 3*4-4*2=4 or 1=0");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("3+4>7 or 3*4-4*2<4 or 1=0 and 1=1");
            result = d.DynamicInvoke();
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void TestBlocks()
        {
            Delegate d;
            object result;
            d = CompileQuery("3+4>7 or 3*(4-4)*2<4 or 1=0 and 1=1");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("4+1=6 and (7=0 or 8+1=9)");
            result = d.DynamicInvoke();
            Assert.AreEqual(false, result);

            d = CompileQuery("(4+1=5 and 1+4*2=9) or (6=0 and 9*(8+1)=81) and 1=1");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("4+1=5 and (1+4*2=9 and (6=0 or 9*(8+1)=81)) and 1=1");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestString()
        {
            Delegate d = CompileQuery(@"'abc\''");
            object result = d.DynamicInvoke();
            Assert.AreEqual("abc'", result);
            //单引号转义
            d = CompileQuery(@"'ab\'c'+'\'def' + 'ghi\''");
            result = d.DynamicInvoke();
            Assert.AreEqual("ab'c'defghi'", result);
            //双引号转义
            d = CompileQuery(@"""ab\""c""+""\""def"" + ""ghi\""""");
            result = d.DynamicInvoke();
            Assert.AreEqual("ab\"c\"defghi\"", result);
            //引号不一致，不执行转义(单)
            d = CompileQuery(@"""ab\'c""+""\'def"" + ""ghi\'""");
            result = d.DynamicInvoke();
            Assert.AreEqual(@"ab\'c\'defghi\'", result);
            //引号不一致，不执行转义(双)
            d = CompileQuery(@"'ab\""c'+'\""def' + 'ghi\""'");
            result = d.DynamicInvoke();
            Assert.AreEqual(@"ab\""c\""defghi\""", result);

            d = CompileQuery("'abc'+'def' + 'ghi'='abcdefghi'");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("'abc' like '%b%'");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("'Author1' like 'Author%' and 'abc' like '%bc'");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("'Author1' like '%'");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("'Author1' like ''");
            result = d.DynamicInvoke();
            Assert.AreEqual(false, result);

            d = CompileQuery("'abc' like 'ABC'");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("'abc' = 'ABC'");
            result = d.DynamicInvoke();
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void TestNegative()
        {
            Delegate d = CompileQuery("-1=0-1");
            object result = d.DynamicInvoke();
            Assert.AreEqual(true, result);

            d = CompileQuery("-1*2");
            result = d.DynamicInvoke();
            Assert.AreEqual(-2m, result);

            d = CompileQuery("-3*-4+-1*2");
            result = d.DynamicInvoke();
            Assert.AreEqual(10m, result);

            d = CompileQuery("-3*(-4+-1)*2 = 30 and (-3*-4+-1*2)=10");
            result = d.DynamicInvoke();
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestMethod()
        {
            ORELExecutable exe;
            object result;

            exe = OREL.Compile("3+num('1-2-3')");
            result = exe.Execute();
            Assert.AreEqual(null, result);

            exe = OREL.Compile("3+num('123')");
            result = exe.Execute();
            Assert.AreEqual(126m, result);

            exe = OREL.Compile("num('123')*3");
            result = exe.Execute();
            Assert.AreEqual(369m, result);

            exe = OREL.Compile("(num('123')*3 = 369) and (3+num('12'+'3') = 126)");
            result = exe.Execute();
            Assert.AreEqual(true, result);

            exe = OREL.Compile("date_fmt(now(),'yyyyMMdd')");
            result = exe.Execute();
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd"), result);

            exe = OREL.Compile("text(123)");
            result = exe.Execute();
            Assert.AreEqual("123", result);

            exe = OREL.Compile("text(date('2018-12-31'))");
            result = exe.Execute();
            Assert.AreEqual("2018/12/31 0:00:00 +08:00", result);
        }

        [TestMethod]
        public void TestBetween()
        {
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("Resources\\Json\\json1.json"));
            MemberDefinition[] members = new[]
            {
                 new MemberDefinition("Data", DataType.Object ),
                 new MemberDefinition("Page", DataType.Number,"Data" ),
                 new MemberDefinition("Time", DataType.DateTime,"Data" ),
                 new MemberDefinition("CommentCount", DataType.Number,"Data" ),
            };

            ORELExecutable exe;
            object result;

            exe = OREL.Compile("Page between [1,8)", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("2 between (2,8]", members);
            result = exe.Execute(obj);
            Assert.AreEqual(false, result);

            exe = OREL.Compile("Time between('2017-1-1','2018-1-1') and CommentCount between[1,10]", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestNot()
        {
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("Resources\\Json\\json1.json"));
            MemberDefinition[] members = new[]
            {
                 new MemberDefinition("Data", DataType.Object ),
                 new MemberDefinition("Page", DataType.Number ),
                 new MemberDefinition("CommentCount", DataType.Number, "Data" ),
                 new MemberDefinition("Time", DataType.DateTime, "Data" ),
                 new MemberDefinition("Comments",  DataType.List, "Data")
            };

            ORELExecutable exe;
            object result;

            exe = OREL.Compile("!1=1");
            result = exe.Execute();
            Assert.AreEqual(false, result);

            exe = OREL.Compile("!1=1 or 2=2");
            result = exe.Execute();
            Assert.AreEqual(true, result);

            exe = OREL.Compile("!(1=1 or 2=2)");
            result = exe.Execute();
            Assert.AreEqual(false, result);

            exe = OREL.Compile("!Time between('2017-1-1','2018-1-1') and CommentCount between[1,10]", members);
            result = exe.Execute(obj);
            Assert.AreEqual(false, result);

            exe = OREL.Compile("Time between('2017-1-1','2018-1-1') and !CommentCount between[11,12]", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("4+1=5 and (1+4*2=9 and !(6=0 or 9*(8+1)=81)) and 1=1", members);
            result = exe.Execute(obj);
            Assert.AreEqual(false, result);

            exe = OREL.Compile("4+1=5 and (1+4*2=9 and (!6=0 or 9*(8+1)=81)) and 1=1", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestObject()
        {
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("Resources\\Json\\json1.json"));
            MemberDefinition[] members = new[]
            {
                
                 new MemberDefinition("Data", DataType.Object ),
                 new MemberDefinition("Title", DataType.Text, "Data" ),
                 new MemberDefinition("Page", DataType.Number, "Data" ),
                 new MemberDefinition("CommentCount", DataType.Number, "Data" ),
                 new MemberDefinition("Time", DataType.DateTime, "Data" ),
                 new MemberDefinition("Comments",  DataType.List, "Data"),
                 new MemberDefinition("ClientType",  DataType.Number),
                 new MemberDefinition("ClientType1",  DataType.Number),
                 new MemberDefinition("自定义",  DataType.Number, "Data"),
                 new MemberDefinition("数字翻页",  DataType.Number, "Data"),
            };

            ORELExecutable exe;
            object result;

            //属性不存在于预定义字段中
            try
            {
                exe = OREL.Compile("Page1 between [1,8)", members);
                result = exe.Execute(obj);
                Assert.AreEqual(true, result);
            }
            catch (InvalidMemberNameException)
            {
                //throw;
            }

            //属性的值为null
            exe = OREL.Compile("ClientType = null", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("数字翻页 = null", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            //存在于预定义字段，但是不存在于数据中
            exe = OREL.Compile("ClientType1 = null", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("自定义 = null", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Title like '宝骏310%' ", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Data.Page", members);
            result = exe.Execute(obj);
            Assert.AreEqual(2m, result);

            exe = OREL.Compile("CommentCount > 7", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("CommentCount = '8'", members);   //非文本类型数据和文本关联操作时，视作文本类型
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("CommentCount+1", members);
            result = exe.Execute(obj);
            Assert.AreEqual(9m, result);

            exe = OREL.Compile("CommentCount>Page", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("CommentCount+'1'", members);  //非文本类型数据和文本关联操作时，视作文本类型
            result = exe.Execute(obj);
            Assert.AreEqual("81", result);

            exe = OREL.Compile("date_part(Time,'y') = 2017", members);  //日期类型隐式转换
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Time >= '2017-9-18' and Time < '2017-9-19' ", members);  //日期类型隐式转换
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("date_fmt(Time+'1d2h','yyyyMMddHHmmss') = '20170919140000'", members);  //日期类型隐式转换
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("date('2018-01-01') + '1d' = '2018-1-2'", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("date('2018-01-01') - '1d' = '2017-12-31'", members);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestArray()
        {
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("Resources\\Json\\json1.json"));
            MemberDefinition[] members = new[]
            {
                 new MemberDefinition("EntranceUrl", DataType.Text ),
                 new MemberDefinition("Page", DataType.Number ),
                 new MemberDefinition("Data", DataType.Object ),
                 new MemberDefinition("CommentCount", DataType.Number, "Data" ),
                 new MemberDefinition("CrawlTime", DataType.DateTime ),
                 new MemberDefinition("Comments",  DataType.List, "Data"),
                 new MemberDefinition("LikedCount",  DataType.Number, "Data.Comments"),
                 new MemberDefinition("Content",  DataType.Text, "Data.Comments")
            };
            var desc = SchemaProvider.FromMemberDefinitions(members, "Data");
            ORELExecutable exe;
            object result;
            IList list;

            exe = OREL.Compile("Comments[LikedCount=4 and Content!=''][1..2]", desc);
            result = exe.Execute(obj);
            list = result as IList;
            Assert.AreEqual(2, list.Count);

            exe = OREL.Compile("Data.Comments[LikedCount=4][1..2]", desc);
            result = exe.Execute(obj);
            list = result as IList;
            Assert.AreEqual(2, list.Count);

            exe = OREL.Compile("Comments[LikedCount=4][1].Content", desc);
            result = exe.Execute(obj);
            Assert.AreEqual("这个配置真的神级车了", result);

            exe = OREL.Compile("Comments[LikedCount=4][2].Content='下月哈弗就危险了'", desc);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Data.Comments[LikedCount=4][2].Content='下月哈弗就危险了'", desc);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("len(Comments)=8", desc);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("len(Data.Comments)=8", desc);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Comments.LikedCount", desc);
            result = exe.Execute(obj);
            list = result as IList;
            Assert.AreEqual(8, list.Count);
            Assert.AreEqual("1", list[0].ToString());

            exe = OREL.Compile("Data.Comments.LikedCount", desc);
            result = exe.Execute(obj);
            list = result as IList;
            Assert.AreEqual(8, list.Count);
            Assert.AreEqual("1", list[0].ToString());

            exe = OREL.Compile("Comments[1..2]", desc);
            result = exe.Execute(obj);
            list = result as IList;
            Assert.AreEqual(2, list.Count);

            exe = OREL.Compile("Comments[5..]", desc); //不设置结束
            result = exe.Execute(obj);
            list = result as IList;
            Assert.AreEqual(4, list.Count);

            exe = OREL.Compile("num(Comments[1].LikedCount)=1", desc);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Comments[LikedCount=4]", desc);
            result = exe.Execute(obj);
            list = result as IList;
            Assert.AreEqual(4, list.Count);

            exe = OREL.Compile("Comments[LikedCount=4].Content[1]", desc);
            result = exe.Execute(obj);
            Assert.AreEqual("这个配置真的神级车了", result.ToString());
        }

        [TestMethod]
        public void TestPrecompile()
        {
            ORELExecutable exe;
            object result;

            var today = IntrinsicFunctions.Today();
            var text = OREL.Precompile("$today()");
            Assert.AreEqual($"date('{today.Value.ToString("yyyy-MM-dd HH:mm:ss")}')", text);
            exe = OREL.Compile(text);
            result = exe.Execute();
            Assert.AreEqual(today, result);

            today = IntrinsicFunctions.Today(-12);
            text = OREL.Precompile("$today(-12)");
            Assert.AreEqual($"date('{today.Value.ToString("yyyy-MM-dd HH:mm:ss")}',-12)", text);
            exe = OREL.Compile(text);
            result = exe.Execute();
            Assert.AreEqual(today, result);

            today = IntrinsicFunctions.Now();
            text = OREL.Precompile("$now()");
            Assert.AreEqual($"date('{today.Value.ToString("yyyy-MM-dd HH:mm:ss")}')", text);
            exe = OREL.Compile(text);
            result = exe.Execute();
            Assert.IsTrue((today.Value - (DateTimeOffset)result).TotalSeconds <= 1);

            today = IntrinsicFunctions.Now();
            text = OREL.Precompile("$now(8)");
            Assert.AreEqual($"date('{today.Value.ToString("yyyy-MM-dd HH:mm:ss")}',8)", text);
            exe = OREL.Compile(text);
            result = exe.Execute();
            Assert.IsTrue((today.Value - (DateTimeOffset)result).TotalSeconds <= 1);

            today = IntrinsicFunctions.Today();
            text = OREL.Precompile("PublishTime between [$today(),$today()+'1d')");
            var dateStr = today.Value.ToString("yyyy-MM-dd HH:mm:ss");
            Assert.AreEqual($"`PublishTime` between [date('{dateStr}'),date('{dateStr}') + '1d')", text);

            //测试含有'符号的文本
            text = OREL.Precompile(@"'Publish\'Time'");
            Assert.AreEqual(@"'Publish\'Time'", text);
        }

        [TestMethod]
        public void TestORELJsonConvert()
        {
            var txt = File.ReadAllText("Resources\\Json\\json2.json");
            MemberDefinition[] members = new[]
            {
                 new MemberDefinition("EntranceUrl", DataType.Text ),
                 new MemberDefinition("Page", DataType.Number ),
                 new MemberDefinition("Data", DataType.Object ),
                 new MemberDefinition("CommentCount", DataType.Number, "Data" ),
                 new MemberDefinition("CrawlTime", DataType.DateTime ),
                 new MemberDefinition("Comments",  DataType.List, "Data"),
                 new MemberDefinition("Likecount",  DataType.Number, "Data.Comments"),
                 new MemberDefinition("Content",  DataType.Text, "Data.Comments"),
                 new MemberDefinition("Author",  DataType.Text, "Data.Comments")
            };
            var descriptor = new DefaultMemberDescriptor(members);

            var settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ORELJsonConverter(descriptor) },
                Formatting = Formatting.Indented
            };

            dynamic obj = JsonConvert.DeserializeObject<ORELObject>(txt, settings);

            ORELExecutable exe;
            object result;

            exe = OREL.Compile("len(Comments)=2", descriptor);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Comments[1].likeCount", descriptor);
            result = exe.Execute(obj);
            Assert.AreEqual(obj.Data.Comments[0].Likecount, result.ToString());

            exe = OREL.Compile("comments[1].content", descriptor);
            result = exe.Execute(obj);
            Assert.AreEqual(obj.Data.Comments[0].Content, result);

            exe = OREL.Compile("comments[1].Author", descriptor);
            result = exe.Execute(obj);
            Assert.AreEqual(obj.Data.Comments[0].Author, result);

            exe = OREL.Compile("commentcount", descriptor);
            result = exe.Execute(obj);
            Assert.AreEqual(obj.Data.CommentCount, result.ToString());

            exe = OREL.Compile("commentcount", descriptor);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(8m, result);

            exe = OREL.Compile("-commentcount", descriptor);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(-8m, result);
        }

        [TestMethod]
        public void TestSelectMany()
        {
            var desc = new List<MemberDefinition>();
            desc.Add(new MemberDefinition("Data", DataType.Object));
            desc.Add(new MemberDefinition("Comments", DataType.List, "Data"));
            desc.Add(new MemberDefinition("LikedCount", DataType.Number, "Data.Comments"));
            desc.Add(new MemberDefinition("Content", DataType.Text, "Data.Comments"));
            desc.Add(new MemberDefinition("Author", DataType.Text, "Data.Comments"));
            desc.Add(new MemberDefinition("CreateTime", DataType.DateTime, "Data.Comments"));
            desc.Add(new MemberDefinition("Replies", DataType.List, "Data.Comments"));
            desc.Add(new MemberDefinition("Author", DataType.Text, "Data.Comments.Replies"));
            desc.Add(new MemberDefinition("Content", DataType.Text, "Data.Comments.Replies"));
            desc.Add(new MemberDefinition("Label", DataType.List, "Data.Comments.Replies"));
            desc.Add(new MemberDefinition("Name", DataType.Text, "Data.Comments.Replies.Label"));
            desc.Add(new MemberDefinition("Content", DataType.Text, "Data.Comments.Replies.Label"));
            desc.Add(new MemberDefinition("Label", DataType.List, "Data.Comments"));
            desc.Add(new MemberDefinition("Name", DataType.Text, "Data.Comments.Label"));
            desc.Add(new MemberDefinition("Content", DataType.Text, "Data.Comments.Label"));

            var txt = File.ReadAllText("Resources\\Json\\json3.json");

            ORELExecutable exe;
            object result;

            exe = OREL.Compile("Comments", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(8, (result as IList).Count);

            exe = OREL.Compile("Comments.LikedCount", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(8, (result as IList).Count);

            exe = OREL.Compile("Comments.Replies", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(6, (result as IList).Count);

            exe = OREL.Compile("Comments.Replies.Label", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(10, (result as IList).Count);

            exe = OREL.Compile("Comments.Label", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(5, (result as IList).Count);
        }

        [TestMethod]
        public void TestMemberDescriptor()
        {
            var desc = SchemaProvider.Empty();
            desc.Add(new MemberDefinition("Data", DataType.Object));
            desc.Add(new MemberDefinition("Comments", DataType.List, "Data"));
            MemberDefinition def2 = desc.Add(new MemberDefinition("LikedCount", DataType.Number, "Data.Comments"));
            Assert.AreEqual("Data.Comments.LikedCount", def2.UniqueName);
            Assert.AreEqual("LikedCount", def2.ActualName);
            var def1 = desc.Add(new MemberDefinition("Replies", DataType.List, "Data.Comments"));
            def2 = desc.Add(new MemberDefinition("Author", DataType.Text, "Data.Comments.Replies"));
            Assert.AreEqual("Data.Comments.Replies", def1.UniqueName);
            Assert.AreEqual("Replies", def1.ActualName);
            Assert.AreEqual("Data.Comments.Replies.Author", def2.UniqueName);
            Assert.AreEqual("Author", def2.ActualName);
        }

        [TestMethod]
        public void TestWriteJson()
        {
            var txt = File.ReadAllText("Resources\\Json\\json1.json");
            MemberDefinition[] members = new[]
            {
                 new MemberDefinition("EntranceUrl", DataType.Text ),
                 new MemberDefinition("Page", DataType.Number ),
                 new MemberDefinition("Data", DataType.Object ),
                 new MemberDefinition("CommentCount", DataType.Number, "Data" ),
                 new MemberDefinition("CrawlTime", DataType.DateTime ),
                 new MemberDefinition("Comments",  DataType.List, "Data"),
                 new MemberDefinition("LikedCount",  DataType.Number, "Data.Comments"),
                 new MemberDefinition("Content",  DataType.Text, "Data.Comments")
            };
            var descriptor = SchemaProvider.FromMemberDefinitions(members, "Data");

            var settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ORELJsonConverter(descriptor) },
                Formatting = Formatting.Indented,
            };

            dynamic obj = JsonConvert.DeserializeObject<ORELObject>(txt, settings);
            string serialized = JsonConvert.SerializeObject(obj, settings);
            serialized = JsonConvert.SerializeObject(1, settings);
            serialized = JsonConvert.SerializeObject(1m, settings);
            serialized = JsonConvert.SerializeObject(null, settings);
            decimal? a = null;
            serialized = JsonConvert.SerializeObject(a, settings);
            a = 1.2m;
            serialized = JsonConvert.SerializeObject(a, settings);
            serialized = JsonConvert.SerializeObject(new List<int>() { 1, 2, 3, 4 }, settings);
            serialized = JsonConvert.SerializeObject(new { A = 1, B = 2, C = 3, D = 4 }, settings);
            serialized = JsonConvert.SerializeObject("abc", settings);


            settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ORELJsonWriter() },
                Formatting = Formatting.Indented,
            };
            serialized = JsonConvert.SerializeObject(obj, settings);
            serialized = JsonConvert.SerializeObject(1, settings);
            serialized = JsonConvert.SerializeObject(1m, settings);
            serialized = JsonConvert.SerializeObject(null, settings);
            a = null;
            serialized = JsonConvert.SerializeObject(a, settings);
            a = 1.2m;
            serialized = JsonConvert.SerializeObject(a, settings);
            serialized = JsonConvert.SerializeObject(new List<int>() { 1, 2, 3, 4 }, settings);
            serialized = JsonConvert.SerializeObject(new { A = 1, B = 2, C = 3, D = 4 }, settings);
            serialized = JsonConvert.SerializeObject("abc", settings);
            serialized = JsonConvert.SerializeObject(new { Outside = "111", Data = obj }, settings);

            var exe = OREL.Compile("{ Field1: EntranceUrl, Page: Page+1, Comments:Data.Comments}", descriptor);
            var obj2 = exe.ExecuteJson(txt);
            serialized = JsonConvert.SerializeObject(obj2, settings);
        }

        [TestMethod]
        public void TestMemberQuote()
        {
            var txt = File.ReadAllText("Resources\\Json\\json4.json");
            MemberDefinition[] members = new[]
            {
                 new MemberDefinition("Date(yyyy/MM/dd)", DataType.DateTime ),
                 new MemberDefinition("List.Page", DataType.Number ),
                 new MemberDefinition("Title-Default", DataType.Text ),
                 new MemberDefinition("List[Links]", DataType.List ),
            };
            var text = OREL.Precompile("`Date(yyyy/MM/dd)` = '2018-1-1'");
            Assert.AreEqual("`Date(yyyy/MM/dd)` = '2018-1-1'", text);
            var exe = OREL.Compile(text, members);
            var result = exe.ExecuteJson(txt);
            Assert.IsTrue((bool)result);

            exe = OREL.Compile("1=`List.Page`", members);
            result = exe.ExecuteJson(txt);
            Assert.IsTrue((bool)result);

            exe = OREL.Compile("`Title-Default` like 'Title'", members);
            result = exe.ExecuteJson(txt);
            Assert.IsTrue((bool)result);

            exe = OREL.Compile("len(`List[Links]`)=3", members);
            result = exe.ExecuteJson(txt);
            Assert.IsTrue((bool)result);
        }

        [TestMethod]
        public void TestDefaultArgument()
        {
            var desc = GetDescriptor3();

            var txt = File.ReadAllText("Resources\\Json\\json3.json");

            ORELExecutable exe;
            object result;

            exe = OREL.Compile("Comments.LikedCount[_ <= 2]", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(3, (result as IList).Count);

            exe = OREL.Compile("Comments.Replies.Label.Name[_ like '%5-1%']", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(2, (result as IList).Count);
        }

        [TestMethod]
        public void TestTransformTo()
        {
            var desc = GetDescriptor3();

            var txt = File.ReadAllText("Resources\\Json\\json3.json");

            ORELExecutable exe;
            IList list;

            exe = OREL.Compile("Comments.LikedCount => _*2", desc);
            list = exe.ExecuteJson(txt) as IList;
            Assert.AreEqual(2m, list[0]);
            Assert.AreEqual(4m, list[1]);
            Assert.AreEqual(6m, list[3]);

            exe = OREL.Compile("Comments => LikedCount*2", desc);
            list = exe.ExecuteJson(txt) as IList;
            Assert.AreEqual(2m, list[0]);
            Assert.AreEqual(4m, list[1]);
            Assert.AreEqual(6m, list[3]);

            exe = OREL.Compile("Comments => _.LikedCount*2", desc);

            list = exe.ExecuteJson(txt) as IList;
            Assert.AreEqual(2m, list[0]);
            Assert.AreEqual(4m, list[1]);
            Assert.AreEqual(6m, list[3]);

            exe = OREL.Compile("Comments.LikedCount => _*2 => _ + 3", desc);
            list = exe.ExecuteJson(txt) as IList;
            Assert.AreEqual(5m, list[0]);
            Assert.AreEqual(7m, list[1]);
            Assert.AreEqual(9m, list[3]);

            exe = OREL.Compile("Comments.Content => len(_)+1", desc);
            list = exe.ExecuteJson(txt) as IList;
            Assert.AreEqual(25m, list[0]);
            Assert.AreEqual(19m, list[1]);
            Assert.AreEqual(5m, list[2]);

            exe = OREL.Compile("Comments => replace(date_fmt(CreateTime,'yyyy年MM月dd日HH时mm分'),'年','year')+LikedCount", desc);
            list = exe.ExecuteJson(txt) as IList;
            Assert.AreEqual("2017year09月16日10时45分1", list[0]);
            Assert.AreEqual("2017year09月16日10时19分2", list[1]);
            Assert.AreEqual("2017year09月15日20时45分2", list[2]);
        }

        [TestMethod]
        public void TestBuildObject()
        {
            var desc = GetDescriptor3();
            var txt = File.ReadAllText("Resources\\Json\\json3.json");

            ORELExecutable exe;

            exe = OREL.Compile("{Comments[1].Replies[1].Label[1].Name}", desc);
            dynamic result = exe.ExecuteJson(txt);
            Assert.AreEqual("CommentsRepliesLabelName1-1-1", result.Name);
            //Assert.AreEqual(2m, result.B);

            exe = OREL.Compile("{A:1=1, B:2}");
            result = exe.Execute();
            Assert.AreEqual(true, result.A);
            Assert.AreEqual(2m, result.B);

            exe = OREL.Compile("{A:Page=1, B:2}", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(false, result.A);
            Assert.AreEqual(2m, result.B);

            exe = OREL.Compile("Comments=>{likecount:LikedCount*10, content:_.Author+':'+Content, CreateTime, LikedCount+1,Replies}", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(10m, result[0].likecount);
            Assert.AreEqual("韦石泉:外观方面，XT5新车型将与现款XT5基本保持一致", result[0].content);
            Assert.AreEqual(new DateTimeOffset(2017, 9, 16, 10, 45, 0, 0, TimeSpan.FromHours(8)), result[0].CreateTime);
            Assert.AreEqual(2m, result[0]._4);
            Assert.AreEqual(2, result[0].Replies.Count);
        }

        [TestMethod]
        public void TestLoopVariable()
        {
            var desc = GetDescriptor3();

            var txt = File.ReadAllText("Resources\\Json\\json3.json");

            ORELExecutable exe;

            exe = OREL.Compile("Comments=> { LikedCount, index: $i }", desc);
            dynamic result = exe.ExecuteJson(txt);
            Assert.AreEqual(2, result[1].index);

            exe = OREL.Compile("Comments=> { LikedCount, index: $i }=>{'a'+$i}", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual("a4", result[3]._1);

            exe = OREL.Compile("Comments=> { LikedCount, index: $i }=>{1+$i}", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(5, result[3]._1);
        }

        [TestMethod]
        public void TestCompose()
        {
            var desc = GetDescriptor3();

            var txt = File.ReadAllText("Resources\\Json\\json3.json");

            ORELExecutable exe;
            dynamic result;

            //exe = OREL.Compile(@"Comments|{Page,Title}                            
            //                =>{ Page,Title,Content }", desc);
            //result = exe.ExecuteJson(txt);

            //exe = OREL.Compile(@"Comments|{Page,Title}
            //                =>{ Page,Title,Content }
            //                =>{ Page,Title,Content }", desc);
            //result = exe.ExecuteJson(txt);

            //Assert.AreEqual("宝骏310自动挡配置曝光 预售5万元起", result[0].Title);
            //Assert.AreEqual("宝骏310自动挡配置曝光 预售5万元起", result[1].Title);
            //Assert.AreEqual("外观方面，XT5新车型将与现款XT5基本保持一致", result[0].Content);
            //Assert.AreEqual("斯威7自动挡预售价发布 成都车展上市", result[1].Content);

            var obj = JsonConvert.DeserializeObject<dynamic>(txt);
            exe = OREL.Compile(@"(Comments.Replies|{title:Title, Page: 1})
                            =>{title,Page,Author, a:Author+'-1'}
                            =>{title,Page,Author,a}", desc);

            //var sw = new Stopwatch();
            //sw.Start();
            //for (int i = 0; i < 10000; i++)
            {
                result = exe.Execute(obj);
            }
            //sw.Stop();
            //Console.WriteLine("execute time:" + sw.ElapsedMilliseconds);
            Assert.AreEqual("宝骏310自动挡配置曝光 预售5万元起", result[0].title.ToString());
            Assert.AreEqual("宝骏310自动挡配置曝光 预售5万元起", result[1].title.ToString());
            Assert.AreEqual(1m, result[0].Page);
            Assert.AreEqual(1m, result[1].Page);
            Assert.AreEqual("Author1-1-1", result[0].a);
            Assert.AreEqual("Author1-2-1", result[1].a);

            exe = OREL.Compile("Comments|{ date: now() }", desc);
            result = exe.Execute(obj);
            Assert.AreEqual("1", result[0].LikedCount.ToString());
            Assert.AreEqual("全是石头路啊", result[1].Author.ToString());
            Assert.IsNotNull(result[1].date.ToString());
        }

        [TestMethod]
        public void TestIfElse()
        {

            ORELExecutable exe;
            dynamic result;

            exe = OREL.Compile(@"if(1=1,'abc',1)");
            result = exe.Execute();
            Assert.AreEqual("abc", result);

            exe = OREL.Compile(@"if(1!=1,'abc',if(1=1,'def'+'efg','1'))");
            result = exe.Execute();
            Assert.AreEqual("defefg", result);

            var desc = GetDescriptor3();
            var txt = File.ReadAllText("Resources\\Json\\json3.json");

            exe = OREL.Compile(@"Comments=>{ a:if(LikedCount>=4, 'more like','less like' ) }", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual("less like", result[0].a);
            Assert.AreEqual("less like", result[1].a);
            Assert.AreEqual("less like", result[2].a);
            Assert.AreEqual("less like", result[3].a);
            Assert.AreEqual("more like", result[4].a);
            Assert.AreEqual("more like", result[5].a);
            Assert.AreEqual("more like", result[6].a);
            Assert.AreEqual("more like", result[7].a);
        }

        [TestMethod]
        public void TestIfElseAddition()
        {
            var text = File.ReadAllText("Resources\\Json\\json7.json");
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);
            var js = new JsonSerializerSettings() { Converters = new[] { new ORELJsonConverter(schema) } };
            var obj2 = JsonConvert.DeserializeObject<ORELObject>(text, js);

            //var schema = SchemaProvider.FromType<InternalClass>();
            //var obj3 = new InternalClass() { Name = "", Number = 22, Date = DateTime.Now , SubClasses = new SubClass[] { new SubClass() { Key="1" } } };

            var exe = OREL.Compile("if(data.title='',data.fav_count,data.id)", schema);
            var result = exe.Execute(obj2);
            Assert.AreEqual(1350L, result);

            exe = OREL.Compile("if(data.inlikes, 1, 2)", schema);
            result = exe.Execute(obj2);
            Assert.AreEqual(2m, result);

            exe = OREL.Compile("if(data.images_list[2].flag,data.fav_count,data.id)", schema);
            result = exe.Execute(obj2);
            Assert.AreEqual("5ce3837c000000000f024d7c", result);
        }

        [TestMethod]
        public void TestSchemaProviderStatic()
        {
            //FromStatic                     
            var testObj = new InternalClass
            {
                Name = "Name1",
                Number = -1,
                Date = DateTime.Now,
                Array = new[] { 1, 2, 3, 4 },
                SubClasses = new SubClass[]
                {
                    new SubClass{Key="Key1",Value = DateTimeOffset.Now },
                    new SubClass{Key="Key2",Value = DateTimeOffset.Now },
                    new SubClass{Key="Key2",Value = DateTimeOffset.Now }
                }
            };
            var schema = SchemaProvider.FromType<InternalClass>();
            var exe = OREL.Compile(@"Name = 'Name1'", schema);
            dynamic result = exe.Execute(testObj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile(@"SubClasses", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(true, result is IEnumerable<SubClass>);

            exe = OREL.Compile(@"SubClasses=>{Name:Key, Index:$i}", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Key1", result[0].Name);
            Assert.AreEqual(2, result[1].Index);

            exe = OREL.Compile(@"Array[_>1]", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(3, result[1]);

            exe = OREL.Compile(@"Array=>{_+1}", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(2, result[0]._1);
            Assert.AreEqual(3, result[1]._1);

            exe = OREL.Compile("date_fmt(Date,'yyyyMMdd')", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd"), result);

            exe = OREL.Compile("Date >'2019-1-1'", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("date_fmt(SubClasses[1].Value,'yyyyMMdd')", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(DateTime.Now.ToString("yyyyMMdd"), result);

            exe = OREL.Compile("SubClasses[1].Value>'2019-1-1'", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("date_fmt(date('2019-12-12'),'yyyyMMdd')", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual("20191212", result);
        }


        [TestMethod]
        public void TestSchemaProviderDynamicMix()
        {
            //from JObject
            var txt = File.ReadAllText("Resources\\Json\\json3.json");
            dynamic testObj = JsonConvert.DeserializeObject<JObject>(txt);

            SchemaProvider schema = SchemaProvider.FromObject(testObj, "Data");
            var exe = OREL.Compile(@"{ RequestId2:RequestId }", schema);

            var result = exe.Execute(testObj);
            SchemaProvider newSchema = SchemaProvider.FromObject(result);
            exe = OREL.Compile(@"RequestId2", newSchema);
            result = exe.Execute(result);
            Assert.AreEqual(12345, result);

            txt = File.ReadAllText("Resources\\Json\\json11.json");
            testObj = JsonConvert.DeserializeObject<JObject>(txt);
            schema = SchemaProvider.FromObject(testObj);

            exe = OREL.Compile("data.data.notes[!noe(id)].id", schema);
            result = exe.Execute(testObj);

            var subSchema = (SchemaProvider)SchemaProvider.FromObject(result[0]);
            Assert.AreEqual(false, subSchema.Any());

            var stuct = new Struct() { Name = "1", Value = "2" };
            schema = SchemaProvider.FromObject(stuct);
            exe = OREL.Compile("Name", schema);
            result = exe.Execute(stuct);
            Assert.AreEqual("1", result);
        }

        private struct Struct
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        [TestMethod]
        public void TestSchemaProviderDynamic()
        {
            //from JObject
            var txt = File.ReadAllText("Resources\\Json\\json3.json");
            dynamic testObj = JsonConvert.DeserializeObject<JObject>(txt);

            var schema = SchemaProvider.FromObject(testObj, "Data");
            var exe = OREL.Compile(@"Comments.LikedCount", schema);
            dynamic result = exe.Execute(testObj);
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual("2", result[1]);

            exe = OREL.Compile(@"SchemaId", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(34, result);

            exe = OREL.Compile(@"array", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(5, result.Count);

            exe = OREL.Compile(@"array[_>2]", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(3, result[0]);

            exe = OREL.Compile(@"array[_>2]=>{_+1}", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(4, result[0]._1);
            Assert.AreEqual(5, result[1]._1);

            //From ExpandoObject
            testObj = JsonConvert.DeserializeObject<ExpandoObject>(txt);
            schema = SchemaProvider.FromObject(testObj, "Data");
            exe = OREL.Compile(@"Comments.LikedCount", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(8, result.Count);
            Assert.AreEqual("2", result[1]);

            exe = OREL.Compile(@"SchemaId", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(34, result);

            exe = OREL.Compile(@"array", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(5, result.Count);

            exe = OREL.Compile(@"array[_>2]", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(3, result[0]);

            exe = OREL.Compile(@"array[_>2]=>{_+1}", schema);
            result = exe.Execute(testObj);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(4, result[0]._1);
            Assert.AreEqual(5, result[1]._1);

            //From ORELObject+generic type
            dynamic ORELObj = new ORELObject();
            ORELObj.SchemaId = 1;
            ORELObj.Data = new GClass<int>() { Name = "1", Value = 1, Child = new GClass<int>() { Name = "2", Value = 2 } };

            schema = SchemaProvider.FromObject(ORELObj, "Data");
            exe = OREL.Compile(@"SchemaId", schema);
            result = exe.Execute(ORELObj);
            Assert.AreEqual(1, result);

            exe = OREL.Compile(@"Value = 1", schema);
            result = exe.Execute(ORELObj);
            Assert.AreEqual(true, result);

            exe = OREL.Compile(@"{ K: Child.Name, V: Child.Value }", schema);
            result = exe.Execute(ORELObj);
            Assert.AreEqual("2", result.K);
            Assert.AreEqual(2, result.V);


        }

        /// <summary>
        /// 当针对JArray且元素是非简单类型JObject做SelectManay时，由于Select出的结果集中的元素仍为JObject，
        /// 此时需要遵循Json.Net的访问方式访问元素（默认并不会将JObject转换为标准动态类型）
        /// </summary>
        [TestMethod]
        public void TestMixedDynamicObject()
        {
            //From MixedObject
            dynamic expando = new ExpandoObject();
            expando.Name = "ex";
            var mixType = new MixType
            {
                Name = "ABC",
                JObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("Resources\\Json\\json2.json")),
                Expando = expando
            };

            var schema = SchemaProvider.FromObject(mixType);
            ORELExecutable exe;
            dynamic result;

            exe = OREL.Compile(@"Name", schema);
            result = exe.Execute(mixType);
            Assert.AreEqual("ABC", result);

            exe = OREL.Compile(@"JObject.data.comments.info", schema);
            result = exe.Execute(mixType);
            Assert.AreEqual("1", result[0].prop1.ToString());

            exe = OREL.Compile(@"JObject.data.comments.ads", schema);
            result = exe.Execute(mixType);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(8, result[7]);

            exe = OREL.Compile(@"JObject.data.comments=>{ad:ads}", schema);
            result = exe.Execute(mixType);
            Assert.AreEqual(1, result[0].ad[0].ToObject<int>());
            Assert.AreEqual(4, result[0].ad.Count);

            exe = OREL.Compile(@"JObject.data.comments[1].likecount", schema);
            result = exe.Execute(mixType);
            Assert.AreEqual("1", result);

            exe = OREL.Compile(@"Expando.Name", schema);
            result = exe.Execute(mixType);
            Assert.AreEqual("ex", result);

            //read failed for read from type that has dynamic property
            try
            {
                var failed = SchemaProvider.FromType<MixType>();
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("Cannot read schema info from dynamic object:JObject.", e.Message);
                //throw;
            }

            //自引用
            expando = new ExpandoObject();
            expando.Self = expando;
            schema = SchemaProvider.FromObject(expando);
            var itor = schema as IEnumerable<IMemberDefinition>;
            Assert.AreEqual(1, itor.Count());
        }

        [TestMethod]
        public void TestSchemaProviderForJArray()
        {
            var text = File.ReadAllText("Resources\\Json\\json20.json");
            var jobject = JsonConvert.DeserializeObject(text);
            var jarray = jobject as JArray;
            Assert.IsNotNull(jarray);

            var schema1 = SchemaProvider.FromObject(jarray);
            var exe = OREL.Compile("_.Property1", schema1);
            dynamic res = exe.Execute(jobject);
            Assert.AreEqual(3, res.Count);

            var schema3 = SchemaProvider.FromObject(res);

            //exe = OREL.Compile("_.Property3._.double", schema1);

            //exe = OREL.Compile("_.Property3.double", schema1);

            //res = exe.Execute(jobject);
            //Assert.AreEqual("2", res[1]);

            var mixObject = new MixType() { Expando = jarray };
            var schema2 = SchemaProvider.FromObject(mixObject);
        }

        [TestMethod]
        public void TestSchemaProviderAnonymous()
        {
            var ao = new
            {
                Key = 1,
                Value = new
                {
                    ValueKey = 1.1,
                    ValueValue = 1.2
                },
                List = new[]
                {
                    new { Name = "ABC1", SubList = new[] { new { A = "DEF1" }, new { A = "DEF2" } } },
                    new { Name = "ABC2", SubList = new[] { new { A = "DEF3" }, new { A = "DEF4" } } }
                }
            };
            var atype = ao.GetType();
            var schema = SchemaProvider.FromObject(ao);
            var exe = OREL.Compile(@"Key=1", schema);
            dynamic result = exe.Execute(ao);
            Assert.AreEqual(true, result);

            exe = OREL.Compile(@"{a:Value.ValueKey, b:Value.ValueValue}", schema);
            result = exe.Execute(ao);
            Assert.AreEqual(1.1, result.a);
            Assert.AreEqual(1.2, result.b);

            exe = OREL.Compile(@"List.Name", schema);
            result = exe.Execute(ao);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("ABC1", result[0]);
            Assert.AreEqual("ABC2", result[1]);

            exe = OREL.Compile(@"List.SubList", schema);
            result = exe.Execute(ao);
            Assert.AreEqual(4, result.Count);

            exe = OREL.Compile(@"List.SubList.A", schema);
            result = exe.Execute(ao);
            Assert.AreEqual(4, result.Count);

        }

        [TestMethod]
        public void TestParameters()
        {
            var txt = File.ReadAllText("Resources\\Json\\json3.json");
            dynamic testObj = JsonConvert.DeserializeObject<JObject>(txt);
            var schemas = GetDescriptor3();
            var exe = OREL.Compile("SchemaId=@schemaId", schemas, new[] { new ParameterDefinition("schemaId", DataType.Number) });
            var result = exe.Execute(testObj, new { schemaId = 34 });
            Assert.AreEqual(true, result);

            exe = OREL.Compile("publishTime>@dt", schemas, new[] { new ParameterDefinition("dt", DataType.DateTime) });
            result = exe.Execute(testObj, new { Dt = new DateTime(2017, 1, 1) });
            Assert.AreEqual(true, result);

            exe = OREL.Compile("{SchemaId, list:@list}", schemas, new[] { new ParameterDefinition("list", DataType.List) });
            result = exe.Execute(testObj, new { list = new[] { new { A = 1, B = 2 }, new { A = 2, B = 3 }, new { A = 3, B = 4 } } });
            Assert.AreEqual(2, result.list[0].B);
            Assert.AreEqual(2, result.list[1].A);
            Assert.AreEqual(34, result.SchemaId);

            exe = OREL.Compile("{SchemaId, obj:@obj}", schemas, new[] { new ParameterDefinition("obj", DataType.Object) });
            result = exe.Execute(testObj, new { obj = new { A = 1, B = 2, C = 3 } });
            Assert.AreEqual(2, result.obj.B);
            Assert.AreEqual(1, result.obj.A);
            Assert.AreEqual(34, result.SchemaId);

            exe = OREL.Compile("publishTime>@dt or @flag", schemas,
                new[] { new ParameterDefinition("dt", DataType.DateTime), new ParameterDefinition("flag", DataType.Boolean) });
            result = exe.Execute(testObj, new { dt = DateTime.Now, flag = true });
            Assert.AreEqual(true, result);
            result = exe.Execute(testObj, new { dt = DateTime.Now, flag = false });
            Assert.AreEqual(false, result);

            exe = OREL.Compile("Comments=>{Content, id:@id}", schemas, new[] { new ParameterDefinition("id", DataType.Text) });
            result = exe.Execute(testObj, new { id = "12345" });
            Assert.AreEqual("12345", result[1].id);
            Assert.AreEqual("12345", result[2].id);

            exe = OREL.Compile("Comments[LikedCount>@count]", schemas, new[] { new ParameterDefinition("count", DataType.Number) });
            result = exe.Execute(testObj, new { count = 2 });
            Assert.AreEqual(5, result.Count);
            result = exe.Execute(testObj, new { count = 3 });
            Assert.AreEqual(4, result.Count);
        }

        [TestMethod]
        public void TestParameters2()
        {
            var exe = OREL.Compile("date_fmt(@date, 'yyyyMMdd')", new[] { new ParameterDefinition("date", DataType.DateTime) });
            dynamic result = exe.Execute(null, new { date = DateTimeOffset.Now });
            Assert.AreEqual(DateTimeOffset.Now.ToString("yyyyMMdd"), result);

            exe = OREL.Compile("{a:@A, b:@B}");
            result = exe.Execute(null, new { A = new int[] { 1, 3, 4 }, B = new { B1 = 1, B2 = 2 } });
            Assert.AreEqual(3, result.a.Length);
            Assert.AreEqual(1, result.b.B1);
        }

        [TestMethod]
        public void TestDynamicParameters()
        {
            ORELExecutable exe;
            dynamic result;

            exe = OREL.Compile("{a:len(@A), b:@B, c:@C, d:date_fmt(@D,'yyyyMMdd'), e:@E}"
                , new[] { new ParameterDefinition("A", DataType.List), new ParameterDefinition("D", DataType.DateTime) });
            //JObject作参数
            var param = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(new
            {
                A = new int[] { 1, 3, 4 },
                B = new { B1 = 1, B2 = 2 },
                C = 1.2,
                D = new DateTime(2018, 12, 1),
                E = "AAAA"
            }));
            result = exe.Execute(null, param);
            Assert.AreEqual(3, result.a);
            Assert.AreEqual(1, result.b.B1.Value);
            //PlainObject
            param = new
            {
                A = new int[] { 1, 3, 4 },
                B = new { B1 = 1, B2 = 2 },
                C = 1.2,
                D = new DateTime(2018, 12, 1),
                E = "AAAA"
            };
            result = exe.Execute(null, param);
            Assert.AreEqual(3, result.a);
            Assert.AreEqual(1, result.b.B1);

            param = new ExpandoObject();
            param.A = new int[] { 1, 3, 4 };
            param.B = new { B1 = 1, B2 = 2 };
            param.C = 1.2;
            param.D = new DateTime(2018, 12, 1);
            param.E = "AAAA";

            result = exe.Execute(null, param);
            Assert.AreEqual(3, result.a);
            Assert.AreEqual(1, result.b.B1);

        }

        [TestMethod]
        public void FixRDBLAC_231()
        {
            var txt = File.ReadAllText("Resources\\Json\\json3.json");
            var schemas = GetDescriptor3();
            ORELExecutable exe;
            dynamic result;
            exe = OREL.Compile("text(publishTime)", schemas);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual("2017/9/18 19:26:06 +08:00", result);

            exe = OREL.Compile("match(Comments[1].Content,'\\d')", schemas);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("match(Comments.Content[1],'\\d')", schemas);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("len(Comments[1].Content)", schemas);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(24, result);

            exe = OREL.Compile("len(Comments.Content[1])", schemas);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(24, result);

            exe = OREL.Compile("len(123)", schemas); //任何类型数据最终会fallback到object
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(3, result);

            exe = OREL.Compile("{Comments[1].Author}", schemas);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual("韦石泉", result.Author);

            exe = OREL.Compile("{Comments.Author[1]}", schemas);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual("韦石泉", result.Author);

            exe = OREL.Compile("{Comments.Replies.Author}", schemas);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(6, result.Author.Count);

        }

        [TestMethod]
        public void TestDataTimeFix()
        {
            //对接测试

            //date2 ，显示调用refdate

            var target = new DateTimeOffset(new DateTime(2018, 12, 9), TimeSpan.FromHours(8));
            var exe = OREL.Compile("date2('3天前',date('2018-12-12'),8)");
            var result = exe.Execute();
            Assert.AreEqual(target, result);

            exe = OREL.Compile("date2('3天前',date('2018-12-12'))");
            result = exe.Execute();
            Assert.AreEqual(target, result);

            exe = OREL.Compile("date2('3天前','2018-12-12',8)");
            result = exe.Execute();
            Assert.AreEqual(target, result);

            exe = OREL.Compile("date2('3天前','2018-12-12')");
            result = exe.Execute();
            Assert.AreEqual(target, result);

            exe = OREL.Compile("date2('3天前','2018-12-12Z')");
            result = exe.Execute();
            Assert.AreEqual(new DateTimeOffset(new DateTime(2018, 12, 9), TimeSpan.FromHours(0)), result);
        }

        [TestMethod]
        public void TestDateTimeOffset()
        {
            var json = File.ReadAllText("Resources\\Json\\json8.json");
            var obj = JsonConvert.DeserializeObject<JObject>(json);
            var schema = SchemaProvider.FromObject(obj);
            var exe = OREL.Compile("data.crawlTime", schema);
            var result = exe.Execute(obj);

            var json2 = @"{""data"":{""crawlTime"":""123""}}";
            var obj2 = JsonConvert.DeserializeObject<JObject>(json2);
            result = exe.Execute(obj2);
            Assert.AreEqual(1, 1);

            var json3 = @"{""data"":{""crawlTime"":""2021-12-30 08:55:52""}}";
            var obj3 = JsonConvert.DeserializeObject<JObject>(json3);
            result = exe.Execute(obj3);
            Assert.AreEqual(1, 1);
        }


        [TestMethod]
        public void TestExtractFix()
        {
            var exe = OREL.Compile("extr('http://www.abc.com/aaaa.com','https?://([\\w\\.]+)',1)");
            var result = exe.Execute();
            Assert.AreEqual("www.abc.com", result);

            exe = OREL.Compile("extr('http://www.abc.com/aaaa.com','https?://([\\w\\.]+)')");
            result = exe.Execute();
            Assert.AreEqual("www.abc.com", ((string[])result)[1]);

            exe = OREL.Compile("extr('http://www.abc.com/aaaa.com','https?://([\\w\\.]+)')[2]");
            result = exe.Execute();
            Assert.AreEqual("www.abc.com", result);
        }

        [TestMethod]
        public void TestArrayInitiation()
        {
            var exe = OREL.Compile("[]");
            dynamic result = exe.Execute();
            Assert.AreEqual(0, result.Count);

            exe = OREL.Compile("[1,2,3,4,5]");
            result = exe.Execute();
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(1, result[0]);

            exe = OREL.Compile("[{A:1,B:2},{C:1,D:2}]");
            result = exe.Execute();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].A);
            Assert.AreEqual(1, result[1].C);

            exe = OREL.Compile("{A:1,B:2,C:['a','b','c']}");
            result = exe.Execute();
            Assert.AreEqual(3, result.C.Count);
            Assert.AreEqual("a", result.C[0]);

            exe = OREL.Compile("[{A:1,B:2},{C:3,D:4}][1]");
            result = exe.Execute();
            Assert.AreEqual(1, result.A);

            exe = OREL.Compile("len([{A:1,B:2},{C:3,D:4}])");
            result = exe.Execute();
            Assert.AreEqual(2, result);

            exe = OREL.Compile("[{A:1,B:2},{C:3,D:4}]=>{ A:A, B:B,C:C,D:D }"); //未支持
            result = exe.Execute();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].A);
            Assert.AreEqual(null, result[1].B);
            Assert.AreEqual(null, result[0].C);
            Assert.AreEqual(4, result[1].D);
        }

        [TestMethod]
        public void TestArrayConcat()
        {
            ORELExecutable exe;
            dynamic result;

            //exe = OREL.Compile("[1]+[2,3,4]");
            //result = exe.Execute();
            //Assert.AreEqual(4, result.Count);

            exe = OREL.Compile("[1]+[2,3,4]+[5,6]");
            result = exe.Execute();
            Assert.AreEqual(6, result.Count);

            var desc = GetDescriptor3();
            var txt = File.ReadAllText("Resources\\Json\\json3.json");

            exe = OREL.Compile("Comments+[{ Page,Title }]", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(9, result.Count);

            exe = OREL.Compile("(Comments[LikedCount>1]=>{ Content,Author })+[ { Content:'Content',Author:'Author' }]", desc);
            result = exe.ExecuteJson(txt);
            Assert.AreEqual(8, result.Count);
        }

        [TestMethod]
        public void TestArrayAddition()
        {
            var txt = File.ReadAllText("Resources\\Json\\json6.json");
            var obj = JsonConvert.DeserializeObject(txt);
            var schema = SchemaProvider.FromObject(obj);
            var exe = OREL.Compile("data[-1].`comment-id`", schema);
            dynamic result = exe.Execute(obj);
            Assert.AreEqual("5cf4ed410000000028032a33", result.ToString());

            exe = OREL.Compile("data[-3].`note-id`", schema);
            result = exe.Execute(obj);
            Assert.AreEqual("5b4d851d672e140538a811d5", result.ToString());

            exe = OREL.Compile("data[-20].`note-id`", schema); //首条
            result = exe.Execute(obj);
            Assert.AreEqual("5b4d851d672e140538a811d5", result.ToString());

            exe = OREL.Compile("data[-21].`note-id`", schema); //首条
            result = exe.Execute(obj);
            Assert.AreEqual(null, result);

            exe = OREL.Compile("data=>{ time: `crawl-time`}", schema); //日期
            result = exe.Execute(obj);
            Assert.AreEqual(20, ((IList)result).Count);

            exe = OREL.Compile("data[-2..-1].`comment-id`", schema); //日期
            result = exe.Execute(obj);
            Assert.AreEqual(2, ((IList)result).Count);
            Assert.AreEqual("5cf602e90000000028020802", result[0].ToString());
            Assert.AreEqual("5cf4ed410000000028032a33", result[1].ToString());

            exe = OREL.Compile("data[3..-3].`comment-id`", schema); //日期
            result = exe.Execute(obj);
            Assert.AreEqual(16, ((IList)result).Count);
            Assert.AreEqual("5b7954934cc573000191dd57", result[0].ToString());
            Assert.AreEqual("5cf64a11000000002600cc2f", result[15].ToString());
        }

        [TestMethod]
        public void TestStringJoin()
        {
            var txt = File.ReadAllText("Resources\\Json\\json6.json");
            var obj = JsonConvert.DeserializeObject(txt);
            var schema = SchemaProvider.FromObject(obj);
            var exe = OREL.Compile("join(null)");
            var result = exe.Execute();
            Assert.AreEqual(String.Empty, result);

            exe = OREL.Compile("join(msg)", schema);
            result = exe.Execute(obj);
            Assert.AreEqual("success", result);

            exe = OREL.Compile("join([])");
            result = exe.Execute();
            Assert.AreEqual(String.Empty, result);

            exe = OREL.Compile("join([1])");
            result = exe.Execute();
            Assert.AreEqual("1", result);

            exe = OREL.Compile("join([1,2,3])");
            result = exe.Execute();
            Assert.AreEqual("1,2,3", result);

            exe = OREL.Compile("join([1,2,3],':')");
            result = exe.Execute();
            Assert.AreEqual("1:2:3", result);

            exe = OREL.Compile("join(data.`note-id`[2..3],':')", schema);
            result = exe.Execute(obj);
            Assert.AreEqual("5b4d851d672e140538a811d5:5b4d851d672e140538a811d5", result);

            exe = OREL.Compile("join(data.`sub-comments`[1..3].`comment-id`)", schema);
            result = exe.Execute(obj);
            Assert.AreEqual("5b767abb576b4400015f9ade,,5b7c163d36e6440001d79f51", result);
        }


        [TestMethod]
        public void TestEmpty()
        {
            ORELExecutable exe;
            dynamic result;

            exe = OREL.Compile("_", SchemaProvider.Empty());
            result = exe.Execute("aaa");
            Assert.AreEqual("aaa", result);

            exe = OREL.Compile("1+1");
            result = exe.Execute();
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void TestBool()
        {
            ORELExecutable exe;
            dynamic result;

            //GClass<int> gc0 = new GClass<int>() { Value = 1 };
            //var schema = SchemaProvider.FromType<GClass<int>>();
            //exe = OREL.Compile("Value=(1+2)", schema);
            //result = exe.Execute(gc0);          

            GClass<bool> gc = new GClass<bool>() { Value = true };
            var schema = SchemaProvider.FromType<GClass<bool>>();
            exe = OREL.Compile("Value=true", schema);
            result = exe.Execute(gc);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Value=false", schema);
            result = exe.Execute(gc);
            Assert.AreEqual(false, result);

            exe = OREL.Compile("1=(2-1)", schema);
            result = exe.Execute(gc);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Value=(1=1)", schema);
            result = exe.Execute(gc);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("Value!=(1>1)", schema);
            result = exe.Execute(gc);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("true!=(1>1)", schema);
            result = exe.Execute(gc);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("false=(1<1)", schema);
            result = exe.Execute(gc);
            Assert.AreEqual(true, result);

            exe = OREL.Compile("true=(1=1)", schema);
            result = exe.Execute(gc);
            Assert.AreEqual(true, result);

            var txt = File.ReadAllText("Resources\\Json\\json8.json");
            var obj = JsonConvert.DeserializeObject<JObject>(txt);
            schema = SchemaProvider.FromObject(obj);
            exe = OREL.Compile("result=true", schema);
            result = exe.Execute(obj);
            Assert.IsTrue(result);
            exe = OREL.Compile("result=true", schema);
            result = exe.Execute(obj);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestJson()
        {
            var txt = File.ReadAllText("Resources\\Json\\json10.json");
            //var obj = JsonConvert.DeserializeObject<dynamic>(txt);
            var obj = JsonConvert.DeserializeObject<object>(txt);
            //var obj3 = obj.ToExpandoObject();
            //var obj4 = obj2.ToExpandoObject();


            var schema = SchemaProvider.FromObject(obj);

            //var statement = "data.data.notes[user!=null].user.userid";
            var statement = "data.data.notes[id='5d1301e600000000270069f8'].id";

            // statement.ToExpandoObject();

            var exe = OREL.Compile(statement, schema);
            var result = exe.Execute(obj, exe);
        }

        [TestMethod]
        public void TestUnixTimestamp()
        {
            string statement;
            ORELExecutable exe;
            object result;
            statement = "date(1533622085)";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(new DateTime(2018, 8, 7, 14, 8, 5), ((DateTimeOffset)result).LocalDateTime);

            statement = "date(1533622085,0)";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(new DateTime(2018, 8, 7, 6, 8, 5), ((DateTimeOffset)result).UtcDateTime);

            statement = "ts(date('2018-8-7 14:8:5'))";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(1533622085, (decimal)result);

            statement = "ts(date('2018-8-7 6:8:5',0))";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(1533622085, (decimal)result);

            statement = "ts_ms(date('2018-8-7 14:8:5'))";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(1533622085000, (decimal)result);

            statement = "date(1533622085000)";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(new DateTime(2018, 8, 7, 14, 8, 5), ((DateTimeOffset)result).LocalDateTime);

            var text = File.ReadAllText("Resources\\Json\\json8.json");
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);
            statement = "date(data.timestamp)";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(new DateTimeOffset(2018, 8, 7, 14, 8, 5, TimeSpan.FromHours(8)), result);

            statement = "ts(data.crawlTime)";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(1640854552m, result);
        }

        [TestMethod]
        public void TestIsNull()
        {
            string statement;
            ORELExecutable exe;
            object result;
            var text = File.ReadAllText("Resources\\Json\\json9.json");
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);

            statement = "isnull(data.data.notes[2].videoInfo)";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            statement = "isnull(data.data.notes[2].tagInfo)";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(false, result);

            statement = "isnull(data.data.notes[3].imagesList)";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(false, result);

            statement = "(data.data.notes[2].videoInfo)=null";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            statement = "(data.data.notes[2].tagInfo)!=null";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            statement = "(data.data.notes[3].imagesList)=null";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(false, result);

            statement = "noe(data.data.notes[2].tagInfo)";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            statement = "noe(data.data.notes[3].imagesList)";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            statement = "noe(data.data.notes[2].type)";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            statement = "noe('')";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            statement = "noe([])";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(true, result);

            statement = "noe([1,2,3])";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(false, result);

            statement = "data.data.notes[!noe(user)].user.userId";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(19, ((IList)result).Count);

            statement = "data.data.notes.user.userId";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(true, ((IList)result).Contains(null));

            statement = "data.data.notes.user.userId[_!=null]";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(19, ((IList)result).Count);

            statement = "data.data.notes.user.userId[!noe(_)]";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(18, ((IList)result).Count);
        }

        [TestMethod]
        public void TestIssueDotAccessInArithmetic()
        {
            string statement;
            ORELExecutable exe;
            dynamic result;
            var text = File.ReadAllText("Resources\\Json\\json9.json");
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);

            statement = "'123'+data.data.totalCount+'456'";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual("1231364027456", result);

            statement = "data.data.notes=>{ title: '123' + user.nickname }";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual("123叶一茜", result[0].title);

            statement = "data.data.notes=>{ title: '123' + user.nickname + '456' }";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual("123叶一茜456", result[0].title);
        }

        [TestMethod]
        public void TestJson12()
        {
            string statement;
            ORELExecutable<IList> exe;
            dynamic result;
            var text = File.ReadAllText("Resources\\Json\\json12.json");
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);
            statement = "data[!noe(questionId)].questionId";
            exe = OREL.Compile<IList>(statement, schema);
            result = exe.Execute(obj);

        }

        [TestMethod]
        public void TestYieldPriority()
        {
            string statement;
            ORELExecutable exe;
            dynamic result;
            var text = File.ReadAllText("Resources\\Json\\json13.json");
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);
            statement = File.ReadAllText("Resources\\statement1.txt");
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(161, result.content.Count);
            Assert.AreEqual(18, result.types.Count);
        }

        [TestMethod]
        public void TestDaySpan()
        {
            ORELExecutable exe;
            dynamic result;

            var obj = new
            {
                startTime = new DateTimeOffset(2019, 12, 12, 8, 0, 0, TimeSpan.FromHours(8)),
                endTime = new DateTimeOffset(2019, 12, 12, 12, 0, 0, TimeSpan.FromHours(8))
            };
            var schema = SchemaProvider.FromObject(obj);
            exe = OREL.Compile("day_span(startTime, endTime)", schema);
            result = exe.Execute(obj);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(obj.startTime, result[0].Start);
            Assert.AreEqual(obj.endTime, result[0].End);

            obj = new
            {
                startTime = new DateTimeOffset(2019, 12, 12, 8, 0, 0, TimeSpan.FromHours(8)),
                endTime = new DateTimeOffset(2019, 12, 13, 12, 0, 0, TimeSpan.FromHours(8))
            };
            //exe = OREL.Compile("day_span(startTime, endTime)", schema);
            result = exe.Execute(obj);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(obj.startTime, result[0].Start);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 13, 0, 0, 0, TimeSpan.FromHours(8)), result[0].End);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 13, 0, 0, 0, TimeSpan.FromHours(8)), result[1].Start);
            Assert.AreEqual(obj.endTime, result[1].End);

            obj = new
            {
                startTime = new DateTimeOffset(2019, 12, 12, 8, 0, 0, TimeSpan.FromHours(8)),
                endTime = new DateTimeOffset(2019, 12, 14, 12, 0, 0, TimeSpan.FromHours(8))
            };
            //exe = OREL.Compile("day_span(startTime, endTime)", schema);
            result = exe.Execute(obj);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(obj.startTime, result[0].Start);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 13, 0, 0, 0, TimeSpan.FromHours(8)), result[0].End);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 13, 0, 0, 0, TimeSpan.FromHours(8)), result[1].Start);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 14, 0, 0, 0, TimeSpan.FromHours(8)), result[1].End);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 14, 0, 0, 0, TimeSpan.FromHours(8)), result[2].Start);
            Assert.AreEqual(obj.endTime, result[2].End);

            obj = new
            {
                startTime = new DateTimeOffset(2019, 12, 12, 8, 0, 0, TimeSpan.FromHours(8)),
                endTime = new DateTimeOffset(2019, 12, 14, 0, 0, 0, TimeSpan.FromHours(8))
            };
            //exe = OREL.Compile("day_span(startTime, endTime)", schema);
            result = exe.Execute(obj);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(obj.startTime, result[0].Start);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 13, 0, 0, 0, TimeSpan.FromHours(8)), result[0].End);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 13, 0, 0, 0, TimeSpan.FromHours(8)), result[1].Start);
            Assert.AreEqual(obj.endTime, result[1].End);

            obj = new
            {
                startTime = new DateTimeOffset(2019, 12, 12, 8, 0, 0, TimeSpan.FromHours(8)),
                endTime = new DateTimeOffset(2020, 1, 3, 0, 0, 0, TimeSpan.FromHours(8))
            };
            //exe = OREL.Compile("day_span(startTime, endTime)", schema);
            result = exe.Execute(obj);
            Assert.AreEqual(22, result.Count);

            obj = new
            {
                startTime = new DateTimeOffset(2019, 12, 12, 8, 0, 0, TimeSpan.FromHours(8)),
                endTime = new DateTimeOffset(2019, 12, 13, 0, 10, 0, TimeSpan.FromHours(8))
            };
            exe = OREL.Compile("hour_span(startTime, endTime)", schema);
            result = exe.Execute(obj);
            Assert.AreEqual(17, result.Count);

            obj = new
            {
                startTime = new DateTimeOffset(2019, 12, 12, 8, 0, 0, TimeSpan.FromHours(8)),
                endTime = new DateTimeOffset(2019, 12, 13, 0, 10, 0, TimeSpan.FromHours(8))
            };
            exe = OREL.Compile("hour_span(startTime, endTime)[1..]", schema);
            result = exe.Execute(obj);
            Assert.AreEqual(17, result.Count);
        }

        [TestMethod]
        public void TestDynamicSchema()
        {
            string statement;
            ORELExecutable exe;
            dynamic result;

            statement = "{ a: 1, b: 2 }.a";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(1, result);

            try
            {
                statement = "{{ a: 1, b: 2 }.a, b}";
                exe = OREL.Compile(statement);
            }
            catch (InvalidMemberNameException)
            {
                //ignore
            }


            statement = "[{ a: 1, b: 2 },{ a: 3, b: 4 }].a";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(3, result[1]);

            //statement = "{prop1:[{ a: 1, b: 2 },{ a: 3, b: 4 }]=>{c:a+b}.c, prop2:a}";
            statement = "([{ a: 1, b: 2 },{ a: 3, b: 4 }]=>{c:a+b}).c";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(3, result[0]);
            Assert.AreEqual(7, result[1]);

            statement = "day_span(date('2019-12-12'),date('2019-12-14')).Start";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 12, 0, 0, 0, TimeSpan.FromHours(8)), result[0]);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 13, 0, 0, 0, TimeSpan.FromHours(8)), result[1]);

            statement = "day_span(date('2019-12-12'),date('2019-12-14'))=>{Start, End}";
            exe = OREL.Compile(statement);
            result = exe.Execute();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 12, 0, 0, 0, TimeSpan.FromHours(8)), result[0].Start);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 13, 0, 0, 0, TimeSpan.FromHours(8)), result[0].End);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 13, 0, 0, 0, TimeSpan.FromHours(8)), result[1].Start);
            Assert.AreEqual(new DateTimeOffset(2019, 12, 14, 0, 0, 0, TimeSpan.FromHours(8)), result[1].End);
        }

        [TestMethod]
        public void TestDeepLevelYield()
        {
            string statement;
            ORELExecutable exe;
            dynamic result;
            var text = File.ReadAllText("Resources\\Json\\json14.json");
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);

            statement = @"response.body.data=>
                {
                   items: display.sub_brands.item.series_list.item=>{cover_url,hot},
                   display: display.title
                }";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);

            Assert.AreEqual("http://p3.pstatp.com/large/w240/motor-img/a4f9d56bada17a8fffb9fcede936e720.png", result[0].items[0].cover_url);
            Assert.AreEqual("166685", result[0].items[0].hot);

        }

        [TestMethod]
        public void TestToutiaoExtract()
        {
            string statement;
            ORELExecutable exe;
            dynamic result;
            var text = File.ReadAllText("Resources\\Json\\json15.json");
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);

            statement = @"data=>
                {
template_key,
ala_src,
cell_type,
title:if(title=null,display.title,title),
display.sub_brand_name,
display.series_type,
display.price_text,
display.series_config,
display.cars.item,
items:display.items=>{name, reason},
display.summary.text,
images:display.results=>{text,img_url},
abstract,
desc,
comment_count,
digg_count,
read_count,
hits: display.hits=>{title,video_url,digg_count,play_count,info.user_nickname},
label,
merge_article: merge_article=>{abstract,article_url,comment_count,datetime,digg_count,forward_count,media_name,read_count,summary},
merge_weitoutiao: merge_weitoutiao=>{ comment_count,content,create_time:date(create_time),digg_count,forward_count,media_name,share_url,summary},
queries.text,
url,
host,
total,
                }";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);

        }

        [TestMethod]
        public void TestReduce()
        {
            string statement;
            ORELExecutable exe;
            dynamic result;
            var text = File.ReadAllText("Resources\\Json\\json16.json", System.Text.Encoding.Default);
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);

            statement = @"data.result=>{
 trains: split(_,'|')->{ 
        Stopped: $1='', 
        TrainNo: $4, 
        Depart: $5, 
        Arrive: $6,
        DepartTime: $9,
        ArriveTime: $10,
        TimeSpan: $11,
        Seat2: $31,
        Seat1: $32,
        SeatB: $33,
    }
}
";

            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);

            Assert.AreEqual("G104", result[1].trains.TrainNo);
            Assert.AreEqual("有", result[1].trains.Seat1);
            Assert.AreEqual(true, result[0].trains.Stopped);

            statement = @"data.result=>{
 TrainNo: split(_,'|')->{ 
        Stopped: $1='', 
        TrainNo: $4, 
        Depart: $5    
    }.TrainNo
}
";

            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);

            Assert.AreEqual("G104", result[1].TrainNo);
        }

        [TestMethod]
        public void TestMemberAccessFromRoot()
        {
            string statement;
            ORELExecutable exe;
            dynamic result;
            var text = File.ReadAllText("Resources\\Json\\json17.json", System.Text.Encoding.UTF8);
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);

            statement = @"{
    response.body.status,
    trains:response.body.data.result=>{
            train: split(_,'|')->{ 
                    Stopped: $1='', 
                    TrainNo: $4, 
                    Depart: $5, 
                    Arrive: $6,
                    DepartTime: $9,
                    ArriveTime: $10,
                    TimeSpan: $11,
                    Seat2: $31,
                    Seat1: $32,
                    SeatB: $33,
                    date: request.query.leftTicketDTO_train_date
                }
            }.train
}
";

            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);

            Assert.AreEqual("G102", result.trains[0].TrainNo);
            Assert.AreEqual("有", result.trains[1].Seat1);
            Assert.AreEqual(false, result.trains[0].Stopped);
            Assert.AreEqual("2020-04-30", result.trains[0].date);
        }

        [TestMethod]
        public void TestMemberAccessFromArray()
        {
            var schema = SchemaProvider.FromType<InternalClass>();
            var statement = "name";
            var exe = OREL.Compile(statement, schema);
            var result = exe.Execute(new InternalClass[1]);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestJson2Object()
        {
            string statement;
            ORELExecutable exe;
            dynamic result;
            var text = File.ReadAllText("Resources\\Json\\json18.json", encoding: System.Text.Encoding.UTF8);
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);
            statement = @"
                                response.body.result.data=>j2o(content)
            =>{
                abstract,
                abstract1,
                cell_ctrls,
                cell_ctrls.cell_flag,
            }
                         ";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.IsNotNull(result);
            Assert.AreEqual("5898240", result[0].cell_flag.ToString());

            statement = @"
                    response.body.result.data=>j2a(code)=>{ texts: _.text }

             ";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.IsNotNull(result);
            Assert.AreEqual("aaa", result[0].texts[0].ToString());
            Assert.AreEqual("nnn", result[0].texts[1].ToString());

            statement = @"
                    j2a(response.body.result.message)->{ f1:$1,f2:$2,f3:$3 }
             ";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.IsNotNull(result);
            Assert.IsNotNull("1", result.f1.ToString());
        }

        [TestMethod]
        public void TestCtripDataProcess()
        {
            string statement;
            ORELExecutable exe;
            dynamic result;
            var text = File.ReadAllText("Resources\\Json\\json19.json", encoding: System.Text.Encoding.UTF8);
            var obj = JsonConvert.DeserializeObject(text);
            var schema = SchemaProvider.FromObject(obj);

            //cabins
            statement = @"
(routeList=>{
    cabins:legs.flight_Cabins=>{
        flight_Cabin_Price,
        flight_Cabin_Class,
        flight_Cabin_Price_Class,
        freeLuggageAmount,
        route_id,
    }
}).cabins
";

            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);
            Assert.AreEqual(22, result.Count);
            Assert.AreEqual(324, result[0].flight_Cabin_Price);
            Assert.AreEqual("e88d69d9-76bb-4612-81bb-96c2ad5f54a8", result[0].route_id);
            Assert.AreEqual("e88d69d9-76bb-4612-81bb-96c2ad5f54a8", result[1].route_id);
            Assert.AreEqual("31473c90-3303-4c27-8394-dbd5dde4f899", result[2].route_id);
            Assert.AreEqual(425, result[2].flight_Cabin_Price);
            //transit
            statement = @"
(routeList[route_Type='Transit']=>
{
    fromFilghtNumber: legs[1].flight_Number,
    fromFilghtAirline: legs[1].flight_Airline_Name,
    fromDepartureAirportName: legs[1].flight_Departure_Airport_Name,
    toFilghtNumber: legs[2].flight_Number,
    toFilghtAirline: legs[2].flight_Airline_Name,
    toDepartureAirportName: legs[2].flight_Departure_Airport_Name,
    route_id,
    route_order
})
+(
routeList[route_Type='FlightTrain']=>
{
    fromTrainNo: legs[1].train_transportNo,
    fromStationName: legs[1].train_Departure_Station_Name,    
    toFilghtNumber: legs[2].flight_Number,
    toFilghtAirline: legs[2].flight_Airline_Name,
    toDepartureAirportName: legs[2].flight_Departure_Airport_Name,
    route_id,
    route_order
})
";
            exe = OREL.Compile(statement, schema);
            result = exe.Execute(obj);

            Assert.AreEqual(7, result.Count);
            Assert.AreEqual("CZ3312", result[0].fromFilghtNumber);
            Assert.AreEqual("CZ6886", result[0].toFilghtNumber);
            Assert.AreEqual("e88d69d9-76bb-4612-81bb-96c2ad5f54a8", result[0].route_id);
            Assert.AreEqual("94c3ee52-df2e-4ec1-a7c2-496be33060cb", result[5].route_id);
            Assert.AreEqual("Z3", result[5].fromTrainNo);
            Assert.AreEqual(null, result[5].fromFilghtNumber);
            Assert.AreEqual("CZ6950", result[5].toFilghtNumber);

            //var settings = new JsonSerializerSettings()
            //{
            //    Converters = new List<JsonConverter>() { new ORELJsonWriter() },
            //    Formatting = Formatting.Indented
            //};

            //var json = JsonConvert.SerializeObject(result, settings);
        }

        [TestMethod]
        public void TestArrrayItemSchema()
        {
            var text = File.ReadAllText("Resources\\Json\\json21.json", encoding: System.Text.Encoding.UTF8);
            var obj = JsonConvert.DeserializeObject<JArray>(text);
            var data = obj.First().SelectToken("$.Data");
            var schema = SchemaProvider.FromObject(data);
            var exe = OREL.Compile(@"data=>{url,title,content,noteType,
topics:topics.name,pictures,publishTime,noteId,isRecommend,recommendNoteId,
readCount,commentCount,likeCount,shareCount,
collectedCount,userId,userName,redId,crawlTime,
topicId:topics.topicId}", schema);
            var result = exe.Execute(data);
            var schema2 = SchemaProvider.FromObject(result);
        }

        [TestMethod]
        public void TestDictionary()
        {
            var dict = new Dictionary<string, MixType>();
            dict.Add("11", new MixType() { Name = "Mix" });
            var obj = new { Values = dict.Values.ToList() };
            var schema = SchemaProvider.FromObject(obj);
            var exe = OREL.Compile("Values[Name like '%Mix%']", schema);
            var r = exe.Execute(obj);
        }

        [TestMethod]
        public void TestRange()
        {
            string ql = "range(today(), today()+'30d', '2d')";
            var exe = OREL.Compile(ql);
            dynamic r = exe.Execute();
            Assert.AreEqual(15, r.Count);

            ql = "range(todate('2021-3-1'), todate('2021-4-1'), '1d')";
            exe = OREL.Compile(ql);
            r = exe.Execute();
            Assert.AreEqual(31, r.Count);
            Assert.AreEqual(new DateTimeOffset(2021, 3, 1, 0, 0, 0, TimeSpan.FromHours(8)), r[0]);
            Assert.AreEqual(new DateTimeOffset(2021, 3, 31, 0, 0, 0, TimeSpan.FromHours(8)), r[30]);

            ql = "range(today(), today()+'30d', '1d')";
            exe = OREL.Compile(ql);
            r = exe.Execute();
            Assert.AreEqual(30, r.Count);

            ql = "range(todate('2021-3-1'), todate('2021-4-1'), '12h30m40s')";
            exe = OREL.Compile(ql);
            r = exe.Execute();
            Assert.AreEqual(60, r.Count);

            ql = "range(1, 10, 2)";
            exe = OREL.Compile(ql);
            r = exe.Execute();
            Assert.AreEqual(5, r.Count);
            Assert.AreEqual(9m, r[4]);

            ql = "range(1, 10, 2.5)";
            exe = OREL.Compile(ql);
            r = exe.Execute();
            Assert.AreEqual(4, r.Count);
            Assert.AreEqual(8.5m, r[3]);
        }

        public class MixType
        {
            public string Name { get; set; }
            public JObject JObject { get; set; }
            public dynamic Expando { get; set; }
        }

        public class GClass<T>
        {
            public string Name { get; set; }
            public T Value { get; set; }
            public GClass<T> Child { get; set; }
        }

        public class InternalClass
        {
            public string Name { get; set; }
            public int Number { get; set; }
            public DateTime Date { get; set; }
            public int[] Array { get; set; }
            public SubClass[] SubClasses { get; set; }
        }

        public class SubClass
        {
            public string Key { get; set; }
            public DateTimeOffset Value { get; set; }
        }

        /// <summary>
        /// 构建Json3.json对应数据的MemberDescriptor；
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<MemberDefinition> GetDescriptor3()
        {
            var desc = new List<MemberDefinition>();
            desc.Add(new MemberDefinition("SchemaId", DataType.Number));
            desc.Add(new MemberDefinition("PublishTime", DataType.DateTime));
            desc.Add(new MemberDefinition("Data", DataType.Object));
            desc.Add(new MemberDefinition("Page", DataType.Number, "Data"));
            desc.Add(new MemberDefinition("Title", DataType.Text, "Data"));
            desc.Add(new MemberDefinition("CommentCount", DataType.Number, "Data"));
            desc.Add(new MemberDefinition("Comments", DataType.List, "Data"));
            desc.Add(new MemberDefinition("LikedCount", DataType.Number, "Data.Comments"));
            desc.Add(new MemberDefinition("Content", DataType.Text, "Data.Comments"));
            desc.Add(new MemberDefinition("Author", DataType.Text, "Data.Comments"));
            desc.Add(new MemberDefinition("CreateTime", DataType.DateTime, "Data.Comments"));
            desc.Add(new MemberDefinition("Replies", DataType.List, "Data.Comments"));
            desc.Add(new MemberDefinition("Author", DataType.Text, "Data.Comments.Replies"));
            desc.Add(new MemberDefinition("Content", DataType.Text, "Data.Comments.Replies"));
            desc.Add(new MemberDefinition("Label", DataType.List, "Data.Comments.Replies"));
            desc.Add(new MemberDefinition("Name", DataType.Text, "Data.Comments.Replies.Label"));
            desc.Add(new MemberDefinition("Content", DataType.Text, "Data.Comments.Replies.Label"));
            desc.Add(new MemberDefinition("Label", DataType.List, "Data.Comments"));
            desc.Add(new MemberDefinition("Name", DataType.Text, "Data.Comments.Label"));
            desc.Add(new MemberDefinition("Content", DataType.Text, "Data.Comments.Label"));
            return desc;
        }

        private Delegate CompileQuery(string startement)
        {
            LambdaExpression lamda = BuildQuery(startement);
            return lamda.Compile();
        }

        private LambdaExpression BuildQuery(string statement)
        {
            System.Collections.Generic.List<Token> tokens = Tokenizer.Scan(statement).First();
            TreeBuilder treeBuilder = new TreeBuilder();
            treeBuilder.AppendRange(tokens);
            Expression exp = treeBuilder.GernerateTree();
            LambdaExpression lamda = Expression.Lambda(exp);
            return lamda;
        }
    }
}
