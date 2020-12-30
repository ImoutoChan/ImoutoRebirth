using System;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.DataAccess.Repositories.Queries;
using ImoutoRebirth.Room.WebApi.Requests;
using ImoutoRebirth.Room.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ImoutoRebirth.Room.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionFilesController : ControllerBase
    {
        private readonly ICollectionFileRepository _collectionFileRepository;
        private readonly ILocationTagsUpdaterService _locationTagsUpdaterService;
        private readonly IMapper _mapper;

        public CollectionFilesController(
            ICollectionFileRepository collectionFileRepository,
            IMapper mapper,
            ILocationTagsUpdaterService locationTagsUpdaterService)
        {
            _collectionFileRepository = collectionFileRepository;
            _mapper = mapper;
            _locationTagsUpdaterService = locationTagsUpdaterService;
        }

        /// <summary>
        ///     Retrieve all files by request.
        /// </summary>
        /// <returns>The collection of files.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CollectionFileResponse[]>> Search(
            [FromBody] CollectionFilesRequest request)
        {
            var query = _mapper.Map<CollectionFilesQuery>(request);
            var files = await _collectionFileRepository.SearchByQuery(query);
            return _mapper.Map<CollectionFileResponse[]>(files);
        }

        /// <summary>
        ///     Retrieve count of files by request.
        /// </summary>
        /// <remarks>
        ///     Note that Skip and Count fields are ignored.
        /// </remarks>
        /// <returns>The count of files that was found by request.</returns>
        [HttpPost("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Count(
            [FromBody] CollectionFilesRequest request)
        {
            var query = _mapper.Map<CollectionFilesQuery>(request);
            var count = await _collectionFileRepository.CountByQuery(query);
            return count;
        }

        /// <summary>
        ///     Get new tags.
        /// </summary>
        /// <remarks>
        ///     Note that Skip and Count fields are ignored.
        /// </remarks>
        /// <returns>The count of files that was found by request.</returns>
        [HttpPost("updateSourceTags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task UpdateSourceTags() => await _locationTagsUpdaterService.UpdateLocationTags();

        /// <summary>
        ///     Remove file with id.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Remove([BindRequired] Guid id)
        {
            try
            {
                await _collectionFileRepository.Remove(id);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(e.Message);
            }

            return Ok();
        }
    }
}