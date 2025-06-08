using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CookiesProyect.Constants;
using System.Security.Cryptography;
using System.Text;
using CookiesProyect.Database;
using CookiesProyect.Dtos.Auth;
using CookiesProyect.Dtos.Common;
using CookiesProyect.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using CookiesProyect.Database.Entities;
using CookiesProyect.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Azure.Core;

namespace CookiesProyect.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly ContactsDbContext _context;
        private readonly ILogger<UserEntity> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            ContactsDbContext context,
            ILogger<UserEntity> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

        }

        #region Endpoints
        public async Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            var userEntity = await _userManager.FindByEmailAsync(dto.Email);
            if (userEntity == null) return ResponseHelper.ResponseError<LoginResponseDto>(404, "No se encontró al usuario.");

            // Verificar si el usuario está bloqueado
            var lockoutEndDate = await _userManager.GetLockoutEndDateAsync(userEntity);

            if (lockoutEndDate.HasValue && lockoutEndDate.Value > DateTime.Now) return ResponseHelper.ResponseError<LoginResponseDto>(423, "Este usuario está bloqueado. Intentalo de nuevo más tarde.");

            var result = await _signInManager.
                PasswordSignInAsync(dto.Email,
                                    dto.Password,
                                    isPersistent: false,
                                    lockoutOnFailure: false);


            if (result.Succeeded)
            {
                return await LogInWithToken(userEntity);
            }

            await _userManager.AccessFailedAsync(userEntity);

            return ResponseHelper.ResponseError<LoginResponseDto>(401, "Credenciales incorrectas.");


        }

        public async Task<ResponseDto<LoginResponseDto>> RegisterAsync(RegisterDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new UserEntity
                {
                    UserName = dto.Email,
                    Email = dto.Email,   
                };

                // Creación del usuario
                var result = await _userManager.CreateAsync(user, dto.Password);

                return await CreateUserAndLogin(dto, transaction, user, result);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(); 
                _logger.LogError(e, "Ocurrió un error inesperado al registrar el usuario.");
                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Ocurrió un error inesperado al registrar el usuario."
                };
            }
        }

        public async Task<ResponseDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
        {
            try
            {
                // Obtener tokens desde el body o cookies
                var accessToken = dto?.Token ?? GetCookieValue("token");
                var refreshToken = dto?.RefreshToken ?? GetCookieValue("refresh-token");

                // Utilizar el método de validación con diccionario
                var validationResult = ValidateTokensWithDictionary(accessToken, refreshToken);
                if (validationResult != null) return validationResult;

                var principal = GetTokenPrincipal(accessToken);
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var userId = principal.FindFirst("UserId")?.Value;

                if (email is null)
                    return ResponseHelper.ResponseError<LoginResponseDto>(401, "Acceso no autorizado: no se encontró un correo válido.");

                var user = await _userManager.FindByEmailAsync(email);
                if (user is null)
                    return ResponseHelper.ResponseError<LoginResponseDto>(401, "Acceso no autorizado: el usuario no existe.");

                if (user.RefreshToken != refreshToken)
                    return ResponseHelper.ResponseError<LoginResponseDto>(401, "Acceso no autorizado: la sesión no es válida.");

                if (user.RefreshTokenExpire < DateTime.Now)
                    return ResponseHelper.ResponseError<LoginResponseDto>(401, "Acceso no autorizado: la sesión ha expirado.");

                // Renovar token
                var newClaims = await GetClaims(user);
                var jwtToken = GetToken(newClaims);
                var newRefreshToken = GenerateRefreshTokenString();

                // Actualizar datos del usuario
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpire = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var tokenCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // false SOLO en desarrollo
                    SameSite = SameSiteMode.None, // o None si usas HTTPS en frontend
                    //Expires = jwtToken.ValidTo
                };

                // Convertir el token a string para agregarlo a la token cookie y al body response
                var signedToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

                _httpContextAccessor.HttpContext.Response.Cookies.Append("token", signedToken, tokenCookieOptions);

                var refreshTokenCookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // false SOLO en desarrollo
                    SameSite = SameSiteMode.None,
                    Path = "api/auth/refresh-token", // Solo disponible para esta ruta
                    Expires = user.RefreshTokenExpire
                };

                _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh-token", newRefreshToken, refreshTokenCookieOptions);

                var response = new LoginResponseDto
                {
                    Email = email,
                    Token = signedToken,
                    TokenExpiration = user.RefreshTokenExpire,
                    RefreshToken = newRefreshToken
                };

                return ResponseHelper.ResponseSuccess(200, "Token renovado satisfactoriamente", response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return ResponseHelper.ResponseError<LoginResponseDto>(500, "Ocurrió un error al renovar el token.");
            }
        }

        public async Task<ResponseDto<LoginResponseDto>> ValidateAuth()
        {
            var accessToken = GetCookieValue("token");
            if (string.IsNullOrEmpty(accessToken)) return ResponseHelper.ResponseError<LoginResponseDto>(400, "Token no encontrado"); 
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(accessToken);
                var expiration = jwtToken.ValidTo;
                if (expiration < DateTime.Now) return ResponseHelper.ResponseError<LoginResponseDto>(401, "Token expirado");
                return ResponseHelper.ResponseSuccess<LoginResponseDto>(200, "Token válido", null);
            }
            catch (Exception)
            {
                return ResponseHelper.ResponseError<LoginResponseDto>(403, "Token inválido");
            }
        }

        #endregion


        // Aux Methods

        public ClaimsPrincipal GetTokenPrincipal(string token)
        {
            var securityKey = new SymmetricSecurityKey(Encoding
                .UTF8.GetBytes(_configuration.GetSection("JWT:Secret").Value));

            var validation = new TokenValidationParameters
            {
                IssuerSigningKey = securityKey,
                ValidateLifetime = false,
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateAudience = false
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
        }

        private string GetCookieValue(string cookieName)
        {
            return _httpContextAccessor?.HttpContext?.Request?.Cookies[cookieName];
        }
     
        private ResponseDto<LoginResponseDto> ValidateTokensWithDictionary(string accessToken, string refreshToken)
        {
            // Crear diccionario con las validaciones
            var validationErrors = new Dictionary<string, bool>
            {
                { "token", string.IsNullOrEmpty(accessToken) },
                { "refresh-token", string.IsNullOrEmpty(refreshToken) }
            };

            // Verificar qué tokens faltan
            var missingTokens = validationErrors.Where(x => x.Value).Select(x => x.Key).ToList();

            // Generar mensaje de error basado en las validaciones
            if (missingTokens.Count > 0)
            {
                if (missingTokens.Count == 2)
                {
                    return ResponseHelper.ResponseError<LoginResponseDto>(400,
                        "Acceso no autorizado: faltan token y refresh token.");
                }

                return ResponseHelper.ResponseError<LoginResponseDto>(400,
                    $"Acceso no autorizado: falta el {missingTokens[0]}.");
            }

            // Si no hay errores, retornar null
            return null;
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddMinutes(int.Parse(_configuration["JWT:Expires"] ?? "30")),
                    claims: authClaims, signingCredentials: new SigningCredentials(
                                                                authSigninKey,
                                                                SecurityAlgorithms.HmacSha256)
             );
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("---------- > > > > > " + DateTime.Now.AddMinutes(int.Parse(_configuration["JWT:Expires"] ?? "30")));
            Console.ResetColor();
            var secret = _configuration["JWT:Secret"];
            var iss = _configuration["JWT:ValidIssuer"];

            return token;
        }

        private async Task<List<Claim>> GetClaims(UserEntity userEntity)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, userEntity.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserId", userEntity.Id),
                };

            var userRoles = await _userManager.GetRolesAsync(userEntity);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            return authClaims;
        }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        private async Task<ResponseDto<LoginResponseDto>> LogInWithToken(UserEntity userEntity)
        {
            // Generación del Token
            //var userEntity = await _userManager.FindByEmailAsync(dto.Email);
      

            // ClaimList creation
            List<Claim> authClaims = await GetClaims(userEntity);

            var jwtToken = GetToken(authClaims);

            var refreshToken = GenerateRefreshTokenString();

            userEntity.RefreshToken = refreshToken;
            userEntity.RefreshTokenExpire = DateTime.Now
                .AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));

            _context.Entry(userEntity);
            await _context.SaveChangesAsync();

            var tokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // false SOLO en desarrollo
                SameSite = SameSiteMode.None, // o None si usas HTTPS en frontend
                Expires = userEntity.RefreshTokenExpire
            };

            var signedToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            _httpContextAccessor.HttpContext.Response.Cookies.Append("token",signedToken, tokenCookieOptions);

            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // false SOLO en desarrollo
                SameSite = SameSiteMode.None, 
                Path = "api/auth/refresh-token", // Solo disponible para esta ruta
                Expires = userEntity.RefreshTokenExpire
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refresh-token", refreshToken, refreshTokenCookieOptions);


            return new ResponseDto<LoginResponseDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Inicio de sesion satisfactorio",
                Data = new LoginResponseDto
                {
                    Email = userEntity.Email,
                    Token = signedToken, // convertir token en string
                    TokenExpiration = userEntity.RefreshTokenExpire,
                    RefreshToken = refreshToken,
                }
            };
        }

        private async Task<ResponseDto<LoginResponseDto>> CreateUserAndLogin(RegisterDto dto, IDbContextTransaction transaction, UserEntity user, IdentityResult result)
        {
            if (!result.Succeeded)
            {
                List<IdentityError> errorList = result.Errors.ToList();  // Listamos los errores
                string errors = "";

                foreach (var error in errorList)
                {
                    errors += error.Description;
                    if (error.Code == "DuplicateEmail") // si el error trata de DuplicateEmail, personalizar ErrorMessage
                    {
                        return new ResponseDto<LoginResponseDto>
                        {
                            StatusCode = 400,
                            Status = false,
                            Message = "El email ya está registrado en el sistema."
                        };
                    }
                }

                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 400,
                    Status = false,
                    Message = errors
                };
            }

            // Usuario creado exitosamente
            var userEntity = await _userManager.FindByEmailAsync(dto.Email);
            await _userManager.AddToRoleAsync(userEntity, RolesConstants.USER);

            await _context.SaveChangesAsync(); 

            await transaction.CommitAsync();

            var authClaims = new List<Claim>
            {
                new (ClaimTypes.Email, userEntity.Email),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new ("UserId", userEntity.Id),
                new (ClaimTypes.Role, RolesConstants.USER)
            };

            var jwtToken = GetToken(authClaims);
            var refreshToken = GenerateRefreshTokenString();

            userEntity.RefreshToken = refreshToken;
            userEntity.RefreshTokenExpire = DateTime.Now
                .AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));

            _context.Entry(userEntity);
            await _context.SaveChangesAsync();


            return new ResponseDto<LoginResponseDto>
            {
                StatusCode = 200,
                Status = true,
                Message = "Registro de usuario realizado satisfactoriamente.",
                Data = new LoginResponseDto
                {
                    Email = userEntity.Email,
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtToken), // convertir token en string
                    TokenExpiration = jwtToken.ValidTo,
                    RefreshToken = refreshToken,
                }
            };
        }  
    }
    }
