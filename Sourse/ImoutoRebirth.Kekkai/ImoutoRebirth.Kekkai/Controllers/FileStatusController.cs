using ImoutoRebirth.Kekkai.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ImoutoRebirth.Kekkai.Controllers;

[ApiController]
[Route("[controller]")]
public class FileStatusController : ControllerBase
{
    private readonly IMediator _mediator;

    public FileStatusController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public IAsyncEnumerable<FileStatusResult> GetAsync(IReadOnlyCollection<string> hashes) 
        => _mediator.CreateStream(new FilesStatusesQuery(hashes.Distinct().ToList()));
}
