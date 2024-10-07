using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Core.Entities;

[Index(nameof(AccessToken), IsUnique = true)]
[Index(nameof(RefreshToken), IsUnique = true)]
public class TokenEntity
{
    [Key] public Guid Id { get; set; }
    
    public Guid UserId { get; set; }

    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string AccessToken { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.MaxTokenLength)]
    public string RefreshToken { get; set; } = string.Empty;
    
    public bool RefreshTokenActive { get; set; }
    
    public DateTimeOffset AccessTokenExpireAt { get; set; }
    
    public DateTimeOffset RefreshTokenExpireAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public virtual UserEntity User { get; set; } = default!;
}