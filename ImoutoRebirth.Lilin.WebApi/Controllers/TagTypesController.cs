using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Lilin.Services.CQRS.Queries;
using ImoutoRebirth.Lilin.WebApi.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ImoutoRebirth.Lilin.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagTypesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public TagTypesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        ///     Retrieve all tag types.
        /// </summary>
        /// <returns>The collection of all tag types.</returns>
        [HttpGet]
        public async Task<ActionResult<TagTypeResponse[]>> GetAll()
        {
            var tagTypes = await _mediator.Send(new TagTypesQuery());
            return _mapper.Map<TagTypeResponse[]>(tagTypes);
        }
    }
}