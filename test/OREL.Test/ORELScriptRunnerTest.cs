using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Orel.Test
{
    [TestClass]
    public class ORELScriptRunnerTest
    {
        private async Task<JObject> LoadJson(string path)
        {
            var jsonText = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<JObject>(jsonText);
        }

        [TestMethod]
        public void TestExternalMethod()
        {
            var runner = new ORELScriptRunner();
            runner.AddExternalMethod("add", new Func<int, int, int>((a, b) => a + b));
            runner.AddExternalMethod("add", new Func<string, string, string>((a, b) => a + b));

            var readfile = new Func<string, Task<object>>(async path =>
                  {
                      var jsonText = await File.ReadAllTextAsync(path);
                      return JsonConvert.DeserializeObject<JObject>(jsonText);
                  });
            runner.AddExternalMethod("loadJson", readfile);

            Assert.AreEqual(3, runner.Invoke("add(1,2)"));
            Assert.AreEqual("12", runner.Invoke("add('1','2')"));

            dynamic jobject = runner.Invoke("loadJson('Resources\\Json\\json9.json')");
            Assert.AreEqual("success", jobject.message.ToString());
        }

        [TestMethod]
        public void TestExport()
        {
            var runner = new ORELScriptRunner();
            runner.AddExternalMethod("loadJson", new Func<string, Task<JObject>>(LoadJson));
            var script = @"
data<-{ a:1, b:2 };

b<-data.a;

b+1;
";
            var result = runner.Invoke(script);
            Assert.AreEqual(2m, result);

            dynamic idList = runner.Invoke("data<-loadJson('Resources\\Json\\json9.json');data.data.data.notes=>{ id }");
            Assert.AreEqual("5d11f6e70000000027029667", idList[0].id);
        }

        [TestMethod]
        public void TestLoadCSV()
        {
            var runner = new ORELScriptRunner();
            runner.AddExternalMethod("loadAsList", new Func<string, List<IList<string>>>(path =>
            {
                var list = new List<IList<string>>();
                using (var reader = new StreamReader(path))
                using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    while (csvReader.Read())
                    {
                        list.Add(csvReader.Parser.Record);
                    }
                }
                return list;
            }));

            var script = @"
data<-loadAsList('Resources\\Csv\\csv1.csv');

data=>{
title: _[1],
number: _[2],
}

";
            runner.Invoke(script);
        }

        [TestMethod]
        public void TestComment()
        {
            var runner = new ORELScriptRunner();
            dynamic result = runner.Invoke(@"
#script start
v1<- 1+1; # set v1
#set v2
v2<- 2+2; # v1+v2
#set return value
#abc
{
    v1, v2
}
");
            Assert.AreEqual(2m, result.v1);
            Assert.AreEqual(4m, result.v2);
        }

        [TestMethod]
        public void TestListOperation()
        {
            var runner = new ORELScriptRunner();
            dynamic result;

            result = runner.Invoke(@"
            list1<-[2,3,4,5]=>{a: _};
            ");

            result = runner.Invoke(@"
            list1<-[[1,2,3,4],[2,3,4,5]]=>{ a:_[1],b:_[2] };
            list2<-[[5,6],[7,8]]=>{ c:_[1],b:$2 };
            ");
            Assert.AreEqual(5m, result[0].c);
            Assert.AreEqual(6m, result[0].b);
            Assert.AreEqual(7m, result[1].c);
            Assert.AreEqual(8m, result[1].b);

            result = runner.Invoke(@"
            list1<-[[1,2,3,4],[2,'3',4,5]]=>$2;
            ");
            Assert.AreEqual(2m, result[0]);
            Assert.AreEqual("3", result[1]);

            result = runner.Invoke(@"
list1<-[[{a1:1,b1:2,},{a2:'xxx',b2:'222'},3.1],[{a1:3,b1:4,},{a2:'yyy',b2:'zzz'},4.2]]=>{ r1:$1.a1, r2:$2.a2, r3:$3 };
");
            Assert.AreEqual(1m, result[0].r1);
            Assert.AreEqual("xxx", result[0].r2);
            Assert.AreEqual(3.1m, result[0].r3);
        }

        [TestMethod]
        public void TestZipList()
        {
            var runner = new ORELScriptRunner();

            var debug = runner.Debug(@"
            list1<-[[1,2,3,4],[2,3,4,5]]=>{ a:_[1],b:_[2] };
            list2<-[[5,6],[7,8]]=>{ c:_[1],b:$2 };
            list3<-[1,2,3]=>{e: _};
            result<-zip(list1,list2,list3);
            result=>{ a: $1.a, b: $1.b, d: $2.b, $3.e}
            ");
            var last = debug.Last();
            Assert.AreEqual(true, last.Success);
            Assert.AreEqual(1, ((dynamic)last.Result)[0].a);

        }

        [TestMethod]
        public void TestProduct()
        {
            var runner = new ORELScriptRunner();
            var stmts = @"
list1<-[1,2,3,4];
list2<-['a','b','c'];
cp<-product(list1,list2);
cp=>{ a: $1, b: $2 }
";
            var debug = runner.Debug(stmts);
            var last = debug.Last();
            Assert.AreEqual(true, last.Success);
            Assert.AreEqual(12, ((dynamic)last.Result).Count);
            Assert.AreEqual(1, ((dynamic)last.Result)[0].a);
            Assert.AreEqual("a", ((dynamic)last.Result)[0].b);
            Assert.AreEqual(4, ((dynamic)last.Result)[11].a);
            Assert.AreEqual("c", ((dynamic)last.Result)[11].b);
        }

        [TestMethod]
        public void TestForCtrip()
        {
            var runner = new ORELScriptRunner();
            runner.AddExternalMethod("loadCSV", new Func<string, List<IList<string>>>(path =>
            {
                var list = new List<IList<string>>();
                using (var reader = new StreamReader(path))
                using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    while (csvReader.Read())
                    {
                        list.Add(csvReader.Parser.Record);
                    }
                }
                return list;
            }));

            var stmts = @"
#从航线文件中加载起降城市列表
input<-loadcsv('Resources\csv\airline.csv');

#构建起降城市元组集合
city<-input=>{ start:$1, stop:$2 };

#构建30天日期序列
date<-range(today(),today()+'30d','1d');

#构建city与date的直积关系
cp<-product(city, date);

#输出爬取参数，字段名称对应Input参数Key
cp=>{ start_city: $1.start, stop_city: $1.stop, date: $2 }
";

            //debug
            var results = runner.Debug(stmts);
            Assert.AreEqual(5, results.Count);
            var last = results.Last();
            Assert.AreEqual(180, ((dynamic)last.Result).Count);
            Assert.AreEqual("广州", ((dynamic)last.Result)[179].start_city);
            Assert.AreEqual("郑州", ((dynamic)last.Result)[179].stop_city);
            Assert.AreEqual(DateTime.Now.AddDays(29).ToString("yyyyMMdd"), ((dynamic)last.Result)[179].date.ToString("yyyyMMdd"));
            //invoke
            //var result = runner.Invoke(stmts);
        }
    }
}
