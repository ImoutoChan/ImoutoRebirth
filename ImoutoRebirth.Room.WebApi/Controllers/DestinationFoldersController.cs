using AutoMapper;
using ImoutoRebirth.Room.DataAccess.Exceptions;
using ImoutoRebirth.Room.DataAccess.Models;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.WebApi.Requests;
using ImoutoRebirth.Room.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ImoutoRebirth.Room.WebApi.Controllers;

[Route("api/Collections/{collectionId}/[controller]")]
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

    /// <summary>
    /// Get the destination folder for collection.
    /// </summary>
    /// <param name="collectionId">The collection id.</param>
    /// <returns>The destination folder.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<DestinationFolderResponse>> Get([BindRequired] Guid collectionId)
    {
        var destinationFolder = await _destinationFolderRepository.Get(collectionId);

        if (destinationFolder == null)
            return NotFound();

        return _mapper.Map<DestinationFolderResponse>(destinationFolder);
    }

    /// <summary>
    /// CreateOrUpdate or update a destination folder for collection.
    /// </summary>
    /// <param name="collectionId">The collection id.</param>
    /// <param name="request">Destination folder parameters.</param>
    /// <returns>Created destination folder.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<DestinationFolderResponse>> CreateOrUpdate(
        [BindRequired] Guid collectionId,
        [FromBody] DestinationFolderCreateRequest request)
    {
        var createData = _mapper.Map<DestinationFolderCreateData>((collectionId, request));

        var destinationFolder = await _destinationFolderRepository.AddOrReplace(createData);

        return CreatedAtAction(
            nameof(Get),
            new { collectionGuid = collectionId },
            _mapper.Map<DestinationFolderResponse>(destinationFolder));
    }

    /// <summary>
    /// Delete the destination folder.
    /// </summary>
    /// <param name="collectionId">The collection id. Aren't needed and added only for routes consistency.</param>
    /// <param name="destinationFolderId">Id of the folder that will be deleted.</param>
    [HttpDelete("{destinationFolderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Delete(
        [BindRequired] Guid collectionId,
        [BindRequired] Guid destinationFolderId)
    {
        try
        {
            await _destinationFolderRepository.Remove(destinationFolderId);
        }
        catch (EntityNotFoundException e)
        {
            return NotFound(e.Message);
        }

        return Ok();
    }
}