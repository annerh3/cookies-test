using CookiesProyect.Dtos.Common;
using CookiesProyect.Dtos.Contact;
using CookiesProyect.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookiesProyect.Controllers
{
    [ApiController]
    [Route("api/contacts")]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _contactService;
        public ContactsController(IContactService contactService) 
        {
            _contactService = contactService;
        }

        [HttpGet]
        [Authorize]
        //[Authorize(Roles = $"{RolesConstants.ADMIN}")]
        public async Task<ActionResult<ResponseDto<ContactDto>>> GetAll(string searchTerm, int page = 1)
        {
            var response = await _contactService.GetAll(searchTerm, page);
            return StatusCode(response.StatusCode, response);
        }


    }
}
