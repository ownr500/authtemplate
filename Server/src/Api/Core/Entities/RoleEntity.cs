using System.ComponentModel.DataAnnotations;
using API.Core.Enums;

namespace API.Core.Entities;

public class RoleEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Role Role { get; set; }

    public virtual ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}