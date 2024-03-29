﻿using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.Domain.TagAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class TagsSearchQueryHandler : IQueryHandler<TagsSearchQuery, IReadOnlyCollection<Tag>>
{
    private const int DefaultLimit = 10;
    
    private readonly LilinDbContext _lilinDbContext;

    public TagsSearchQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<Tag>> Handle(TagsSearchQuery request, CancellationToken ct)
    {
        var (searchPattern, requestedLimit) = request;

        var limit = requestedLimit ?? DefaultLimit;
        
        var tagsWithTypes = _lilinDbContext.Tags
            .OrderByDescending(x => x.Count)
            .Include(x => x.Type);

        List<TagEntity> finalResult;
        if (string.IsNullOrEmpty(searchPattern))
        {
            finalResult = await tagsWithTypes.Take(limit).ToListAsync(cancellationToken: ct);
        }
        else
        {
            // uncomment after this is fixed https://github.com/dotnet/efcore/issues/32432 (8.0.2)
            // searchPattern = searchPattern.ToLower();

            var searchPattern1 = searchPattern.ToLower();
            var searchPattern2 = searchPattern.ToLower();
            var searchPattern3 = searchPattern.ToLower();

            finalResult = await tagsWithTypes
                .Where(x => x.Name.ToLower().Equals(searchPattern1))
                .Take(limit)
                .ToListAsync(cancellationToken: ct);

            if (finalResult.Count < limit)
            {
                var startsWith = await tagsWithTypes
                    .Where(x => !x.Name.ToLower().Equals(searchPattern1))
                    .Where(x => x.Name.ToLower().StartsWith(searchPattern2))
                    .Take(limit)
                    .ToListAsync(cancellationToken: ct);

                finalResult.AddRange(startsWith);
            }

            if (finalResult.Count < limit)
            {
                limit -= finalResult.Count;

                var contains = await tagsWithTypes
                    .Where(x => !x.Name.ToLower().Equals(searchPattern1))
                    .Where(x => !x.Name.ToLower().StartsWith(searchPattern2))
                    .Where(x => x.Name.ToLower().Contains(searchPattern3))
                    .Take(limit)
                    .ToListAsync(cancellationToken: ct);

                finalResult.AddRange(contains);
            }
        }

        return finalResult.Select(x => x.ToModel()).ToArray();
    }
}
