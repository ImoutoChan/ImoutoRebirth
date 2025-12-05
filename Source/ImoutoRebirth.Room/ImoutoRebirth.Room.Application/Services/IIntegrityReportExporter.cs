using ImoutoRebirth.Room.Domain.IntegrityAggregate;

namespace ImoutoRebirth.Room.Application.Services;

public interface IIntegrityReportExporter
{
    Task<IReadOnlyCollection<string>> ExportReportToFiles(IntegrityReportCollection report, string exportToFolder);
}
