using System;
using System.Collections.Generic;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.ImoutoViewer
{
    internal interface IImoutoViewerService
    {
        void OpenFile(string path, Guid collectionId, IEnumerable<SearchTag> searchTags);
    }
}