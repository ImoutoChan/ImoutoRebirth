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
    [Route("api/Collections/{collectionGuid}/[controller]")]
    [ApiController]
    public class DestinationFolderController : ControllerBase
    {
        private readonly IDestinationFolderRepository _destinationFolderRepository;
        private readonly IMapper _mapper;

        public DestinationFolderController(
            IDestinationFolderRepository destinationFolderRepository,
            IMapper mapper)
        {
            _destinationFolderRepository = destinationFolderRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<ActionResult<DestinationFolderResponse>> Get(Guid collectionGuid)
        {
            var destinationFolder = await _destinationFolderRepository.Get(collectionGuid);

            if (destinationFolder == null)
                return NotFound();

            return _mapper.Map<DestinationFolderResponse>(destinationFolder);
        }

        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Create))]
        public async Task<ActionResult<DestinationFolderResponse>> Create(
            Guid collectionGuid,
            [FromBody] DestinationFolderCreateRequest request)
        {
            var createData = _mapper.Map<DestinationFolderCreateData>((collectionGuid, request));

            var destinationFolder = await _destinationFolderRepository.AddOrReplace(createData);

            return CreatedAtAction(
                nameof(Get),
                new { collectionGuid = collectionGuid },
                _mapper.Map<DestinationFolderResponse>(destinationFolder));
        }

        [HttpDelete("{guid}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
        public async Task<ActionResult> Delete(Guid collectionGuid, Guid guid)
        {
            try
            {
                await _destinationFolderRepository.Remove(guid);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(e.Message);
            }

            return Ok();
        }
    }
}