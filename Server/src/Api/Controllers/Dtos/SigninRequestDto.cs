using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Constants;

namespace API.Controllers.Dtos;

public record SigninRequestDto(
    [property: JsonPropertyName("login")]
    [StringLength(ValidationConstants.MaxLoginLength, MinimumLength = ValidationConstants.MinLoginLength)]
    string Login,
    [property: JsonPropertyName("password")]
    [StringLength(ValidationConstants.MaxPasswordLength, MinimumLength = ValidationConstants.MinPasswordLength)]
    string Password
);