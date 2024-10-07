using System.ComponentModel.DataAnnotations;
using API.Constants;
using Microsoft.EntityFrameworkCore;

namespace API.Core.Entities;

[Index(nameof(LoginNormalized), IsUnique = true)]
[Index(nameof(EmailNormalized), IsUnique = true)]
public class UserEntity
{
    [Key] public Guid Id { get; set; }

    [MaxLength(ValidationConstants.MaxLengthName)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.MaxLengthName)]
    public string LastName { get; set; } = string.Empty;

    public int Age { get; set; }

    [MaxLength(ValidationConstants.MaxLoginLength)]
    public string Login { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.MaxLoginLength)]
    public string LoginNormalized { get; set; } = string.Empty;

    [MaxLength(ValidationConstants.MaxEmailLength)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(ValidationConstants.MaxEmailLength)]
    public string EmailNormalized { get; set; } = string.Empty;
    
    [MaxLength(ValidationConstants.MaxPasswordHashLength)]
    public string PasswordHash { get; set; } = string.Empty;

    public virtual ICollection<TokenEntity> Tokens { get; set; } = new List<TokenEntity>();
    public virtual ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}