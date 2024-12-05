using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StrengthTrack.Api.Entities;

namespace StrengthTrack.Api.Persistence.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("CLIENTS");
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.Cpf);

            builder.Property(p => p.Id)
                .HasColumnName("ID")
                .UseIdentityColumn(1, 1)
                .IsRequired();

            builder.Property(p => p.Name)
                .HasColumnName("NAME")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Cpf)
                .HasColumnName("CPF")
                .HasMaxLength(11)
                .IsRequired();

            builder.HasMany(p => p.Sessions)
                .WithOne(s => s.Client)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}