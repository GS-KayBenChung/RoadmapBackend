using Microsoft.EntityFrameworkCore;
using Domain;

namespace Persistence
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) { }

        public DbSet<UserRoadmap> UserRoadmap { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Roadmap> Roadmaps { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<ToDoTask> ToDoTasks { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRoadmap>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<AuditLog>()
                .HasKey(l => l.LogId);

            modelBuilder.Entity<AuditLog>()
                .HasOne(l => l.User) 
                .WithMany(u => u.Logs) 
                .HasForeignKey(l => l.UserId);  

            modelBuilder.Entity<Roadmap>()
                .HasKey(r => r.RoadmapId);

            modelBuilder.Entity<Roadmap>()
                .HasOne(r => r.CreatedByUser) 
                .WithMany(u => u.Roadmaps)  
                .HasForeignKey(r => r.CreatedBy); 

            modelBuilder.Entity<Milestone>()
                .HasKey(m => m.MilestoneId);

            modelBuilder.Entity<Milestone>()
                .HasOne(m => m.Roadmap) 
                .WithMany(r => r.Milestones) 
                .HasForeignKey(m => m.RoadmapId); 

            modelBuilder.Entity<Section>()
                .HasKey(s => s.SectionId);

            modelBuilder.Entity<Section>()
                .HasOne(s => s.Milestone) 
                .WithMany(m => m.Sections) 
                .HasForeignKey(s => s.MilestoneId); 

            modelBuilder.Entity<ToDoTask>()
                .HasKey(t => t.TaskId);

            modelBuilder.Entity<ToDoTask>()
                .HasOne(t => t.Section) 
                .WithMany(s => s.ToDoTasks) 
                .HasForeignKey(t => t.SectionId); 
        }
    }
}
