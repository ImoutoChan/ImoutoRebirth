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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<SourceFolderResponse[]>> GetAll([BindRequired] Guid collectionId)
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SourceFolderResponse>> Create(
            [BindRequired] Guid collectionId, 
            [FromBody] SourceFolderCreateRequest request)
        {
            var createData = _mapper.Map<SourceFolderCreateData>((collectionId, request));

            var sourceFolder =  await _sourceFolderRepository.Add(createData);

            return Created("/", _mapper.Map<SourceFolderResponse>(sourceFolder));
        }

        /// <summary>
        /// Update the source folder for given collection.
        /// </summary>
        /// <param name="sourceFolderId">The id of the source folder that will be updated.</param>
        /// <param name="request">Source folder parameters.</param>
        /// <param name="collectionId">The collection id. Aren't needed and added only for routes consistency.</param>
        /// <returns>Updated source folder.</returns>
        [HttpPut("{sourceFolderId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<SourceFolderResponse>> Update(
            [BindRequired] Guid collectionId,
            [BindRequired] Guid sourceFolderId, 
            [FromBody] SourceFolderCreateRequest request)
        {
            var createData = _mapper.Map<SourceFolderUpdateData>((sourceFolderId, request));

            var sourceFolder =  await _sourceFolderRepository.Update(createData);

            return Created("/", _mapper.Map<SourceFolderResponse>(sourceFolder));
        }

        /// <summary>
        /// Delete the source folder.
        /// </summary>
        /// <param name="sourceFolderId">Id of the folder that will be deleted.</param>
        /// <param name="collectionId">The collection id. Aren't needed and added only for routes consistency.</param>
        [HttpDelete("{sourceFolderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Delete(
            [BindRequired] Guid collectionId, 
            [BindRequired] Guid sourceFolderId)
        {
            try
            {
                await _sourceFolderRepository.Remove(sourceFolderId);
            }
            catch (EntityNotFoundException e)
            {
                return NotFound(e.Message);
            }

            return Ok();
        }
    }
}