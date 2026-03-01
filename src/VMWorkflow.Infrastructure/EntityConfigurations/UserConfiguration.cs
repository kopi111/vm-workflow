using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Role).IsRequired().HasMaxLength(50);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);

        builder.HasIndex(u => u.Username).IsUnique();
    }
}
