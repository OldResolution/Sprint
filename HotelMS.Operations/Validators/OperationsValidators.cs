using FluentValidation;
using HotelMS.Operations.DTOs;

namespace HotelMS.Operations.Validators;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.DepartmentName).NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");
    }
}

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentDto>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.DepartmentName).NotEmpty().WithMessage("Department name is required.")
            .MaximumLength(100).WithMessage("Department name cannot exceed 100 characters.");
    }
}

public class CreateStaffValidator : AbstractValidator<CreateStaffDto>
{
    public CreateStaffValidator()
    {
        RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("Valid DepartmentId is required.");
        RuleFor(x => x.EmployeeName).NotEmpty().WithMessage("Employee name is required.");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");
        RuleFor(x => x.NIC).NotEmpty().WithMessage("NIC is required.");
        RuleFor(x => x.Salary).GreaterThan(0).WithMessage("Salary must be greater than zero.");
        RuleFor(x => x.Age).InclusiveBetween(18, 100).WithMessage("Age must be between 18 and 100.");
        RuleFor(x => x.Occupation).NotEmpty().WithMessage("Occupation is required.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
    }
}

public class UpdateStaffValidator : AbstractValidator<UpdateStaffDto>
{
    public UpdateStaffValidator()
    {
        RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("Valid DepartmentId is required.");
        RuleFor(x => x.EmployeeName).NotEmpty().WithMessage("Employee name is required.");
        RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required.");
        RuleFor(x => x.NIC).NotEmpty().WithMessage("NIC is required.");
        RuleFor(x => x.Salary).GreaterThan(0).WithMessage("Salary must be greater than zero.");
        RuleFor(x => x.Age).InclusiveBetween(18, 100).WithMessage("Age must be between 18 and 100.");
        RuleFor(x => x.Occupation).NotEmpty().WithMessage("Occupation is required.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
    }
}
