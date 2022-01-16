using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess.Models;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Core.Services
{

    public class DestinationFolderService : IDestinationFolderService
    {
        private readonly ILogger<DestinationFolderService> _logger;
        private readonly IFileService _fileService;

        public DestinationFolderService(
            ILogger<DestinationFolderService> logger,
            IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        public MovedInformation Move(
            DestinationFolder destinationFolder,
            MoveInformation moveInformation)
        {
            if (moveInformation.MoveProblem == MoveProblem.AlreadyContains)
            {
                var shouldDelete = destinationFolder.GetDestinationDirectory() != null;
                if (shouldDelete)
                    moveInformation.SystemFile.File.Delete();

                _logger.LogWarning(
                    "File with this md5 already contains in database. Md5: {Md5} File: {NewFile}. File deleted {FileDeleted}.",
                    moveInformation.SystemFile.Md5,
                    moveInformation.SystemFile.File.FullName,
                    shouldDelete);

                return new MovedInformation(
                    moveInformation,
                    false,
                    moveInformation.SystemFile.File);
            }

            var newPath = GetNewPath(destinationFolder, moveInformation);

            _logger.LogInformation("Destination path is calculated: {NewPath}.", newPath);

            var newFile = new FileInfo(newPath);
            var wasANewFile = _fileService.MoveFile(moveInformation.SystemFile, ref newFile);
            _logger.LogInformation("Saved to: {NewPath}.", newFile.FullName);

            return new MovedInformation(
                moveInformation,
                wasANewFile && moveInformation.MoveProblem == MoveProblem.None,
                newFile);
        }

        private static string GetNewPath(
            DestinationFolder destinationFolder,
            MoveInformation moveInformation)
        {
            var newPathParts = new List<string>();

            AddDestinationFolder(destinationFolder, moveInformation, newPathParts);

            var problemSubfolder = GetProblemSubfolder(destinationFolder, moveInformation.MoveProblem);

            var renamed = false;

            if (problemSubfolder != null)
            {
                newPathParts.Add(problemSubfolder);
            }
            else
            {
                if (destinationFolder.ShouldCreateSubfoldersByHash)
                {
                    AddMd5Path(moveInformation, newPathParts);
                }

                if (destinationFolder.ShouldRenameByHash)
                {
                    RenameToMd5(moveInformation, newPathParts);
                    renamed = true;
                }
            }

            if (!renamed)
                newPathParts.Add(moveInformation.SystemFile.File.Name);

            return Path.Combine(newPathParts.ToArray());
        }

        private static void RenameToMd5(MoveInformation moveInformation, List<string> newPathParts)
        {
            newPathParts.Add($"{moveInformation.SystemFile.Md5}{moveInformation.SystemFile.File.Extension}");
        }

        private static void AddMd5Path(MoveInformation moveInformation, List<string> newPathParts)
        {
            var md5Sub = moveInformation.SystemFile.Md5.Substring(0, 2);
            var md5SubSub = moveInformation.SystemFile.Md5.Substring(2, 2);

            newPathParts.Add(md5Sub);
            newPathParts.Add(md5SubSub);
        }

        private static void AddDestinationFolder(
            DestinationFolder destinationFolder,
            MoveInformation moveInformation,
            ICollection<string> newPathParts)
        {
            var destDirectory = destinationFolder.GetDestinationDirectory();
            var fileDirectory = moveInformation.SystemFile.File.DirectoryName;

            if (destDirectory == null)
            {
                if (fileDirectory != null)
                    newPathParts.Add(fileDirectory);
            }
            else
            {
                newPathParts.Add(destDirectory.FullName);
            }
        }

        private static string? GetProblemSubfolder(DestinationFolder destinationDirectory,
            MoveProblem moveProblem)
        {
            return moveProblem switch
            {
                MoveProblem.None => null,
                MoveProblem.InvalidFormat => destinationDirectory.FormatErrorSubfolder,
                MoveProblem.WithoutHash => destinationDirectory.WithoutHashErrorSubfolder,
                MoveProblem.IncorrectHash => destinationDirectory.HashErrorSubfolder,
                _ => throw new ArgumentOutOfRangeException(nameof(moveProblem), moveProblem, null)
            };
        }
    }
}
