using Elattaba.API.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Elattaba.API.Controllers
{
    [Route("errors/{statusCode}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)] 
    public class ErrorsController : ControllerBase
    {
        public IActionResult Errors(int statusCode)
        {
            return new ObjectResult(new ResponseAPI(statusCode));
        }
    }
}