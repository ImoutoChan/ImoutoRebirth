using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImoutoRebirth.Meido.DataAccess.EntityConfigurations
{
    internal class ParsingStatusConfiguration : IEntityTypeConfiguration<ParsingStatus>
    {
        public void Configure(EntityTypeBuilder<ParsingStatus> b)
        {
            b.HasKey(x => new { x.FileId, x.Source });
            b.Property(x => x.Md5).IsRequired();
            b.AddShadowTimeTracking();

            b.Property(x => x.FileId);
            b.Property(x => x.Source);
        }
    }
}