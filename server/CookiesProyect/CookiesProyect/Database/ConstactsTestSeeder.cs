using CookiesProyect.Constants;
using CookiesProyect.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CookiesProyect.Database
{
    public class ConstactsTestSeeder
    {

        // Lista de Clientes Quemados
        private static List<object> userClients = new List<object>
        {
            new { id = new Guid("ca738dbb-0c10-47b2-a1e7-f61e2e46f515"), name = "marlon_lopez", email = "m_lopez@gmail.com" },
            new { id = new Guid("ee10a477-0bb1-4f24-851a-d275f187f5fd"), name = "shalom_henriquez", email = "s_hqz2@gmail.com" },
            new { id = new Guid("dcf71a2c-d287-4b4a-b16a-f4e380f59959"), name = "ruth_quintanilla", email = "ruthquintanilla3@icloud.com" },
            new { id = new Guid("17a4c7c8-60aa-4d88-9900-666a4ae59ea3"), name = "naara_chavez", email = "naara.chavez@unah.hn" },
            new { id = new Guid("8f5046cf-d8ee-49ac-a0b0-7b1328bde15f"), name = "siscomp", email = "siscomp.hn@gmail.com" },
            new { id = new Guid("ca02aadc-05f1-453f-bf7b-c80562d52e55"), name = "aire_frio", email = "aire.frio@gmail.com" },
            new { id = new Guid("720ce9c4-d31d-46bc-b45b-70ece08ece67"), name = "muni_src", email = "src_muni@gmail.com" },
            new { id = new Guid("c6a65998-231e-4355-bf67-536a243ccfae"), name = "empresa_aguas_src", email = "gerencia@aguasdesantarosa.org" },
            new { id = new Guid("367ea698-7bb6-466f-9482-5a11427b22c0"), name = "iglesia_menonita", email = "menonita_src@gmail.com" },
            new { id = new Guid("3027d32a-e2c5-4935-bfa4-b07a5708b980"), name = "iglesia_catolica_src", email = "e.cat_src@gmail.com" },
            new { id = new Guid("2f1d0ac0-e87b-4daf-b153-b4ba121d4d33"), name = "pilarh", email = "pilarh_hn@gmail.com" },
            new { id = new Guid("374756e0-ce74-4b67-ae49-89b1467a0c0e"), name = "vision_fund", email = "vision_fund@gmail.com" }

        };
        // Carga de los Datos de la APP
        public static async Task LoadDataAsync(
            ContactsDbContext context,
            ILoggerFactory loggerFactory,
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            try
            {
                await LoadRolesAndUsersAsync(userManager, roleManager, loggerFactory);
                await LoadContactsAsync(loggerFactory, context);


            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ConstactsTestSeeder>();
                logger.LogError(e, $"{MessagesConstant.SEEDER_INIT_ERROR}");
            }

        }
        //Agregando los Roles 
        public static async Task LoadRolesAndUsersAsync(
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory
            )
        {
            try
            {
                if (!await roleManager.Roles.AnyAsync())
                {
                    await roleManager.CreateAsync(new IdentityRole(RolesConstants.ADMIN)); // creando roles
                    await roleManager.CreateAsync(new IdentityRole(RolesConstants.USER));

                }

                if (!await userManager.Users.AnyAsync())
                {   
                    for (int i = 0; i < userClients.Count(); i++)// Creando Usuarios que son clientes
                    {
                        dynamic usercClientInfo = userClients[i]; // para poder acceder a las propiedades del objeto

                        var user = new UserEntity
                        {
                            Id = usercClientInfo.id.ToString(),
                            Email = usercClientInfo.email,
                            UserName = usercClientInfo.email,
                        };

                        await userManager.CreateAsync(user, "Temporal01*"); // crear usuario cliente
                        await userManager.AddToRoleAsync(user, RolesConstants.USER); // asignar rol
                    }


                    // agregando los Administradores
                    var userAdmin1 = new UserEntity
                    {
                        Email = "admin@gmail.com",
                        UserName = "admin@gmail.com",
                    };

                    var userAdmin2 = new UserEntity
                    {
                        Email = "annerh3@gmail.com",
                        UserName = "annerh3@gmail.com",
                    };

                    // Agregando las Contrase;as de los Administradores
                    await userManager.CreateAsync(userAdmin1, "Temporal01*");
                    await userManager.CreateAsync(userAdmin2, "Temporal01*");
                    
                    // Agregando Los roles de los Administradores
                    await userManager.AddToRoleAsync(userAdmin1, RolesConstants.ADMIN);
                    await userManager.AddToRoleAsync(userAdmin2, RolesConstants.ADMIN);

                }

            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ConstactsTestSeeder>();
                logger.LogError(e.Message);
            }
        }

        public static async Task LoadContactsAsync(ILoggerFactory loggerFactory, ContactsDbContext context)
        {
            try
            {
                var jsonFilePath = "SeedData/contacts.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var contacts = JsonConvert.DeserializeObject<List<ContactEntity>>(jsonContent);

                if (!await context.Contacts.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();

                    for (int i = 0; i < contacts.Count; i++) // actualiza propiedades de auditoría
                    {
                        contacts[i].CreatedBy = user.Id;
                        contacts[i].CreatedDate = DateTime.Now;
                        contacts[i].UpdatedBy = user.Id;
                        contacts[i].UpdatedDate = DateTime.Now;
                    }

                    context.AddRange(contacts);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ContactsDbContext>();
                logger.LogError(e, "Error al ejecutar el Seed de Contactos.");
            }
        }



    }
}
