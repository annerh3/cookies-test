using CookiesProyect.Dtos.Auth;
using CookiesProyect.Dtos.Common;

namespace CookiesProyect.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ResponseDto<LoginResponseDto>> RegisterAsync(RegisterDto dto);
        Task<ResponseDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
        Task<ResponseDto<LoginResponseDto>> ValidateAuth();
    }
}
