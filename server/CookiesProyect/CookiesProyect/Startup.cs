using CookiesProyect.Helpers;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CookiesProyect.Database;
using CookiesProyect.Services.Interfaces;
using CookiesProyect.Services;
using CookiesProyect.Database.Entities;


namespace CookiesProyect
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        // Creacion del Constructor de Startup
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); // agregando los Controladores
            services.AddEndpointsApiExplorer();

            services.AddOpenApi("v1", options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            });

            services.AddControllers().AddNewtonsoftJson(options => // Añadir Controladores con Newtonsoft.Json (del pack: Microsoft.AspNetCore.Mvc.NewtonsoftJson)
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; // Esto le indica a Newtonsoft.Json que ignore las referencias cíclicas durante la serialización.
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });


            //Agregando el DbContext
            services.AddDbContext<ContactsDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Add custom services
            services.AddTransient<IAuditService, AuditService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IContactService, ContactService>();



            // Add Rate Limiting 
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Request.Path.ToString(), // cada endpoint tendrá su propio limite
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3, // Máximo de 100 solicitudes
                            Window = TimeSpan.FromMinutes(1), // las solicitudes de un usuario se cuentan dentro de UN MINUTO, y luego el contador se reinicia al pasar ese tiempo.
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 10 // Permitir cola de hasta 10 solicitudes
                        }));

                options.RejectionStatusCode = 429;
            });

            // Add Identity
            services.AddIdentity<UserEntity, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

            }).AddEntityFrameworkStores<ContactsDbContext>()
              .AddDefaultTokenProviders();

            // Registrar TokenValidationParameters como Singleton
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidAudience = Configuration["JWT:ValidAudience"],
                ValidIssuer = Configuration["JWT:ValidIssuer"],
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
            };
            services.AddSingleton(tokenValidationParameters);

            // agregando el servicio de Autentificacion mediante Token Bearer
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = tokenValidationParameters;

                // Personalizar la respuesta 401
                options.Events = CustomJwtBearerEventsFactory.Create();
            });

            services.AddAuthorization();

            // Utilizado para la validacion con identity en Audit 
            services.AddHttpContextAccessor();



            // CORS Configuration
            services.AddCors(opt =>
            {
                var allowURLS = Configuration.GetSection("AllowURLS").Get<string[]>();

                opt.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins(allowURLS)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            });

            services.AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.All;
                options.MediaTypeOptions.AddText("application/json");

            });

            services.AddLogging(builder =>
            {
                builder.AddConsole(); 
                builder.AddDebug(); 
            });

        }

        public void Configure(WebApplication app, IWebHostEnvironment env) // Configuracion del Middleware:
        {
            if (env.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options.Theme = ScalarTheme.Saturn;
                    options.Title = "Cookies Proyect";
                    options.DefaultHttpClient = new(ScalarTarget.JavaScript, ScalarClient.Axios);
                    options
                       .WithPreferredScheme("Bearer") // Security scheme name from the OpenAPI document
                       .WithHttpBearerAuthentication(bearer =>
                       {
                           bearer.Token = "";
                       });
                });

                app.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/")
                    {
                        context.Response.Redirect("/scalar");  
                        return;
                    }
                    await next();
                });
            }


            //app.UseHttpLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseRateLimiter();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();
        }
    }
}
