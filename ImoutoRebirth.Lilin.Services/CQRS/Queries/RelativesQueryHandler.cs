using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using MediatR;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class RelativesQueryHandler : IQueryHandler<RelativesQuery, IReadOnlyCollection<RelativeInfo>>
    {
        private readonly IMediator _mediator;

        public RelativesQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IReadOnlyCollection<RelativeInfo>> Handle(
            RelativesQuery request, 
            CancellationToken cancellationToken)
        {
            // todo add cache
            var parentTag = await GetParentTag(cancellationToken);
            var childTag = await GetChildTag(cancellationToken);

            var md5 = request.Md5.ToLowerInvariant();

            var parents = GetFileInfoByTagValueRelativeInfo(parentTag.Id, md5, cancellationToken)
                .Select(x => new RelativeInfo(RelativeType.Parent, x));

            var children = GetFileInfoByTagValueRelativeInfo(childTag.Id, md5, cancellationToken)
                .Select(x => new RelativeInfo(RelativeType.Child, x));

            return await parents.Union(children).ToArrayAsync(cancellationToken);
        }

        private async IAsyncEnumerable<FileInfo> GetFileInfoByTagValueRelativeInfo(
            Guid tagId, 
            string value, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var filesQuery = new FilesSearchQuery(
                new[] {new TagSearchEntry(tagId, value, TagSearchScope.Included)});

            var found = await _mediator.Send(filesQuery, cancellationToken);


            // todo read about cancellation in IAsyncEnumerable
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var guid in found)
            {
                var fileInfo = await _mediator.Send(new FileInfoQuery(guid), cancellationToken);
                yield return fileInfo;
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
 
        private async Task<Tag> GetChildTag(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new TagsSearchQuery("Child", limit: 1), cancellationToken);
            return result.Single();
        }

        private async Task<Tag> GetParentTag(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new TagsSearchQuery("ParentMd5", limit: 1), cancellationToken);
            return result.Single();
        }
    }
}