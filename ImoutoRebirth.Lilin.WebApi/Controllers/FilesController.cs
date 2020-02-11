using System;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Lilin.Services.CQRS.Commands;
using ImoutoRebirth.Lilin.Services.CQRS.Queries;
using ImoutoRebirth.Lilin.WebApi.Requests;
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
        [HttpGet("{fileId}")]
        public async Task<ActionResult<FileInfoResponse>> GetFileInfo(Guid fileId)
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

        /// <summary>
        ///     Get all files that's contains certain tags and values.
        /// </summary>
        /// <returns>
        ///     The collection of all found files. Pagination available by count/skip parameters.
        /// </returns>
        [HttpPost("search")]
        public async Task<ActionResult<Guid[]>> GetFilesByTags([FromBody] FilesSearchRequest request)
        {
            var query = _mapper.Map<FilesSearchQuery>(request);

            var fileIds = await _mediator.Send(query);

            return fileIds;
        }

        /// <summary>
        ///     Get count of files that's contains certain tags and values.
        /// </summary>
        /// <returns>
        ///     The number of found files for given tags.
        /// </returns>
        [HttpPost("search/count")]
        public async Task<ActionResult<int>> GetFilesCountByTags([FromBody] FilesSearchRequest request)
        {
            var query = _mapper.Map<FilesSearchQueryCount>(request);
            return await _mediator.Send(query);
        }

        /// <summary>
        ///     Create FileTagInfo and bind tags to files with specified values.
        /// </summary>
        [HttpPost("tags")]
        public async Task<ActionResult> BindTagsToFiles([FromBody] BindTagsRequest request)
        {
            var command = _mapper.Map<BindTagsCommand>(request);
            await _mediator.Send(command);

            return Ok();
        }

        /// <summary>
        ///     Remove tag from file with given value.
        /// </summary>
        [HttpDelete]
        public async Task<ActionResult> UnbindTagFromFile([FromBody] UnbindTagRequest request)
        {
            var command = _mapper.Map<UnbindTagCommand>(request);
            await _mediator.Send(command);

            return Ok();
        }
    }
}
