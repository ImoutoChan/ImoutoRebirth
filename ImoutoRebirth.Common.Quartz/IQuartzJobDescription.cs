using Quartz;

namespace ImoutoRebirth.Common.Quartz
{
    public interface IQuartzJobDescription
    {
        IJobDetail GetJobDetails();

        ITrigger GetJobTrigger();
    }
}