using System;
using System.Reflection;

namespace UnityEngine.DependencyInjection
{
    internal interface ITypeMetadataProvider
    {
        ParameterInfo[] GetMethodParameters(MethodInfo method);
        T GetCustomAttribute<T>(ParameterInfo element) where T : Attribute;
        MethodInfo GetMethod(Type classType, string name, BindingFlags bindingAttr);
        Action<object> CompileMethodCall(string methodName, string paramName, MethodInfo injectMethod, object[] arguments);
    }

}
