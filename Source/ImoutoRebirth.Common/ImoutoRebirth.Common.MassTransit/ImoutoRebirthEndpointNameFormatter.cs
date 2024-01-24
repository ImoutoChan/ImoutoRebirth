using System.Text;
using System.Text.RegularExpressions;
using MassTransit;
using MassTransit.Metadata;
using MassTransit.NewIdFormatters;

namespace ImoutoRebirth.Common.MassTransit;

internal partial class ImoutoRebirthEndpointNameFormatter : IEndpointNameFormatter
{
    private const int MaxTemporaryQueueNameLength = 72;
    private const int OverheadLength = 29;
    private const string JoinSeparator = "-";
    
    public ImoutoRebirthEndpointNameFormatter(string consumingServiceName) 
        => ConsumingServiceNamePrefix = SanitizeName(consumingServiceName);
    
    public ImoutoRebirthEndpointNameFormatter(Type serviceNameFromType)
    {
        var serviceName = ExtractOwnerServiceNameFromConsumerPrefix(serviceNameFromType);
        ConsumingServiceNamePrefix = SanitizeName(serviceName);
    }

    private string ConsumingServiceNamePrefix { get; }

    public string Separator { get; } = JoinSeparator;

    public string TemporaryEndpoint(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            tag = "endpoint";

        var host = HostMetadataCache.Host;

        var machineName = NonAlpha().Replace(host.MachineName!, "");
        var machineNameLength = machineName.Length;

        var processName = NonAlpha().Replace(host.ProcessName!, "");
        var processNameLength = processName.Length;

        var tagLength = tag.Length;
        var nameLength = machineNameLength + processNameLength + tagLength + OverheadLength;
        var overage = nameLength - MaxTemporaryQueueNameLength;

        const int spread = (MaxTemporaryQueueNameLength - OverheadLength) / 3;
        if (overage > 0 && machineNameLength > spread)
        {
            overage -= machineNameLength - spread;
            machineNameLength = spread;
        }

        if (overage > 0 && processNameLength > spread)
        {
            overage -= processNameLength - spread;
            processNameLength = spread;
        }

        if (overage > 0 && tagLength > spread)
        {
            overage -= tagLength - spread;
            tagLength = spread;
        }

        var sb = new StringBuilder(machineNameLength + processNameLength + tagLength + OverheadLength);

        sb.Append(machineName, 0, machineNameLength);
        sb.Append('_');
        sb.Append(processName, 0, processNameLength);

        sb.Append('_');
        sb.Append(tag, 0, tagLength);
        sb.Append('_');
        sb.Append(NewId.Next().ToString(ZBase32Formatter.LowerCase));

        return sb.ToString();
    }

    public string Consumer<T>() where T : class, IConsumer => GetConsumerName(typeof(T));

    public string Message<T>() where T : class => GetMessageName(typeof(T));

    public string Saga<T>() where T : class, ISaga => GetSagaName(typeof(T));

    public string ExecuteActivity<T, TArguments>()
        where T : class, IExecuteActivity<TArguments>
        where TArguments : class
    {
        var activityName = GetActivityName(typeof(T));

        return $"{activityName}_execute";
    }

    public string CompensateActivity<T, TLog>()
        where T : class, ICompensateActivity<TLog>
        where TLog : class
    {
        var activityName = GetActivityName(typeof(T));

        return $"{activityName}_compensate";
    }

    public string SanitizeName(string name) =>
        SeparatorReplacePattern().Replace(name, m => JoinSeparator + m.Value).ToLowerInvariant();

    private string GetConsumerName(Type type)
    {
        var ownerServiceNamePrefix = ExtractOwnerServiceNameFromConsumerPrefix(type);

        var prefix = ConsumingServiceNamePrefix == ownerServiceNamePrefix
            ? ConsumingServiceNamePrefix
            : ConsumingServiceNamePrefix + "_" + ownerServiceNamePrefix;
        
        if (type.IsGenericType && type.Name.Contains('`'))
        {
            return prefix + "_" + SanitizeName(FormatName(type.GetGenericArguments().Last()));
        }

        const string consumer = "Consumer";

        var consumerName = FormatName(type);

        if (consumerName.EndsWith(consumer, StringComparison.InvariantCultureIgnoreCase))
        {
            consumerName = consumerName[..^consumer.Length];

            if (string.IsNullOrWhiteSpace(consumerName))
                throw new ConfigurationException(
                    $"A consumer may not be named \"{consumer}\". Add a meaningful prefix when using ConfigureEndpoints.");
        }

        return prefix + "_" + SanitizeName(consumerName);
    }

    private string GetMessageName(Type type)
    {
        if (type.IsGenericType && type.Name.Contains('`'))
            return ConsumingServiceNamePrefix + "_" + SanitizeName(FormatName(type.GetGenericArguments().Last()));

        return ConsumingServiceNamePrefix + "_" + SanitizeName(FormatName(type));
    }
    private string GetSagaName(Type type)
    {
        const string saga = "Saga";

        var sagaName = FormatName(type);

        if (sagaName.EndsWith(saga, StringComparison.InvariantCultureIgnoreCase))
        {
            sagaName = sagaName[..^saga.Length];

            if (string.IsNullOrWhiteSpace(sagaName))
                throw new ConfigurationException(
                    $"A saga may not be named \"{saga}\". Add a meaningful prefix when using ConfigureEndpoints.");
        }

        return ConsumingServiceNamePrefix + "_" + SanitizeName(sagaName);
    }

    private string GetActivityName(Type activityType)
    {
        const string activity = "Activity";

        var activityName = FormatName(activityType);

        if (activityName.EndsWith(activity, StringComparison.InvariantCultureIgnoreCase))
        {
            activityName = activityName[..^activity.Length];

            if (string.IsNullOrWhiteSpace(activityName))
                throw new ConfigurationException(
                    $"An activity may not be named \"{activity}\". Add a meaningful prefix when using ConfigureEndpoints.");
        }

        return ConsumingServiceNamePrefix + "_" + SanitizeName(activityName);
    }

    private static string FormatName(Type type)
    {
        return type.IsInterface && InterfaceRegex().IsMatch(type.Name)
            ? type.Name[1..] // type is interface and looks like ISomeInterface
            : type.Name;
    }

    /// <summary>
    /// Owner service is the service that declare that message type. It's producer for events and consumer for commands. 
    /// </summary>
    private string ExtractOwnerServiceNameFromConsumerPrefix(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IConsumer<>))
        {
            return SanitizeName(type.GetGenericArguments().First().Namespace!.Split('.').Skip(1).First());
        }
        
        var consumedMessageType = type.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IConsumer<>))
            .Select(x => x.GetGenericArguments().First())
            .First();

        var serviceName = consumedMessageType.Namespace!.Split('.').Skip(1).First();
        return SanitizeName(serviceName);
    }
    
    [GeneratedRegex("[^a-zA-Z0-9]", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.NonBacktracking)]
    private static partial Regex NonAlpha();

    [GeneratedRegex("(?<=[a-z0-9])[A-Z]", RegexOptions.Compiled)]
    private static partial Regex SeparatorReplacePattern();
    
    [GeneratedRegex("^I[A-Z]", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.NonBacktracking)]
    private static partial Regex InterfaceRegex();
}
