using System;
using System.Collections.Generic;
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
        ///     Retrieve file info with all tags and notes by id.
        /// </summary>
        /// <returns>File info result.</returns>
        [HttpGet]
        public async Task<ActionResult<FileInfoResponse>> GetFileInfo([BindRequired] Guid fileId)
        {
            var fileInfo = await _mediator.Send(new FileInfoQuery(fileId));
            return _mapper.Map<FileInfoResponse>(fileInfo);
        }

        /// <summary>
        ///     Check all parent and child tags in the database to see if there are related images for given md5 hash.
        /// </summary>
        /// <returns>
        ///     The collection of relatives types for file and correspondent file info with their tags and notes.
        /// </returns>
        [HttpGet("relatives")]
        public async Task<ActionResult<RelativeResponse[]>> GetRelatives([BindRequired] string md5)
        {
            var fileInfo = await _mediator.Send(new RelativesQuery(md5));
            return _mapper.Map<RelativeResponse[]>(fileInfo);
        }
    }
}
