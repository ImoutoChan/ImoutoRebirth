using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Navigator.Services.Collections;

internal class SourceFolderService : ISourceFolderService
{
    private readonly ISourceFolders _sourceFolders;
    private readonly IMapper _mapper;

    public SourceFolderService(ISourceFolders sourceFolders, IMapper mapper)
    {
        _sourceFolders = sourceFolders;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<SourceFolder>> GetSourceFoldersAsync(Guid collectionId)
    {
        var result = await _sourceFolders.GetAllAsync(collectionId);
        return _mapper.Map<IReadOnlyCollection<SourceFolder>>(result);
    }

    public async Task<SourceFolder> AddSourceFolderAsync(SourceFolder sourceFolder)
    {
        var request = _mapper.Map<SourceFolderCreateRequest>(sourceFolder);
        var result = await _sourceFolders.CreateAsync(sourceFolder.CollectionId, request);
        return _mapper.Map<SourceFolder>(result);
    }

    public async Task<SourceFolder> UpdateSourceFolderAsync(SourceFolder sourceFolder)
    {
        if (!sourceFolder.Id.HasValue)
            throw new ArgumentException("Can't update new collection.", nameof(sourceFolder));

        var request = _mapper.Map<SourceFolderCreateRequest>(sourceFolder);
        var result = await _sourceFolders.UpdateAsync(sourceFolder.CollectionId, sourceFolder.Id.Value, request);
        return _mapper.Map<SourceFolder>(result);
    }

    public Task DeleteSourceFolderAsync(Guid collectionId, Guid sourceFolderId)
    {
        return _sourceFolders.DeleteAsync(collectionId, sourceFolderId);
    }
}