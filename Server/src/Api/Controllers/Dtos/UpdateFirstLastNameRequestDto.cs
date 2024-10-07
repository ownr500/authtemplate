using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Constants;

namespace API.Controllers.Dtos;

public record UpdateFirstLastNameRequestDto(
    [StringLength(ValidationConstants.MaxLengthName, MinimumLength = ValidationConstants.MinLengthName)]
    [property: JsonPropertyName("firstName")]
    string FirstName,
    [StringLength(ValidationConstants.MaxLengthName, MinimumLength = ValidationConstants.MinLengthName)]
    [property: JsonPropertyName("lastName")]
    string LastName
);