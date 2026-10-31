using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace LMS.Infrastructure.Data.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);
            builder.Property(m => m.Description)
            .HasMaxLength(2000);
            builder.HasOne(m => m.Course)
            .WithMany(c => c.Modules)
            .HasForeignKey(m => m.CourseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(m => m.Activities)
            .WithOne(a => a.Module)
            .HasForeignKey(a => a.ModuleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
            builder.ToTable(t => t.HasCheckConstraint(
            "CK_Module_Dates",
            "[StartDate] <= [EndDate]"));
        }
    }
}
