using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HttpAudioControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController
    {
        [HttpGet]
        [Route("/")]
        public Task<string> basicGetRequest()
        {
            return Task.FromResult("Hello World!");
        }
    }
}