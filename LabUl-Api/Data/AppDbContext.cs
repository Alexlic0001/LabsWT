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

            // Простая настройка отношения
            modelBuilder.Entity<Dish>()
                .HasOne(d => d.Category)           // У блюда есть одна категория
                .WithMany()                        // У категории много блюд (без навигации)
                .HasForeignKey(d => d.CategoryId)  // Внешний ключ
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка индекса для уникальности NormalizedName
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.NormalizedName)
                .IsUnique();
        }
    }
}