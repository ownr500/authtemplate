﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Constants;

namespace API.Controllers.DTO;

public record ChangeRequestDto(
    [StringLength(ValidationConstants.MaxLengthName, MinimumLength = ValidationConstants.MinLengthName)]
    [property: JsonPropertyName("firstName")]
    string FirstName,
    [StringLength(ValidationConstants.MaxLengthName, MinimumLength = ValidationConstants.MinLengthName)]
    [property: JsonPropertyName("lastName")]
    string LastName
);