using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess
{
    public interface ICollectionsCacheStorage
    {
        List<OverseedColleciton> Collections { get; }

        bool Filled { get; }

        void Fill(IEnumerable<OverseedColleciton> items);

        Task Update(Func<List<OverseedColleciton>, Task> updateFunc);

        void Update(Action<List<OverseedColleciton>> updateFunc);
    }
}