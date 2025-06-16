using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ParkRoutePlanner.Models;

public partial class ParkDataContext : DbContext
{
    public ParkDataContext()
    {
    }

    public ParkDataContext(DbContextOptions<ParkDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ride> Rides { get; set; }

    public virtual DbSet<RideCategory> RideCategories { get; set; }

    public virtual DbSet<RideDistance> RideDistances { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    public virtual DbSet<VisitStation> VisitStations { get; set; }

    public virtual DbSet<Visitor> Visitors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP\\SQLEXPRESS;Database=NewDataPark;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ride>(entity =>
        {
            entity.HasKey(e => e.RideId).HasName("PK__rides__C7E4D077222EC5DC");

            entity.ToTable("rides");

            entity.Property(e => e.RideId).HasColumnName("ride_id");
            entity.Property(e => e.AvgDurationMinutes).HasColumnName("avg_duration_minutes");
            entity.Property(e => e.AvgWaitTimeMinutes).HasColumnName("avg_wait_time_minutes");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("location");
            entity.Property(e => e.MaxAge).HasColumnName("max_age");
            entity.Property(e => e.MinAge).HasColumnName("min_age");
            entity.Property(e => e.MinHeightCm).HasColumnName("min_height_cm");
            entity.Property(e => e.OperatingDays)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("operating_days");
            entity.Property(e => e.PopularityRating).HasColumnName("popularity_rating");
            entity.Property(e => e.RideName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ride_name");

            entity.HasMany(d => d.Categories).WithMany(p => p.Rides)
                .UsingEntity<Dictionary<string, object>>(
                    "RideCategoriesPerRide",
                    r => r.HasOne<RideCategory>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ride_cate__categ__5441852A"),
                    l => l.HasOne<Ride>().WithMany()
                        .HasForeignKey("RideId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ride_cate__ride___534D60F1"),
                    j =>
                    {
                        j.HasKey("RideId", "CategoryId").HasName("PK__ride_cat__9AB03EECDAEB91E3");
                        j.ToTable("ride_categories_per_ride");
                        j.IndexerProperty<int>("RideId").HasColumnName("ride_id");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                    });
        });

        modelBuilder.Entity<RideCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__ride_cat__D54EE9B4525DD8BC");

            entity.ToTable("ride_categories");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryDescription)
                .HasMaxLength(255)
                .HasColumnName("category_description");
        });

        modelBuilder.Entity<RideDistance>(entity =>
        {
            entity.HasKey(e => new { e.FromRideId, e.ToRideId }).HasName("PK__ride_dis__49098B2DBB7ABBA0");

            entity.ToTable("ride_distances");

            entity.Property(e => e.FromRideId).HasColumnName("from_ride_id");
            entity.Property(e => e.ToRideId).HasColumnName("to_ride_id");
            entity.Property(e => e.DistanceMeters).HasColumnName("distance_meters");

            entity.HasOne(d => d.FromRide).WithMany(p => p.RideDistanceFromRides)
                .HasForeignKey(d => d.FromRideId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ride_dist__from___3A81B327");

            entity.HasOne(d => d.ToRide).WithMany(p => p.RideDistanceToRides)
                .HasForeignKey(d => d.ToRideId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ride_dist__to_ri__3B75D760");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__staff__1963DD9C3D3DF390");

            entity.ToTable("staff");

            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.StaffName)
                .HasMaxLength(100)
                .HasColumnName("staff_name");
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.VisitId).HasName("PK__visits__375A75E17AD6B035");

            entity.ToTable("visits");

            entity.Property(e => e.VisitId).HasColumnName("visit_id");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.MaxAge).HasColumnName("max_age");
            entity.Property(e => e.MinAge).HasColumnName("min_age");
            entity.Property(e => e.MinHeightCm).HasColumnName("min_height_cm");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.VisitDate).HasColumnName("visit_date");
            entity.Property(e => e.VisitorId).HasColumnName("visitor_id");

            entity.HasOne(d => d.Visitor).WithMany(p => p.Visits)
                .HasForeignKey(d => d.VisitorId)
                .HasConstraintName("FK__visits__visitor___5070F446");

            entity.HasMany(d => d.Categories).WithMany(p => p.Visits)
                .UsingEntity<Dictionary<string, object>>(
                    "RideCategoriesPerVisit",
                    r => r.HasOne<RideCategory>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ride_cate__categ__5812160E"),
                    l => l.HasOne<Visit>().WithMany()
                        .HasForeignKey("VisitId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ride_cate__visit__571DF1D5"),
                    j =>
                    {
                        j.HasKey("VisitId", "CategoryId").HasName("PK__ride_cat__6A0E9B7A81A818FC");
                        j.ToTable("ride_categories_per_visit");
                        j.IndexerProperty<int>("VisitId").HasColumnName("visit_id");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("category_id");
                    });

            entity.HasMany(d => d.Rides).WithMany(p => p.Visits)
                .UsingEntity<Dictionary<string, object>>(
                    "RidesPerVisit",
                    r => r.HasOne<Ride>().WithMany()
                        .HasForeignKey("RideId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__rides_per__ride___5CD6CB2B"),
                    l => l.HasOne<Visit>().WithMany()
                        .HasForeignKey("VisitId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__rides_per__visit__5BE2A6F2"),
                    j =>
                    {
                        j.HasKey("VisitId", "RideId").HasName("PK__rides_pe__5B2438E6185D3076");
                        j.ToTable("rides_per_visit");
                        j.IndexerProperty<int>("VisitId").HasColumnName("visit_id");
                        j.IndexerProperty<int>("RideId").HasColumnName("ride_id");
                    });
        });

        modelBuilder.Entity<VisitStation>(entity =>
        {
            entity.HasKey(e => new { e.VisitId, e.RideId }).HasName("PK__visit_st__5B2438E6F783AF8C");

            entity.ToTable("visit_stations");

            entity.Property(e => e.VisitId).HasColumnName("visit_id");
            entity.Property(e => e.RideId).HasColumnName("ride_id");
            entity.Property(e => e.EstimatedTime).HasColumnName("estimated_time");
            entity.Property(e => e.StationNumber).HasColumnName("station_number");
            entity.Property(e => e.Visited)
                .HasDefaultValue(false)
                .HasColumnName("visited");

            entity.HasOne(d => d.Ride).WithMany(p => p.VisitStations)
                .HasForeignKey(d => d.RideId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__visit_sta__ride___619B8048");

            entity.HasOne(d => d.Visit).WithMany(p => p.VisitStations)
                .HasForeignKey(d => d.VisitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__visit_sta__visit__60A75C0F");
        });

        modelBuilder.Entity<Visitor>(entity =>
        {
            entity.HasKey(e => e.VisitorId).HasName("PK__visitors__87ED1B513B341472");

            entity.ToTable("visitors");

            entity.HasIndex(e => e.Email, "UQ__visitors__AB6E6164D0810417").IsUnique();

            entity.Property(e => e.VisitorId).HasColumnName("visitor_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.VisitorName)
                .HasMaxLength(100)
                .HasColumnName("visitor_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
