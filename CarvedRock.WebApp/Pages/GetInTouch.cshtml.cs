using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarvedRock.WebApp.Pages
{
    public class GetInTouchModel : PageModel
    {
        [BindProperty] 
        public string Content { get; set; }
        public void OnGet()
        {
        }
        public async void OnPostAsync()
        {

            var bestContent = Content;

            var form= await Request.ReadFormAsync();

            var betterContent = form["content"];
            var betterEmail = form["emailaddress"];

            var content = Request.Form["content"];
            var emaildAddress = Request.Form["emailaddress"];
        }
    }
}
