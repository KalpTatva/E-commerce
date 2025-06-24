
using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Repository.CustomValidation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DateDifferenceAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var startDateProp = validationContext.ObjectType.GetProperty("StartDate");
            var endDateProp = validationContext.ObjectType.GetProperty("EndDate");

            if (startDateProp == null || endDateProp == null)
                return new ValidationResult("StartDate or EndDate property not found.");

            var startDateValue = startDateProp.GetValue(validationContext.ObjectInstance);
            var endDateValue = endDateProp.GetValue(validationContext.ObjectInstance);

            if (startDateValue == null || endDateValue == null)
                return ValidationResult.Success; // Let [Required] handle nulls

            var startDate = (DateTime)startDateValue;
            var endDate = (DateTime)endDateValue;

            if (startDate > endDate)
                return new ValidationResult("Start date should not be after end date.");

            return ValidationResult.Success;
        }
    }
}