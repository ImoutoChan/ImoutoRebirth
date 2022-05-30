using System.Threading.Tasks;

namespace ImoutoRebirth.Meido.Core.FaultTolerance;

public interface IFaultToleranceService
{
    Task RequeueFaults();
}