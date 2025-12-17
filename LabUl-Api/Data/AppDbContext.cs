using Microsoft.EntityFrameworkCore;
using LabUI.Models;

namespace LabUlApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<Dish>()
                .HasOne(d => d.Category)         
                .WithMany()                        
                .HasForeignKey(d => d.CategoryId)  
                .OnDelete(DeleteBehavior.Restrict);

           
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.NormalizedName)
                .IsUnique();
        }
    }
}