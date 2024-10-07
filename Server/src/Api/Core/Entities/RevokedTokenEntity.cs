using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Core.Entities;

[Index(nameof(Token), IsUnique = true)]
public class RevokedTokenEntity
{
    [Key] public Guid Id { get; set; }
    
    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string Token { get; set; } = string.Empty;

    public DateTimeOffset TokenExpireAt { get; set; }
}