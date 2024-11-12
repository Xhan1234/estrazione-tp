using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;

namespace MyWebApp
{
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Change DbSet name to Tasks to match the controller
        public DbSet<TaskData> TaskDatas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskData>()
            .ToTable("TaskData") // Specify the table name if it differs
            .HasKey(t => t.IdTask);
    }
}

}
