using System;

namespace ImoutoRebirth.Meido.MessageContracts;

public interface INewFileCommand
{
    string Md5 { get; }

    Guid FileId { get; }
}