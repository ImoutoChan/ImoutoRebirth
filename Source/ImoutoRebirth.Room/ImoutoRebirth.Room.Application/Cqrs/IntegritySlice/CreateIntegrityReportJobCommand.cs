using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;

public record CreateIntegrityReportJobCommand(
    IReadOnlyCollection<Guid>? OnlyCollectionIds,
    string? ExportToFolder) : ICommand<Guid>;

internal class BuildIntegrityReportCommandHandler : ICommandHandler<CreateIntegrityReportJobCommand, Guid>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IIntegrityReportRepository _integrityReportRepository;
    private readonly IClock _clock;

    public BuildIntegrityReportCommandHandler(
        IMediator mediator,
        ICollectionRepository collectionRepository,
        IIntegrityReportExporter integrityReportExporter,
        ILoggerFactory loggerFactory,
        IIntegrityReportRepository integrityReportRepository,
        IClock clock)
    {
        _collectionRepository = collectionRepository;
        _integrityReportRepository = integrityReportRepository;
        _clock = clock;
    }

    public async Task<Guid> Handle(CreateIntegrityReportJobCommand command, CancellationToken ct)
    {
        var (onlyCollectionIds, exportToFolder) = command;

        var collections = await _collectionRepository.GetAll(ct);
        var now = _clock.GetCurrentInstant();

        if (onlyCollectionIds.SafeAny())
            collections = collections.Where(x => onlyCollectionIds.Contains(x.Id)).ToList();

        var newReport = IntegrityReport.Create(now, collections, exportToFolder);
        await _integrityReportRepository.Create(newReport);
        return newReport.ReportId;
    }
}
