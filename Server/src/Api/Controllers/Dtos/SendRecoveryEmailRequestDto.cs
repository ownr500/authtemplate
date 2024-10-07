using System.ComponentModel.DataAnnotations;
using API.Constants;

namespace API.Controllers.Dtos;

public record SendRecoveryEmailRequestDto(
    [EmailAddress]
    [MaxLength(ValidationConstants.MaxEmailLength)]
    string Email);