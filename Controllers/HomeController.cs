using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RazorLight;
namespace OrdersApi.Controllers;
using OrdersApi.Dtos;
//{
  [ApiController]
 // [Route("[controller]")]
  [Route("/")]
  

public class HomeController : ControllerBase
{
     private readonly ILogger<CustomerController> _logger;
     private readonly RazorLightEngine _razor;
      private readonly NpgsqlDataSource _db;

     public HomeController(ILogger<CustomerController> logger, NpgsqlDataSource db, IConfiguration config)
        {
            _razor = new RazorLightEngineBuilder() .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates")) .UseMemoryCachingProvider() .Build();
            _logger = logger;
          //  _configuration = config;
            _db = db;
        }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Home page requested sync");

        var model = new { PageTitle = "Report SELECTION" };
        var htmlrazor = await _razor.CompileRenderAsync("home.cshtml", model);
        return Content(htmlrazor, "text/html");
    }
}
