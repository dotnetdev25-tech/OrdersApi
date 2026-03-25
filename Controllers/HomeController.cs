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
    public IActionResult Index(){
        _logger.LogInformation("\n!!!!!!home!!!!!!!");
     var html = """
    <html>
    <head><title>Reports</title></head>
    <body>
        <h1>Availablexxx Reports</h1>
        <ul>
            <li><a href="http://localhost:5000/Customer?page=1&pageSize=10&format=html">customer list</a></li>
            <li><a href="/api/orders/running-totals?format=html">Running Totals</a></li>
            <li><a href="/api/customers/summary?format=html">Customer Summary</a></li>
            <li><a href="http://localhost:5000/Customer/search?searchName=jjt&format=html">Customer search</a></li>
        </ul>
    </body>
    </html>
    """; 
    

    
    var text = """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{{ page_title }}</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <script>
        async function generateReport(event) {
            console.log("Generating report...");
            event.preventDefault();

            const form = event.target;

            try {
                const response = await fetch(form.action, {
                    method: form.method,
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                const reportHtml = await response.text();
                document.getElementById('report-container').innerHTML = reportHtml;
                document.title = document.getElementById('report_type').value;
                console.log("Report generated for type: " + document.title);

            } catch (error) {
                console.error("Error generating report:", error);
                document.getElementById('report-container').innerHTML = '<p class="text-danger">Error generating report. Please try again.</p>';
            }
        }

        function showReportParameters() {
            const reportType = document.getElementById('report_type').value;
            const dateParams = document.getElementById('date-params');

            if (reportType === 'callsbyserver' || reportType === 'allcalls') {
                dateParams.classList.remove('d-none');
            } else {
                dateParams.classList.add('d-none');
            }
        }

        document.addEventListener('DOMContentLoaded', () => {
            document.getElementById('report_type').addEventListener('change', showReportParameters);
        });
    </script>
</head>
<body class="container mt-4 d-flex justify-content-center">
    <style>
        .form-label { white-space: nowrap; }
    </style>
    <form action="/reports" method="get" onsubmit="generateReport(event)" class="w-100" style="max-width: 640px;">
        <div class="card border rounded shadow-sm p-4 mb-4 bg-white">
            <div class="row mb-3 align-items-center">
                <label for="report_type" class="col-sm-4 col-form-label">Report Type:</label>
                <div class="col-sm-8">
                    <select id="report_type" name="report_type" class="form-select" onchange="showReportParameters()">
                        <option value="summaryreport">Summary Report (sanity check)</option>
                        <option value="callsbyserver">Calls by Server</option>
                        <option value="callsbydnis">Calls by DNIS</option>
                        <option value="callsbyhour">Calls by Hour</option>
                        <option value="callsbydate">Calls by Date</option>
                        <option value="allcalls">All Calls</option>
                        <option value="trendingreport">Trending Report</option>
                        <option value="serveragg">Server Agg Report</option>
                        <option value="concurrentusage">Concurrent Usage Report</option>
                        <option value="kpis">KPI Report</option>
                    </select>
                </div>
            </div>
            <div class="row mb-3 align-items-center">
                <label for="date_range" class="col-sm-4 col-form-label">Date Range:</label>
                <div class="col-sm-8">
                    <select id="date_range" name="date_range" class="form-select">
                        <option value="today">Today</option>
                        <option value="alltime">All Time</option>
                        <option value="yesterday">Yesterday</option>
                        <option value="this_month">This Month</option>
                        <option value="last_7_days">Last 7 Days</option>
                    </select>
                </div>
            </div>
            <div id="date-params" class="d-none">
                <div class="row mb-3 align-items-center">
                    <label for="start_date" class="col-sm-4 col-form-label">Start Date:</label>
                    <div class="col-sm-8">
                        <input type="date" id="start_date" name="start_date" class="form-control" value="2023-10-21">
                    </div>
                </div>
                <div class="row mb-3 align-items-center">
                    <label for="serverid" class="col-sm-4 col-form-label">Server ID:</label>
                    <div class="col-sm-8">
                        <input type="number" id="serverid" name="serverid" class="form-control">
                    </div>
                </div>
                <div class="row mb-3 align-items-center">
                    <label for="end_date" class="col-sm-4 col-form-label">End Date:</label>
                    <div class="col-sm-8">
                        <input type="date" id="end_date" name="end_date" class="form-control">
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col text-end">
                    <button type="submit" class="btn btn-primary">Generate Report</button>
                </div>
            </div>
        </div>
    </form>
    <div id="report-container" class="mt-4 card p-3 shadow-lg" style="max-height: 600px; overflow-y: auto; width: 100%; max-width: 640px;">
    </div>
</body>
</html>
""";  
    var testhtml = """
<!DOCTYPE html>
<html>
  <head>
    <meta charset="utf-8">
    <title>Simple Layout</title>
    <style>
      body {
        margin: 0;
        font-family: sans-serif;
      }

      .container {
        width: 900px;
        margin: 0 auto;
        border: 1px solid #460202;
      }

      .header,
      .footer {
        background: #333;
        color: white;
        padding: 20px;
      }

      .content-area {
        padding: 10px;
      }

      .main {
        display: inline-block;
        width: 65%;
        vertical-align: top;
        background: #f0f0f0;
        padding: 10px;
      }

      .sidebar {
        display: inline-block;
        width: 30%;
        vertical-align: top;
        background: #e0e0ff;
        padding: 10px;
      }
    </style>
  </head>
  <body>
    <div class="container">
      <div class="header">Header</div>

      <div class="content-area">
        <div class="main">
   <p>Main content1</p>
        </div>
        <div class="sidebar">
          <p>sidebar content1</p>
        </div>
      </div>

      <div class="footer">Footer</div>
    </div>
  </body>
</html>
"""; 
var model = new { PageTitle = "Reports" };
var htmlrazor = _razor.CompileRenderAsync("home.cshtml", model).Result;
return Content(htmlrazor, "text/html");
   //  var htmlx =  _razor.CompileRenderAsync("CustomerSummary.cshtml", customer);
  //  return Content(text, "text/html");
      
        return Content(text, "text/html");
   // }
}
}
