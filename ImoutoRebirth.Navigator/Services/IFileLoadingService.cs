using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.ViewModel;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    internal interface IFileLoadingService
    {
        Task LoadFiles(
            int bulkFactor, 
            int previewSize,
            Guid? collectionId,
            IReadOnlyCollection<SearchTag> searchTags,
            Action<int> counterUpdater, 
            Action<IReadOnlyCollection<INavigatorListEntry>, CancellationToken> entryUpdater,
            Action rollbackAction,
            Action initAction,
            Action finishAction);
    }
}