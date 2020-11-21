using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.Utils;

namespace ImoutoRebirth.Navigator.Services.ImoutoViewer
{
    internal class ImoutoViewerService : IImoutoViewerService
    {
        public void OpenFile(string path, Guid collectionId, IEnumerable<SearchTag> searchTags)
        {
            var args = new ImoutoViewerArgs(
                collectionId, 
                GetDto(searchTags).ToArray());

            var base64Args = Base64Serializer.Serialize(args);

            try
            {
                var myProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = Associations.AssocQueryString(Associations.AssocStr.ASSOCSTR_EXECUTABLE,
                            "." + path.Split('.').Last()),
                        Arguments = path + $" -nav-search={base64Args}"
                    }
                };
                myProcess.Start();
            }
            catch
            {
                Process.Start(path);
            }
        }

        private static IEnumerable<SearchTagDto> GetDto(IEnumerable<SearchTag> searchTags) 
            => searchTags.Select(x => new SearchTagDto(x.Tag.Id, x.Value, x.SearchType));
    }
}