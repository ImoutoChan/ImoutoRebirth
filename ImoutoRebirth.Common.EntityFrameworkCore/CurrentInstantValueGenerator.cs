using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NodaTime;

namespace ImoutoRebirth.Common.EntityFrameworkCore;

public class CurrentInstantValueGenerator : ValueGenerator<Instant>
{
    public override Instant Next(EntityEntry entry) => SystemClock.Instance.GetCurrentInstant();

    public override bool GeneratesTemporaryValues => false;
}
