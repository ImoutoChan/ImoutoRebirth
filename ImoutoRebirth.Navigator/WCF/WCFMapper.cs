using System.Linq;
using Imouto.WcfExchangeLibrary.Core.Data;
using ImoutoRebirth.Navigator.ViewModel;

namespace ImoutoRebirth.Navigator.WCF
{
    static class WCFMapper
    {
        public static Folder MapFolder(FolderVM folderVM)
        {
            var result = new Folder();
            result.Id = folderVM.Id;
            result.Path = folderVM.Path;

            if (folderVM is DestinationFolderVM)
            {
                var destFolderVM = folderVM as DestinationFolderVM;
                result.Type = FolderType.Destination;
                result.NeedDevideImagesByHash = destFolderVM.NeedDevideImagesByHash;
                result.NeedRename = destFolderVM.NeedRename;
                result.IncorrectFormatSubpath = destFolderVM.IncorrectFormatSubpath;
                result.IncorrectHashSubpath = destFolderVM.IncorrectHashSubpath;
                result.NonHashSubpath = destFolderVM.NonHashSubpath;
            }
            else if (folderVM is SourceFolderVM)
            {
                var sourceFolderVM = folderVM as SourceFolderVM;
                result.Type = FolderType.Source;
                result.NeedCheckFormat = sourceFolderVM.CheckFormat;
                result.NeedCheckNameHash = sourceFolderVM.CheckNameHash;
                result.Extensions = sourceFolderVM.SupportedExtensionsRaw.ToList();
                result.TagsFromSubfoder = sourceFolderVM.TagsFromSubfolder;
                result.AddTagFromFileName = sourceFolderVM.AddTagFromFileName;
            }
            return result;
        }
    }
}
