using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CookiesProyect.Database.Entities
{
    public class UserEntity : IdentityUser
    {
        [Column("refresh_token")]
        [StringLength(450)]
        public string RefreshToken { get; set; }

        [Column("refresh_token_expire")]
        public DateTime RefreshTokenExpire { get; set; }
    }
}
