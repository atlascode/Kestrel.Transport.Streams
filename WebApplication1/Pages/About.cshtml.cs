using Atlas.AspNetCore.Server.Kestrel.Transport.Streams;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace WebApplication1.Pages
{
    public class AboutModel : PageModel
    {
        public string Message { get; set; }

        public async Task OnGetAsync()
        {
            var connection = StreamTransport.CreateConnection();
            this.Message = await connection.Get("/contact");
        }
    }
}
