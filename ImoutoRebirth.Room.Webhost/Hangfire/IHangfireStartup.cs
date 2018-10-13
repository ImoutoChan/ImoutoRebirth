namespace ImoutoRebirth.Room.Webhost.Hangfire
{
    internal interface IHangfireStartup
    {
        void EnqueueJobs();
    }
}