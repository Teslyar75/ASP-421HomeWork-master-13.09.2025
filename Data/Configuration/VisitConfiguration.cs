using ASP_421.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ASP_421.Data.Configuration
{
    public class VisitConfiguration : IEntityTypeConfiguration<Visit>
    {
        public void Configure(EntityTypeBuilder<Visit> builder)
        {
            builder.HasKey(v => v.Id);
            
            builder.Property(v => v.VisitTime)
                .IsRequired()
                .HasColumnType("datetime2");
            
            builder.Property(v => v.RequestPath)
                .IsRequired()
                .HasMaxLength(500);
            
            builder.Property(v => v.UserLogin)
                .HasMaxLength(100);
            
            builder.Property(v => v.ConfirmationCode)
                .IsRequired()
                .HasMaxLength(10);
            
            builder.Property(v => v.IsConfirmed)
                .IsRequired()
                .HasDefaultValue(false);
            
            builder.Property(v => v.ConfirmedAt)
                .HasColumnType("datetime2");
            
            builder.Property(v => v.UserAgent)
                .HasMaxLength(1000);
            
            builder.Property(v => v.IpAddress)
                .HasMaxLength(45); // IPv6 max length
            
            // Индексы для быстрого поиска
            builder.HasIndex(v => v.VisitTime);
            builder.HasIndex(v => v.RequestPath);
            builder.HasIndex(v => v.UserLogin);
            builder.HasIndex(v => v.ConfirmationCode);
            builder.HasIndex(v => v.IsConfirmed);
        }
    }
}
