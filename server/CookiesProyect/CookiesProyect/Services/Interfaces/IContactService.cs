using CookiesProyect.Dtos.Common;
using CookiesProyect.Dtos.Contact;

namespace CookiesProyect.Services.Interfaces
{
    public interface IContactService
    {
        Task<ResponseDto<PaginationDto<List<ContactDto>>>> GetAll(string searchTerm = "", int page = 1);
    }
}
    