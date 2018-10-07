namespace ImoutoRebirth.Room.Host.Environment
{
    public interface IEnvironmentProvider
    {
        EnvironmentType Environment { get; }

        string ServiceName { get; }
    }
}