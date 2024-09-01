using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LijnBlog.Api.Controllers
{
    [ApiController]
    public class ClientController : ControllerBase
    {
        [Authorize]
        public async Task Login()
        {

        }
    }
}
