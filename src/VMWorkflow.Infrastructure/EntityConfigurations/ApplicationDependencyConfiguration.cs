using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VMWorkflow.Domain.Entities;

namespace VMWorkflow.Infrastructure.EntityConfigurations;

public class ApplicationDependencyConfiguration : IEntityTypeConfiguration<ApplicationDependency>
{
    public void Configure(EntityTypeBuilder<ApplicationDependency> builder)
    {
        builder.HasKey(d => d.ApplicationDependencyId);


        builder.Property(d => d.DependencyName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Protocol).IsRequired().HasMaxLength(20);
    }
}
