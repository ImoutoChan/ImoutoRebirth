using ImoutoRebirth.Lilin.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.DataAccess
{
    public class LilinDbContext : DbContext
    {
        public DbSet<TagTypeEntity> TagTypes { get; set; }

        public DbSet<TagEntity> Tags { get; set; }

        public DbSet<NoteEntity> Notes { get; set; }

        public DbSet<FileTagEntity> FileTags { get; set; }

        public LilinDbContext(DbContextOptions<LilinDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileTagEntity>(b =>
            {
                b.HasKey(x => new {x.FileId, x.TagId});
                b.HasOne(x => x.Tag)
                 .WithMany(x => x.FileTags)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<TagEntity>(b =>
            {
                b.HasOne(x => x.Type)
                 .WithMany(x => x.Tags)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
