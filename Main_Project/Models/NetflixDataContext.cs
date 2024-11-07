using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Netflix.Models;

namespace Main_Project.Models;

public partial class NetflixDataContext : DbContext
{
    public NetflixDataContext()
    {
    }

    public NetflixDataContext(DbContextOptions<NetflixDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MoviesTable> MoviesTables { get; set; }

    public virtual DbSet<UserDatum> UserData { get; set; }
    public virtual DbSet<Question> Question { get; set; }

    public virtual DbSet<WatchList> WatchLists { get; set; }


    public object Movie_category_table { get; internal set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=NetflixDatabase");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {


        modelBuilder.Entity<MoviesTable>(entity =>
        {
            entity.HasKey(e => e.Movieid);

            entity.ToTable("Movies_table");

            entity.Property(e => e.Movieid).HasColumnName("movieid");
            entity.Property(e => e.Discription)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("discription");
            entity.Property(e => e.Movie)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("movie");
            entity.Property(e => e.Moviecast)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("moviecast");
            entity.Property(e => e.Moviename)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("moviename");
            entity.Property(e => e.Movieposter)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("movieposter");
            entity.Property(e => e.Movieposter2)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("movieposter2");
            entity.Property(e => e.Moviereleaseyear)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("moviereleaseyear");
            entity.Property(e => e.Movietype)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("movietype");
            entity.Property(e => e.Moviedirector)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("moviedirector");
            entity.Property(e => e.Movierating)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("movierating");
            entity.Property(e => e.Movielanguage)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("movielanguage");
            entity.Property(e => e.Movieagerestrictions)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("movieagerestrictions");
            entity.Property(e => e.Category)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("category"); 
            entity.Property(e => e.Content_advisory)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("content_advisory");
            entity.Property(e => e.Trailer)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("trailer");
            
            
        });

        modelBuilder.Entity<UserDatum>(entity =>
        {
            entity.ToTable("User_data");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cardholdername)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cardholdername");
            entity.Property(e => e.Cardnumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cardnumber");
            entity.Property(e => e.Cvv)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cvv");
            entity.Property(e => e.Datetime)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("datetime");
            entity.Property(e => e.Duration)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("duration");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Price)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("price");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<WatchList>(entity =>
        {
            entity.ToTable("Watch_list");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Movieid).HasColumnName("movieid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Movie).WithMany(p => p.WatchLists)
                .HasForeignKey(d => d.Movieid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Watch_list_Movies_table");

            entity.HasOne(d => d.User).WithMany(p => p.WatchLists)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Watch_list_User_data");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");


            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Question1)
              .HasMaxLength(50)
              .IsUnicode(false)
              .HasColumnName("question");
            entity.Property(e => e.Answer)
              .HasMaxLength(50)
              .IsUnicode(false)
              .HasColumnName("answer");

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
