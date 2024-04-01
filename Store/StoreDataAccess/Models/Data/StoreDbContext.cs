using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataAccess.Models.Data
{
	public class StoreDbContext: DbContext
	{
		public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
		{
		}
        public StoreDbContext()
        {
            
        }
        public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Category>()
				.HasMany(c => c.Products)
				.WithOne(p => p.Category)
				.HasForeignKey(p => p.CategoryId);

			modelBuilder.Entity<Product>()
				.Property<decimal>("Price").HasColumnType("decimal(18,2)");

			modelBuilder.Entity<Category>().HasData(
								new Category { Id = 1, Name = "Electronics" },
												new Category { Id = 2, Name = "Clothing" },
																new Category { Id = 3, Name = "Books" }
																			);

			modelBuilder.Entity<Product>().HasData(
								new Product { Id = 1, Name = "Laptop", Price = 1000, CategoryId = 1, Description = "A laptop"},
												new Product { Id = 2, Name = "T-shirt", Price = 20, CategoryId = 2, Description = "A t-shirt"},
																new Product { Id = 3, Name = "Book", Price = 10, CategoryId = 3, Description = "A book"}
																			);
		}
	}
}
