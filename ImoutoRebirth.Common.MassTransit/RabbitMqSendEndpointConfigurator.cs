namespace ImoutoRebirth.Common.MassTransit
{
    public class RabbitMqSendEndpointConfigurator
    {
        public bool Durable { get; set; } = true;

        public bool AutoDelete { get; set; } = false;

        // ReSharper disable once StringLiteralTypo
        internal string GetUrlParams() 
            => $"?durable={Durable.ToString().ToLower()}&autodelete={AutoDelete.ToString().ToLower()}";
    }
}