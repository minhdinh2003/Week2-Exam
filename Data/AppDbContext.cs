using Microsoft.EntityFrameworkCore;
using QuanLySach.Models;
using QuanLySach.MongoModels;
namespace QuanLySach.Data
{

    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Reviewer> Reviewers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Review>();

            base.OnModelCreating(modelBuilder);
        }


    }


}
