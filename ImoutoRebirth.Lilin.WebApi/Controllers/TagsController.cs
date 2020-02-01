using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Lilin.Services.CQRS.Commands;
using ImoutoRebirth.Lilin.Services.CQRS.Queries;
using ImoutoRebirth.Lilin.WebApi.Requests;
using ImoutoRebirth.Lilin.WebApi.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ImoutoRebirth.Lilin.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public TagsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        ///     Search for tags.
        /// </summary>
        /// <returns>The collection of found tags.</returns>
        [HttpPost("search")]
        public async Task<ActionResult<TagResponse[]>> Search([FromBody] TagsSearchRequest request)
        {
            var tags = await _mediator.Send(new TagsSearchQuery(request.SearchPattern, request.Count));
            return _mapper.Map<TagResponse[]>(tags);
        }
        
        /// <summary>
        ///     Create a tag.
        /// </summary>
        /// <returns>Created tag.</returns>
        [HttpPost]
        public async Task<ActionResult<TagResponse>> Create([FromBody] TagCreateRequest request)
        {
            var query = _mapper.Map<CreateTagCommand>(request);
            var created = await _mediator.Send(query);
            return _mapper.Map<TagResponse>(created);
        }
    }
}