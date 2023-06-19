using AutoMapper;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.DataAccess.Repositories.Queries;
using ImoutoRebirth.Room.WebApi.Requests;
using ImoutoRebirth.Room.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ImoutoRebirth.Room.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CollectionFilesController : ControllerBase
{
    private readonly ICollectionFileRepository _collectionFileRepository;
    private readonly ICollectionFileService _collectionFileService;
    private readonly ILocationTagsUpdaterService _locationTagsUpdaterService;
    private readonly IMapper _mapper;

    public CollectionFilesController(
        ICollectionFileRepository collectionFileRepository,
        IMapper mapper,
        ILocationTagsUpdaterService locationTagsUpdaterService,
        ICollectionFileService collectionFileService)
    {
        _collectionFileRepository = collectionFileRepository;
        _mapper = mapper;
        _locationTagsUpdaterService = locationTagsUpdaterService;
        _collectionFileService = collectionFileService;
    }

    /// <summary>
    ///     Retrieve all files by request.
    /// </summary>
    /// <returns>The collection of files.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionFileResponse[]>> Search(
        [FromBody] CollectionFilesRequest request)
    {
        var query = _mapper.Map<CollectionFilesQuery>(request);
        var files = await _collectionFileRepository.SearchByQuery(query);
        return _mapper.Map<CollectionFileResponse[]>(files);
    }

    /// <summary>
    ///     Retrieve all file ids by request.
    /// </summary>
    /// <returns>The collection of file ids.</returns>
    [HttpPost("search-ids")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IReadOnlyCollection<Guid>> SearchIds(
        [FromBody] CollectionFilesRequest request)
    {
        var query = _mapper.Map<CollectionFilesQuery>(request);
        return await _collectionFileRepository.SearchIdsByQuery(query);
    }

    /// <summary>
    ///     Retrieve count of files by request.
    /// </summary>
    /// <remarks>
    ///     Note that Skip and Count fields are ignored.
    /// </remarks>
    /// <returns>The count of files that was found by request.</returns>
    [HttpPost("count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> Count(
        [FromBody] CollectionFilesRequest request)
    {
        var query = _mapper.Map<CollectionFilesQuery>(request);
        var count = await _collectionFileRepository.CountByQuery(query);
        return count;
    }

    /// <summary>
    ///     Get new tags.
    /// </summary>
    /// <remarks>
    ///     Note that Skip and Count fields are ignored.
    /// </remarks>
    /// <returns>The count of files that was found by request.</returns>
    [HttpPost("updateSourceTags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task UpdateSourceTags() => await _locationTagsUpdaterService.UpdateLocationTags();

    /// <summary>
    ///     Remove file with id.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task Remove([BindRequired] Guid id)
    {
        await _collectionFileService.Delete(id);
    }
}
