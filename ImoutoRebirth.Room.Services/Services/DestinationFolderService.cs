﻿using System;
using System.Collections.Generic;
using System.IO;
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
            var newPath = GetNewPath(destinationFolder, moveInformation);

            _logger.LogInformation("Destination path is calculated: {NewPath}.", newPath);

            var newFile = new FileInfo(newPath);
            var wasANewFile = MoveFile(moveInformation.SystemFile, ref newFile);
            _logger.LogInformation("Saved to: {NewPath}.", newFile.FullName);

            return new MovedInformation(
                moveInformation, 
                wasANewFile && moveInformation.MoveProblem == MoveProblem.None,
                newFile);
        }

        /// <summary>
        /// Move file from current to desired location.
        /// </summary>
        /// <param name="oldFile">Current file location.</param>
        /// <param name="newFile">Desired file location.</param>
        /// <returns>Should file be registered.</returns>
        private bool MoveFile(SystemFile oldFile, ref FileInfo newFile)
        {
            if (string.Equals(oldFile.File.FullName, newFile.FullName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("File doesn't require moving.");
                return true;
            }

            if (!oldFile.File.Exists)
                throw new ArgumentException(nameof(oldFile));

            if (!newFile.Directory.Exists)
            {
                _logger.LogInformation("Creating target directory {NewDirectory}", newFile.Directory);
                newFile.Directory.Create();
            }

            if (newFile.Exists)
                return MoveToExisted(oldFile, ref newFile);

            _logger.LogInformation("Moving to {NewPath}.", newFile.FullName);
            oldFile.File.MoveTo(newFile.FullName);

            return true;
        }

        private bool MoveToExisted(SystemFile oldFile, ref FileInfo newFile)
        {
            _logger.LogWarning("Destination file already exists.");

            if (oldFile.Md5 == _fileService.GetMd5Checksum(newFile))
            {
                _logger.LogInformation("Files are identical. Removing file from source folder.");

                oldFile.File.Delete();

                return false;
            }

            _logger.LogInformation("Files are different. Generating new name.");

            int counter = 0;
            FileInfo countedFile;

            var filenameWithoutExtension =
                newFile.Name.Substring(0, newFile.Name.Length - newFile.Extension.Length - 1);

            do
            {
                var countedPath
                    = $@"{newFile.Directory}\{filenameWithoutExtension} ({counter}).{newFile.Extension}";
                countedFile = new FileInfo(countedPath);
                counter++;
            }
            while (countedFile.Exists);

            _logger.LogInformation("Moving to {ChangeNewPath}.", countedFile.FullName);
            oldFile.File.MoveTo(countedFile.FullName);

            newFile = countedFile;

            return true;
        }

        private string GetNewPath(
            DestinationFolder destinationFolder,
            MoveInformation moveInformation)
        {
            var newPathParts = new List<string>();

            AddDestinationFolder(destinationFolder, moveInformation, newPathParts);

            var problemSubfolder = GetProblemSubfolder(destinationFolder,
                moveInformation.MoveProblem);

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
            List<string> newPathParts)
        {
            var destDirectory = destinationFolder.GetDestinationDirectory();

            if (destDirectory == null)
            {
                newPathParts.Add(moveInformation.SystemFile.File.DirectoryName);
            }
            else
            {
                newPathParts.Add(destDirectory.FullName);
            }
        }

        private string GetProblemSubfolder(DestinationFolder destinationDirectory,
            MoveProblem moveProblem)
        {
            switch (moveProblem)
            {
                case MoveProblem.None:
                    return null;
                case MoveProblem.InvalidFormat:
                    return destinationDirectory.FormatErrorSubfolder;
                case MoveProblem.WithoutHash:
                    return destinationDirectory.WithoutHashErrorSubfolder;
                case MoveProblem.IncorrectHash:
                    return destinationDirectory.HashErrorSubfolder;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moveProblem), moveProblem, null);
            }
        }
    }
}