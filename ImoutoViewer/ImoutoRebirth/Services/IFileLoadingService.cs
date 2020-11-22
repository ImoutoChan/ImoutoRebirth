﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services
{
    internal interface IFileLoadingService
    {
        Task<IList<string>> LoadFiles(
            Guid? collectionId,
            IReadOnlyCollection<SearchTag> searchTags,
            CancellationToken cancellationToken = default);
    }
}