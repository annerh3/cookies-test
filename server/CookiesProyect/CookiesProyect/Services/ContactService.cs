using CookiesProyect.Database;
using CookiesProyect.Database.Entities;
using CookiesProyect.Dtos.Common;
using CookiesProyect.Dtos.Contact;
using CookiesProyect.Helpers;
using CookiesProyect.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CookiesProyect.Services
{
    public class ContactService : IContactService
    {
        private readonly ContactsDbContext _context;
        private readonly ILogger<IContactService> _logger;
        private readonly int PAGE_SIZE;

        public ContactService(
            ContactsDbContext context,
            ILogger<IContactService> logger,
            IConfiguration configuration
            )
        {
            _context = context;
            _logger = logger;
            PAGE_SIZE = configuration.GetValue<int>("PageSize");
        }
        public async Task<ResponseDto<PaginationDto<List<ContactDto>>>> GetAll(string searchTerm = "", int page = 1)
        {
            page = Math.Max(1, page);
            int startIndex = (page - 1) * PAGE_SIZE;

            // Consulta base con proyección temprana
            var query = _context.Contacts.AsNoTracking(); // AsNoTracking para lecturas mejora el rendimiento

            // Aplicar filtro más eficiente
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string searchLower = searchTerm.ToLower();
                query = query.Where(x =>
                    EF.Functions.Like(x.FirstName.ToLower(), $"%{searchLower}%") ||
                    EF.Functions.Like(x.LastName.ToLower(), $"%{searchLower}%") ||
                    EF.Functions.Like(x.Country.ToLower(), $"%{searchLower}%") ||
                    EF.Functions.Like(x.City.ToLower(), $"%{searchLower}%")
                );
            }

            // Hacer una sola consulta para obtener total y datos
            var totalContacts = await query.CountAsync();

            // Aplicar ordenamiento y paginación
            var contacts = await query
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName) // Orden secundario para mayor estabilidad
                .Skip(startIndex)
                .Take(PAGE_SIZE)
                .Select(c => new ContactDto // Proyección directa
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Telephone = c.Telephone,
                    City = c.City,
                    Country = c.Country,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude
                })
                .ToListAsync();

            int totalPages = (int)Math.Ceiling((double)totalContacts / PAGE_SIZE);

            // Construir respuesta
            var paginationDto = new PaginationDto<List<ContactDto>>
            {
                CurrentPage = page,
                PageSize = PAGE_SIZE,
                TotalItems = totalContacts,
                TotalPages = totalPages,
                Items = contacts,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages,
            };

            return ResponseHelper.ResponseSuccess(200, "Lista de contactos obtenida correctamente.", paginationDto);
        }
    }
}
