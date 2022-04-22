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
    public async Task<IReadOnlyCollection<FileStatusResult>> GetAsync(IReadOnlyCollection<string> hashes)
    {
        return await _mediator.Send(new FilesStatusesQuery(hashes));
    }
}
