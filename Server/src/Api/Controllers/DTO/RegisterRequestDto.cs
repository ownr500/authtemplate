﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using API.Constants;

namespace API.Controllers.DTO;

public record RegisterRequestDto(
    [StringLength(ValidationConstants.MaxLengthName, MinimumLength = ValidationConstants.MinLengthName)]
    [property: JsonPropertyName("firstName")]
    string FirstName,
    [property: JsonPropertyName("lastName")]
    [StringLength(ValidationConstants.MaxLengthName, MinimumLength = ValidationConstants.MinLengthName)]
    string LastName,
    [property: JsonPropertyName("age")]
    [Range(ValidationConstants.MinAge, int.MaxValue)]
    int Age,
    [property: JsonPropertyName("login")]
    [StringLength(ValidationConstants.MaxLoginLength, MinimumLength = ValidationConstants.MinLoginLength)]
    string Login,
    [property: JsonPropertyName("password")]
    [StringLength(ValidationConstants.MaxPasswordLength, MinimumLength = ValidationConstants.MinPasswordLength)]
    string Password
);