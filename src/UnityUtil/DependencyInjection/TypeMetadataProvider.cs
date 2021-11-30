using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UnityEngine.DependencyInjection
{
    internal class TypeMetadataProvider : ITypeMetadataProvider
    {
        public Action<object> CompileMethodCall(string methodName, string paramName, MethodInfo injectMethod, object[] arguments)
        {
            ParameterExpression clientParam = Expression.Parameter(typeof(object), paramName);
            IEnumerable<Expression> dependencyArgs = injectMethod
                .GetParameters()
                .Select((param, p) => Expression.Constant(arguments[p], param.ParameterType));
            return Expression.Lambda<Action<object>>(
                body: Expression.Call(instance: Expression.Convert(clientParam, injectMethod.DeclaringType), injectMethod, dependencyArgs),
                name: methodName,
                parameters: new[] { clientParam }
            ).Compile();
        }

        public T? GetCustomAttribute<T>(ParameterInfo parameter) where T : Attribute => parameter.GetCustomAttribute<T>();

        public MethodInfo GetMethod(Type classType, string name, BindingFlags bindingFlags) => classType.GetMethod(name, bindingFlags);

        public ParameterInfo[] GetMethodParameters(MethodInfo method) => method.GetParameters();
    }

}
