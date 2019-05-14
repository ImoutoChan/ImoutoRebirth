using ImoutoProject.Common.Cqrs.Abstract;
using System;

namespace ImoutoRebirth.Meido.Services.Cqrs.Commands
{
    internal class AddNewFileCommand : ICommand
    {
        public AddNewFileCommand(Guid fileId, string md5)
        {
            FileId = fileId;
            Md5 = md5;
        }

        public Guid FileId { get; }

        public string Md5 { get; }
    }
}