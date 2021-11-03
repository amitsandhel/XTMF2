using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace XTMF2.Web.Pages
{
    public class CreateSecretUserModel : PageModel
    {
        public void OnGet()
        {
            XTMF2.Web.Controllers.Server.Runtime.UserController.CreateNew("SecretUser", false, out var _, out var _);
        }
    }
}
