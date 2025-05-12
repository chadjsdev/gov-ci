using EcfrApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcfrApp.Data
{
    public class EcfrContext : DbContext
    {
        public EcfrContext(DbContextOptions<EcfrContext> options) : base(options) { }

        public DbSet<Agency> Agencies { get; set; }
        public DbSet<CfrReference> CfrReferences { get; set; }
        public DbSet<Correction> Corrections { get; set; }
        public DbSet<CorrectionReference> CorrectionReferences { get; set; }
        public DbSet<TitleStructure> TitleStructures { get; set; }
        public DbSet<StructureNode> StructureNodes { get; set; }
        public DbSet<CorrectionCount> CorrectionCounts { get; set; } = null!;
        public DbSet<DescendantRange> DescendantRanges { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Agency>()
                .HasMany(a => a.Children)
                .WithOne()
                .HasForeignKey("ParentSlug")
                .IsRequired(false);

            modelBuilder.Entity<CfrReference>()
                .HasOne(cr => cr.Agency)
                .WithMany(a => a.CfrReferences)
                .HasForeignKey(cr => cr.AgencySlug);

            modelBuilder.Entity<CorrectionReference>()
                .HasOne(cr => cr.Correction)
                .WithMany(c => c.CfrReferences)
                .HasForeignKey(cr => cr.CorrectionId);

            modelBuilder.Entity<TitleStructure>()
                .HasMany(ts => ts.Children)
                .WithOne(sn => sn.TitleStructure)
                .HasForeignKey(sn => sn.TitleStructureId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}