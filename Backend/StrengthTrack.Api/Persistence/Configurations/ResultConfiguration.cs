using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrengthTrack.Api.Entities;

namespace StrengthTrack.Api.Persistence.Configurations
{
    public class ResultConfiguration : IEntityTypeConfiguration<Result>
    {
        public void Configure(EntityTypeBuilder<Result> builder)
        {
            builder.ToTable("RESULTS");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("ID")
                .UseIdentityColumn(1, 1)
                .IsRequired();

            builder.Property(p => p.SessionId)
                .HasColumnName("SESSIONID")
                .IsRequired();

            builder.Property(p => p.Value)
                .HasColumnName("VALUE")
                .IsRequired();
        }
    }
}