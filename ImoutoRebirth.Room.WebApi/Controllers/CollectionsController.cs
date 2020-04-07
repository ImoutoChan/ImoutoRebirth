using System;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.WebApi.Requests;
using ImoutoRebirth.Room.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ImoutoRebirth.Room.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionsController : ControllerBase
    {
        private readonly ICollectionRepository _collectionRepository;
        private readonly IMapper _mapper;

        public CollectionsController(
            ICollectionRepository collectionRepository,
            IMapper mapper)
        {
            _collectionRepository = collectionRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieve all collections.
        /// </summary>
        /// <returns>The list of all collections.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CollectionResponse[]>> GetAll()
        {
            var collections = await _collectionRepository.GetAll();
            return _mapper.Map<CollectionResponse[]>(collections);
        }

        /// <summary>
        /// Create a new collection with the specific name.
        /// </summary>
        /// <param name="request">The parameters for the new collection.</param>
        /// <returns>A description of the created collection.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<CollectionResponse>> Create([FromBody] CollectionCreateRequest request)
        {
            var createData = _mapper.Map<CollectionCreateData>(request);

            var collection =  await _collectionRepository.Add(createData);

            return Created("/", _mapper.Map<CollectionResponse>(collection));
        }

        /// <summary>
        /// Rename the collection with the provided id.
        /// </summary>
        /// <param name="id">The collection id.</param>
        /// <param name="newName">The new name for the collection.</param>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Rename([BindRequired] Guid id, [BindRequired]string newName)
        {
            try
            {
                await _collectionRepository.Rename(id, newName);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(e.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Delete the collection with the provided id.
        /// </summary>
        /// <param name="id">The collection id.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Delete([BindRequired] Guid id)
        {
            try
            {
                await _collectionRepository.Remove(id);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(e.Message);
            }

            return Ok();
        }
    }
}
