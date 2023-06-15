using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace ImoutoRebirth.Common.WebApi.Client;

public static class ClientLambdaActivator
{
    private static readonly ConcurrentDictionary<Type, Delegate> ConstructorsCache = new();

    public static TClient CreateClient<TClient>(params object[] args)
    {
        if (!ConstructorsCache.TryGetValue(typeof(TClient), out var activator))
        {
            activator = GetActivator<TClient>(args.Length);
            ConstructorsCache.TryAdd(typeof(TClient), activator);
        }

        return ((ObjectActivator<TClient>)activator)(args);
    }

    private static Delegate GetActivator<TClient>(int parametersLength)
    {
        var constructor = typeof(TClient)
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .First(x => x.GetParameters().Length == parametersLength);

        var paramsInfo = constructor.GetParameters();

        var param = Expression.Parameter(typeof(object[]), "args");

        var argsExp = new Expression[paramsInfo.Length];

        for (var i = 0; i < paramsInfo.Length; i++)
        {
            var index = Expression.Constant(i);
            var paramType = paramsInfo[i].ParameterType;

            var paramAccessorExp = Expression.ArrayIndex(param, index);
            var paramCastExp = Expression.Convert(paramAccessorExp, paramType);

            argsExp[i] = paramCastExp;
        }

        var newExp = Expression.New(constructor, argsExp);
        var lambda = Expression.Lambda(typeof(ObjectActivator<TClient>), newExp, param);

        return lambda.Compile();
    }

    private delegate TClient ObjectActivator<out TClient>(params object[] args);
}
