using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Constants;

namespace API.Controllers.Dtos;

public record ResetPasswordRequestDto(
    [StringLength(ValidationConstants.MaxPasswordLength, MinimumLength = ValidationConstants.MinPasswordLength)]
    [property: JsonPropertyName("newPassword")]
    string NewPassword);