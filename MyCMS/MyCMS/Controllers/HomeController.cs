using Microsoft.AspNetCore.Mvc;

namespace MyCMS.Controllers
{
    /// <summary>
    /// Default MVC Controller
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Open the wwwroot/index.html
        /// </summary>
        public IActionResult Index() => File("/index.html", "text/html");
    }
}
