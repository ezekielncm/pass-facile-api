using Api.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : Controller
    {
        [Authorize]
        [HttpPut("{id:guid}/profile")]
        public IActionResult me(
            [FromBody]ProfilePutRequest request)
        {
            return View();
        }
    }
}
