using System;
using System.Threading.Tasks;

namespace ImoutoRebirth.Common.Domain;

public interface ITransaction : IDisposable
{
    Task CommitAsync();
}