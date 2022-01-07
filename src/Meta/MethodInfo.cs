using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orel.Meta
{
    public class MethodInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ParameterInfo[] Parameters { get; set; }
        public string ReturnDescription { get; set; }
        public DataType ReturnType { get; set; }
    }

    public class ParameterInfo
    {
        public string Name { get; set; }
        public DataType DataType { get; set; }
        public string Description { get; set; }
    }

    public static class Methods
    {
        private static readonly List<MethodInfo> AllMethods;

        static Methods()
        {
            AllMethods = CreateMethodList().ToList();
        }

        private static IEnumerable<MethodInfo> CreateMethodList()
        {
            var methods = typeof(IntrinsicFunctions).GetMethods();
            foreach (var method in methods)
            {
                var methodNameAttr = method.GetCustomAttributes<MethodNameAttribute>().FirstOrDefault();
                if (methodNameAttr == null || methodNameAttr.IsInternal) continue;
                var parameters = method.GetParameters();
                var paramInfos = new ParameterInfo[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];
                    var paramInfo = new ParameterInfo()
                    {
                        Name = p.Name,
                        DataType = p.ParameterType.GetORELDataType(),
                        Description = Messages.ResourceManager.GetString($"Func_{method.Name}_Param{i + 1}")
                    };
                    paramInfos[i] = paramInfo;
                }

                yield return new MethodInfo()
                {
                    Name = methodNameAttr.Name,
                    Description = Messages.ResourceManager.GetString($"Func_{method.Name}_Name"),
                    ReturnDescription = Messages.ResourceManager.GetString($"Func_{method.Name}_Ret"),
                    ReturnType = method.ReturnType.GetORELDataType(),
                    Parameters = paramInfos
                };
            }
        }

        public static List<MethodInfo> All
        {
            get { return AllMethods; }
        }
    }
}
