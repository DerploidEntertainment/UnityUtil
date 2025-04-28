using System;
using System.Reflection;

namespace UnityUtil.DependencyInjection;

/// <summary>
/// This type is used internally by the DI system. You should never need to implement it in your own code.
/// </summary>
public interface ITypeMetadataProvider
{
    ParameterInfo[] GetMethodParameters(MethodBase method);
    T? GetCustomAttribute<T>(ParameterInfo parameter) where T : Attribute;
    MethodInfo GetMethod(Type classType, string name, BindingFlags bindingFlags);
    ConstructorInfo[] GetConstructors(Type classType);
    Action<object> CompileMethodCall(string methodName, string paramName, MethodInfo method, object[] arguments);
    Func<object> CompileConstructorCall(ConstructorInfo constructor, object[] arguments);
}
