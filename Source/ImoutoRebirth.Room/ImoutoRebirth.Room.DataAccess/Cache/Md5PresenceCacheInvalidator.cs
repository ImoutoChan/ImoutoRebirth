using System.Data.Common;
using System.Runtime.CompilerServices;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace ImoutoRebirth.Room.DataAccess.Cache;

internal class Md5PresenceCacheInvalidator(IMd5PresenceCache cache) : DbTransactionInterceptor
{
    private readonly ConditionalWeakTable<DbTransaction, List<Action>> _pending = new();

    public void FileChanged(RoomDbContext db, string md5)
        => InvalidateNowAndAfterCommit(db, () => cache.Remove(md5));

    public void CollectionDeleted(RoomDbContext db) => InvalidateNowAndAfterCommit(db, cache.Clear);

    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        if (!_pending.TryGetValue(transaction, out var invalidations))
            return;

        _pending.Remove(transaction);

        foreach (var invalidate in invalidations)
            invalidate();
    }

    public override Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken ct = default)
    {
        TransactionCommitted(transaction, eventData);
        return Task.CompletedTask;
    }

    private void InvalidateNowAndAfterCommit(RoomDbContext db, Action invalidate)
    {
        invalidate();

        if (db.Database.CurrentTransaction is { } transaction)
            _pending.GetOrCreateValue(transaction.GetDbTransaction()).Add(invalidate);
    }
}
