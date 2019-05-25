using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Meido.Core;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImoutoRebirth.Meido.DataAccess.Entities
{
    internal class SourceActualizingStateConfiguration : IEntityTypeConfiguration<SourceActualizingState>
    {
        public void Configure(EntityTypeBuilder<SourceActualizingState> b)
        {
            b.HasKey(x => x.Source);
            b.AddShadowTimeTracking();
        }
    }
}