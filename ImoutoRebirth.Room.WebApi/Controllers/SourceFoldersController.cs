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
    [Route("api/Collections/{collectionId}/[controller]")]
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

        /// <summary>
        /// Get all source folders for collection.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <returns>The collection of source folders.</returns>
        [HttpGet]
        public async Task<ActionResult<SourceFolderResponse[]>> GetAll(Guid collectionId)
        {
            var sourceFolders = await _sourceFolderRepository.Get(collectionId);
            return _mapper.Map<SourceFolderResponse[]>(sourceFolders);
        }

        /// <summary>
        /// Create a source folder for collection.
        /// </summary>
        /// <param name="collectionId">The collection id.</param>
        /// <param name="request">Source folder parameters.</param>
        /// <returns>Created source folder.</returns>
        [HttpPost]
        public async Task<ActionResult<SourceFolderResponse>> Create(
            Guid collectionId, 
            [FromBody] SourceFolderCreateRequest request)
        {
            var createData = _mapper.Map<SourceFolderCreateData>((collectionId, request));

            var sourceFolder =  await _sourceFolderRepository.Add(createData);

            return Created("/", _mapper.Map<SourceFolderResponse>(sourceFolder));
        }

        /// <summary>
        /// Delete the source folder.
        /// </summary>
        /// <param name="guid">Id of the folder that will be deleted.</param>
        [HttpDelete("{guid}")]
        public async Task<ActionResult> Delete(Guid guid)
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