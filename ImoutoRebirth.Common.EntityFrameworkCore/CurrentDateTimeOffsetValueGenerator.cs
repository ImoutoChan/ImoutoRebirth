using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ImoutoRebirth.Common.EntityFrameworkCore
{
    public class CurrentDateTimeOffsetValueGenerator : ValueGenerator<DateTimeOffset>
    {
        public override DateTimeOffset Next(EntityEntry entry) => DateTimeOffset.Now;

        public override bool GeneratesTemporaryValues => false;
    }
}