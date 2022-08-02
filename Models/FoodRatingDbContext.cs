using Microsoft.EntityFrameworkCore;

namespace food_rating_server.Models
{
    public class FoodRatingDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Change> Changes { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-EUBV5QK;Initial Catalog=FoodRating;Integrated Security=True");
        }
    }
}
