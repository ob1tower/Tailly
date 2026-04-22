using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Client, Specialist")]
public class AccountDeletionController : ControllerBase
{

}