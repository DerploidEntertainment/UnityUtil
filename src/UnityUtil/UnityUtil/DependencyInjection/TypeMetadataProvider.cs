using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UnityUtil.DependencyInjection;

internal class TypeMetadataProvider : ITypeMetadataProvider
{
    public Action<object> CompileMethodCall(string methodName, string paramName, MethodInfo method, object[] arguments)
    {
        ParameterExpression clientParam = Expression.Parameter(typeof(object), paramName);
        IEnumerable<Expression> argExprs = method
            .GetParameters()
            .Select((param, p) => Expression.Constant(arguments[p], param.ParameterType));

        // Return lambda: (object client) => ((methodDeclaringType)client).Method(arg1, arg2, ...)
        return Expression.Lambda<Action<object>>(
            body: Expression.Call(instance: Expression.Convert(clientParam, method.DeclaringType), method, argExprs),
            name: methodName,
            parameters: new[] { clientParam }
        ).Compile();
    }

    public Func<object> CompileConstructorCall(ConstructorInfo constructor, object[] arguments)
    {
        IEnumerable<Expression> argExprs = constructor
            .GetParameters()
            .Select((param, p) => Expression.Constant(arguments[p], param.ParameterType));

        // Return lambda: () => new constructorDeclaringType(arg1, arg2, ...)
        return Expression.Lambda<Func<object>>(body: Expression.New(constructor, argExprs)).Compile();
    }

    public T? GetCustomAttribute<T>(ParameterInfo parameter) where T : Attribute => parameter.GetCustomAttribute<T>();

    public MethodInfo GetMethod(Type classType, string name, BindingFlags bindingFlags) => classType.GetMethod(name, bindingFlags);

    public ConstructorInfo[] GetConstructors(Type classType) => classType.GetConstructors();

    public ParameterInfo[] GetMethodParameters(MethodBase method) => method.GetParameters();
}
