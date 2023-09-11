using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarvedRock.WebApp.Pages
{
    public class GetInTouchModel : PageModel
    {
        public void OnGet()
        {
        }
        public void OnPost()
        {
            var content = Request.Form["content"];
            var emaildAddress = Request.Form["emailaddress"];
        }
    }
}
