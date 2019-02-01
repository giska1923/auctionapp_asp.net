namespace Aukcija.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=DefaultConnection")
        {
        }

        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Auction> Auction { get; set; }
        public virtual DbSet<Bid> Bid { get; set; }
        public virtual DbSet<Image> Image { get; set; }
        public virtual DbSet<Token> Token { get; set; }
        public virtual DbSet<TokenOrder> TokenOrder { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.Auctions)
                .WithOptional(e => e.AspNetUsers)
                .HasForeignKey(e => e.IdUser);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.Bids)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.IdUser)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.TokenOrders)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.IdUser)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Auction>()
                .Property(e => e.StartingP)
                .HasPrecision(18, 5);

            modelBuilder.Entity<Auction>()
                .Property(e => e.CurrentP)
                .HasPrecision(18, 5);

            modelBuilder.Entity<Auction>()
                .Property(e => e.Status)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Auction>()
                .HasMany(e => e.Bids)
                .WithRequired(e => e.Auction)
                .HasForeignKey(e => e.GUIDAuction)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Image>()
                .HasMany(e => e.Auctions)
                .WithOptional(e => e.Image)
                .HasForeignKey(e => e.IdImage);

            modelBuilder.Entity<Token>()
                .Property(e => e.RSD)
                .HasPrecision(18, 5);

            modelBuilder.Entity<Token>()
                .Property(e => e.USD)
                .HasPrecision(18, 5);

            modelBuilder.Entity<Token>()
                .Property(e => e.EUR)
                .HasPrecision(18, 5);

            modelBuilder.Entity<TokenOrder>()
                .Property(e => e.PackageP)
                .HasPrecision(18, 5);

            modelBuilder.Entity<TokenOrder>()
                .Property(e => e.Status)
                .IsFixedLength()
                .IsUnicode(false);
        }
    }
}
