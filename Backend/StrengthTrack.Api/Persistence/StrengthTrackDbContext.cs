using System;
using Microsoft.EntityFrameworkCore;
using StrengthTrack.Api.Entities;
using StrengthTrack.Api.Persistence.Configurations;

namespace StrengthTrack.Api.Persistence
{
    public class StrengthTrackDbContext : DbContext
    {
        public StrengthTrackDbContext(DbContextOptions<StrengthTrackDbContext> options) : base(options) {}

        public DbSet<Client> Clients {get;set;} = null!;
        public DbSet<Session> Sessions {get;set;} = null!;
        public DbSet<Result> Results {get;set;} = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.ApplyConfiguration(new ClientConfiguration());
            modelBuilder.ApplyConfiguration(new SessionConfiguration());
            modelBuilder.ApplyConfiguration(new ResultConfiguration());
        }
    }
}