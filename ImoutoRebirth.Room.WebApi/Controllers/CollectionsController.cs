using System;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.WebApi.Requests;
using ImoutoRebirth.Room.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<ActionResult<CollectionResponse[]>> GetAll()
        {
            var collections = await _collectionRepository.GetAll();
            return _mapper.Map<CollectionResponse[]>(collections);
        }

        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Create))]
        public async Task<ActionResult<CollectionResponse>> Create([FromBody] CollectionCreateRequest request)
        {
            var createData = _mapper.Map<CollectionCreateData>(request);

            var collection =  await _collectionRepository.Add(createData);

            return Created("/", _mapper.Map<CollectionResponse>(collection));
        }

        [HttpDelete("{guid}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
        public async Task<ActionResult> Delete(Guid guid)
        {
            try
            {
                await _collectionRepository.Remove(guid);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(e.Message);
            }

            return Ok();
        }
    }
}
