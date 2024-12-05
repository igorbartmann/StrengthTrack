using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrengthTrack.Api.Entities;

namespace StrengthTrack.Api.Persistence.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("SESSIONS");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("ID")
                .UseIdentityColumn(1, 1)
                .IsRequired();

            builder.Property(p => p.ClientId)
                .HasColumnName("CLIENTID")
                .IsRequired();

            builder.Property(p => p.Date)
                .HasColumnName("DATE")
                .IsRequired();

            builder.HasMany(p => p.Results)
                .WithOne(s => s.Session)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}