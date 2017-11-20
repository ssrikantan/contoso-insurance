using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ContosoinsExtPortal.Models
{
    public partial class ContosoinsauthdbContext : DbContext
    {
        public virtual DbSet<VehiclePolicies> VehPoliciesMaster { get; set; }

        public ContosoinsauthdbContext(DbContextOptions<ContosoinsauthdbContext> options) : base(options)
        { }

        //        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //        {
        //            if (!optionsBuilder.IsConfigured)
        //            {
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        //                optionsBuilder.UseSqlServer(@"Server=contosoinsdb.database.windows.net;Database=Contosoinsauthdb;User Id=onepageradmin; Password=Pass@word123");
        //            }
        //        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VehiclePolicies>(entity =>
            {
                entity.HasIndex(e => e.Policyno)
                    .HasName("PolicyUniqueId")
                    .IsUnique();

                entity.HasIndex(e => e.Vehicleno)
                    .HasName("VehiclenoUNiqueId")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.Firstname).HasMaxLength(10);

                entity.Property(e => e.Inscompany).HasMaxLength(10);

                entity.Property(e => e.Lastmod)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Lastname).HasMaxLength(10);

                entity.Property(e => e.Policyno).HasMaxLength(10);

                entity.Property(e => e.Status).HasMaxLength(10);

                entity.Property(e => e.Uidname).HasMaxLength(30);

                entity.Property(e => e.Userid).HasMaxLength(20);

                entity.Property(e => e.Vehicleno).HasMaxLength(15);

                entity.Property(e => e.Version).HasMaxLength(50);
                entity.Property(e => e.Startdate)
                   .HasColumnType("datetime");
                entity.Property(e => e.Enddate)
                  .HasColumnType("datetime");
            });
        }
    }
}
