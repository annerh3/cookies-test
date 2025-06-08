using CookiesProyect.Dtos.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json;

namespace CookiesProyect.Helpers
{
    public static class CustomJwtBearerEventsFactory
    {
        public static JwtBearerEvents Create()
        {
            return new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    // Esto previene que .NET Core siga procesando el error después de tu respuesta
                    context.HandleResponse();

                    // Ajustar encabezados antes de escribir la respuesta
                    if (!context.Response.HasStarted)
                    {
                        context.Response.OnStarting(() =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        return Task.CompletedTask; 
                    });

                    // Se maneja la respuesta de manera controlada
                    var response = new ResponseDto<string>
                    {
                        Message = "No autorizado: el token es inválido, está expirado o no está presente.",
                        Status = false,
                    };

                    var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                    // Se escribe el cuerpo de la respuesta
                    return context.Response.WriteAsync(json);
                    }

                    return Task.CompletedTask;
                },
                
                OnMessageReceived = context =>
                {
                    // 1. Intentar leer el token desde la cookie
                    var cookieToken = context.Request.Cookies["token"];
                    if (!string.IsNullOrEmpty(cookieToken))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("-*-*-*-*-*-*  Token en Cookie  *-*-*-*-*-*-*-*");
                        Console.ResetColor();

                        Console.WriteLine("Token desde la cookie: " + cookieToken);
                        context.Token = cookieToken;
                            
                    }
                    else
                    {
                        // 2. Si no hay cookie, dejar que JwtBearer lea del header Authorization (por defecto)
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("-*-*-*-*-*-*  Token en Header  *-*-*-*-*-*-*-*");
                            Console.ResetColor();

                            Console.WriteLine("Token desde el header: " + authHeader.Substring("Bearer ".Length).Trim());
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                                
                        }
                    }
                        

                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                    {
                        var claimsPrincipal = context.Principal;
                        var expClaim = claimsPrincipal.FindFirst("exp");

                        if (expClaim != null && long.TryParse(expClaim.Value, out long expUnix))
                        {
                            DateTime expirationUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                            DateTime nowUtc = DateTime.UtcNow;

                            if (nowUtc < expirationUtc)
                            {
                                var minutesLeft = (expirationUtc - nowUtc).TotalMinutes;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Token válido. Expira en {minutesLeft:F2} minutos.");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("El token ya ha expirado.");
                            }

                            Console.ResetColor();
                        }

                        return Task.CompletedTask;
                }

            };
        }
    }
}
