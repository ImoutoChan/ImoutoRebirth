using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
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
        public async Task<ActionResult<CollectionResponse[]>> GetAll()
        {
            var collections = await _collectionRepository.GetCollections();
            return _mapper.Map<CollectionResponse[]>(collections);
        }
    }
}
