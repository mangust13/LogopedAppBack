using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Contracts;

public static class ValidationExtensions
{
    public static ValidationProblemDetails ToValidationProblemDetails(this ValidationResult result) =>
        new(result.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()));
}