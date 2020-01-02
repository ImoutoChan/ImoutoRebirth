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
    public class SourceFoldersController : ControllerBase
    {
        private readonly ISourceFolderRepository _sourceFolderRepository;
        private readonly IMapper _mapper;

        public SourceFoldersController(
            ISourceFolderRepository sourceFolderRepository,
            IMapper mapper)
        {
            _sourceFolderRepository = sourceFolderRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public async Task<ActionResult<SourceFolderResponse[]>> GetAll(Guid collectionGuid)
        {
            var sourceFolders = await _sourceFolderRepository.Get(collectionGuid);
            return _mapper.Map<SourceFolderResponse[]>(sourceFolders);
        }

        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Create))]
        public async Task<ActionResult<SourceFolderResponse>> Create(
            Guid collectionGuid, 
            [FromBody] SourceFolderCreateRequest request)
        {
            var createData = _mapper.Map<SourceFolderCreateData>((collectionGuid, request));

            var sourceFolder =  await _sourceFolderRepository.Add(createData);

            return Created("/", _mapper.Map<SourceFolderResponse>(sourceFolder));
        }

        [HttpDelete("{guid}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
        public async Task<ActionResult> Delete(Guid collectionGuid, Guid guid)
        {
            try
            {
                await _sourceFolderRepository.Remove(guid);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(e.Message);
            }

            return Ok();
        }
    }
}