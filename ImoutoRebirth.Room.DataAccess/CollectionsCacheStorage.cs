using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess
{
    public class CollectionsCacheStorage : ICollectionsCacheStorage
    {
        private readonly ReaderWriterLockSlim _readerWriterLockSlim 
            = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public List<OverseedColleciton> Collections { get; private set; }

        public bool Filled { get; private set; }

        public void Fill(IEnumerable<OverseedColleciton> items)
        {
            _readerWriterLockSlim.EnterWriteLock();

            try
            {
                Collections = items.ToList();
                Filled = true;
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public async Task Update(Func<List<OverseedColleciton>, Task> updateFunc)
        {
            _readerWriterLockSlim.EnterWriteLock();

            try
            {
                await updateFunc(Collections);
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        public void Update(Action<List<OverseedColleciton>> updateFunc)
        {
            _readerWriterLockSlim.EnterWriteLock();

            try
            {
                updateFunc(Collections);
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();;
            }
        }
    }
}