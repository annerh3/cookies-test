using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CookiesProyect.Services
{
    public class RefreshTokenDto
    {
        //[Required(ErrorMessage = "El Token es requerido.")]
        [AllowNull]
        public string Token { get; set; }

        //[Required(ErrorMessage = "El Refresh Token es requerido.")]
        [AllowNull]
        public string RefreshToken { get; set; }
    }
}
