using System;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Lilin.Services.CQRS.Queries;
using ImoutoRebirth.Lilin.WebApi.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ImoutoRebirth.Lilin.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public FilesController(IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _mediator = mediator;
        }

        /// <summary>
        ///     Retrieve file info for id.
        /// </summary>
        /// <returns>File info result.</returns>
        [HttpGet]
        public async Task<ActionResult<FileInfoResponse>> GetFileInfo([BindRequired] Guid fileId)
        {
            var fileInfo = await _mediator.Send(new FileInfoQuery(fileId));
            return _mapper.Map<FileInfoResponse>(fileInfo);
        }
    }
}
