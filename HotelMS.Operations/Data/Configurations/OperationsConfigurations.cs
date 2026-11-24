using HotelMS.Operations.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelMS.Operations.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.DepartmentId);

        builder.Property(d => d.DepartmentName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(d => d.DepartmentName).IsUnique();

        builder.Property(d => d.Description)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(d => d.Staff)
            .WithOne(s => s.Department)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff");
        builder.HasKey(s => s.StaffId);

        builder.Property(s => s.StaffCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.StaffCode).IsUnique();

        builder.Property(s => s.DepartmentId)
            .IsRequired();

        builder.Property(s => s.EmployeeName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Address)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.NIC)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.NIC).IsUnique();

        builder.Property(s => s.Salary)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(s => s.Age)
            .IsRequired();

        builder.Property(s => s.Occupation)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(100);
    }
}
