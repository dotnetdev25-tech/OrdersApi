using MyAwesomeApp.Shared;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using OrdersApi.Dtos;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using RazorLight;

namespace OrdersApi.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly NpgsqlDataSource _db;
        private readonly RazorLightEngine _razor;

        public CustomerController(ILogger<CustomerController> logger, NpgsqlDataSource db, IConfiguration config)
        {
            _razor = new RazorLightEngineBuilder() .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates")) .UseMemoryCachingProvider() .Build();
            _logger = logger;
            _configuration = config;
            _db = db;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<CustomerDto>), 200)]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? format = null)
        {
            //_logger.LogCritical("\n IN MY CONTROLLERGETALLxxxxxxxxx");
            // Get total count
            var countSql = "SELECT COUNT(*) FROM customers";
            await using var countCmd = _db.CreateCommand(countSql);
            countCmd.LogParameters(_logger);
            _logger.LogInformation("COUNT SQL: {Sql}", countCmd.CommandText);
            var totalCountObj = await countCmd.ExecuteScalarAsync();
            if (totalCountObj is not long totalCount)
            {
                totalCount = 0;
            }

            var offset = (page - 1) * pageSize;
            var sql = @"SELECT customers.id, name, email, type_name FROM customers, customer_type WHERE customers.customer_type_id = customer_type.id ORDER BY customers.id LIMIT @pageSize OFFSET @offset";
            await using var cmd = _db.CreateCommand(sql);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.LogParameters(_logger);
            _logger.LogInformation("GET ALL SQL: {Sql}", cmd.CommandText);
            var  customershared = new Customer
            {
                id=99,
                last_name = "last",
                shared = "nothin",
            };
            _logger.LogCritical("\n SHAREDCUSTOMER OBJECT:"+customershared.shared);
            var customers = new List<CustomerDto>();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(new CustomerDto
                {
                    customerid = reader.GetInt32(0),
                    customerName = reader.GetString(1),
                    customerEmail = reader.GetString(2),
                    customerType = reader.GetString(3)
                });
            }

            format = format?.ToLowerInvariant() ?? "json";

            if (format == "html")
            {
                var html = RenderCustomersHtml(customers, page, pageSize, (int)totalCount);
                return Content(html, "text/html");
            }

            var response = new PaginatedResponse<CustomerDto>
            {
                Items = customers,
                Page = page,
                PageSize = pageSize,
                TotalCount = (int)totalCount
            };

            return Ok(response);
        }
            [HttpGet("{id}/html")]
    public async Task<IActionResult> GetCustomerHtml(int id)
    {
        _logger.LogInformation("\n!!!!!INHTTPGET ID HTML ");
                 const string sql = @"
    select
    c.id as customer_id,
    c.name as customer_name,
    c.email as customer_email
    from customers c
    where c.id = $1
   ";
            await using var cmd = _db.CreateCommand(sql);
            cmd.Parameters.AddWithValue(id);
            cmd.LogParameters(_logger);

            _logger.LogInformation("\n!!!!!!!!!!!!!GET1 SQL: {Sql}", cmd.CommandText);

    await using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        var customer = new CustomerDto
        {
         customerName = reader.GetString(1),
         customerEmail = reader.GetString(2)
        };
     var html = await _razor.CompileRenderAsync("CustomerSummary.cshtml", customer);
    return Content(html, "text/html");
    }

      // var html =""; 

        return Ok();
    }
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            _logger.LogInformation("\n!!!!!INHTTP POST ");
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO customers (name, email)
                VALUES (@firstName, @email)
                RETURNING id;
            ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@firstName", dto.Name);
            cmd.Parameters.AddWithValue("@email", dto.Email);

            cmd.LogParameters(_logger);

            _logger.LogInformation("SQL: {Sql}", cmd.CommandText);

            var newIdObj = await cmd.ExecuteScalarAsync();
            if (newIdObj is not int newId)
            {
                throw new InvalidOperationException("Failed to retrieve new customer ID.");
            }

            var createdCustomer = new
            {
                Id = newId,
                dto.Name,
                dto.Email,
                _links = new
                {
                    self = new { href = $"/customer/{newId}" },
                    update = new { href = $"/customer/{newId}", method = "PUT" }
                }
            };

            return Created($"/customer/{newId}", createdCustomer);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
        {
            _logger.LogInformation("\n!!!!!IN HTTP PUT ID ");
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            _logger.LogInformation("!!!!!put commend conn string:" + connectionString);

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            const string sql = @"
                UPDATE customers
                SET name = @firstName,

                    email = @email
                WHERE id = @id;
            ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@firstName", dto.Name);
            cmd.Parameters.AddWithValue("@email", dto.Email);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.LogParameters(_logger);

            _logger.LogInformation("\n PUT SQL: {Sql}", cmd.CommandText);

            var rows = await cmd.ExecuteNonQueryAsync();
            return Ok(rows);

        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CustomerResponse), 200)]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id, [FromQuery] string? format = null)
        {
            _logger.LogInformation("\n!!!!!INHTTP GETBYID ");
            const string sql = @"
    select
    c.id as customer_id,
    c.name as customer_name,
    c.email as customer_email,
    o.id as order_id,
    o.order_date,
    i.id as item_id,
    i.product_name,
    i.quantity,
    i.unit_price
        from customers c
   left join orders o on o.customer_id = c.id
   left join order_items i on i.order_id = o.id
    where c.id = $1
  order by o.id, i.id;
   ";

            await using var cmd = _db.CreateCommand(sql);
            cmd.Parameters.AddWithValue(id);
            cmd.LogParameters(_logger);

            _logger.LogInformation("GET1 SQL: {Sql}", cmd.CommandText);

            await using var reader = await cmd.ExecuteReaderAsync();

            CustomerResponse? customer = null;
            var orders = new Dictionary<int, OrderResponse>();
   //0      c.id as customer_id,
   //1 c.name as customer_name,
   //2 c.email as customer_email,
   //3 o.id as order_id,
   //4 o.order_date,
   //5 i.id as item_id,
   //6 i.product_name,
   //7 i.quantity,
   //8 i.unit_price,
   //9 ct.type_name
   // from customers c

            while (await reader.ReadAsync())
            {
                if (customer is null)
                {
                    customer = new CustomerResponse{
                       customerId= reader.GetInt32(0),
                        customerName=reader.GetString(1),
                        customerEmail=reader.GetString(2),
                        Orders= new List<OrderResponse>(),
                        customerType ="fixin"
                    };
                }

                if (!reader.IsDBNull(3))
                {
                    var orderId = reader.GetInt32(3);

                    if (!orders.TryGetValue(orderId, out var order))
                    {
                        order = new OrderResponse{
                            Id = orderId,
                            OrderDate =reader.GetDateTime(4),
                            Items=new List<OrderItemResponse>(),
                        };

                        orders[orderId] = order;
                        customer.Orders.Add(order);
                    }

                    if (!reader.IsDBNull(5))
                    {
                        order.Items.Add(new OrderItemResponse(
                            reader.GetInt32(5),
                            reader.GetString(6),
                            reader.GetInt32(7),
                            reader.GetDecimal(8)
                        ));
                    }
                }
            }

            if (customer is null)
            {
                return NotFound();
            }
            format = format?.ToLowerInvariant() ?? "json";

            if (format == "html")
            {
                var html = RenderCustomerDetailHtml(customer);
                return Content(html, "text/html");
            }

            return Ok(customer);
        }

        [HttpGet("{id:int}/edit")]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("\n!!!!!INHTTPGET EDIT ");
            const string sql = @"SELECT id, name, email FROM customers WHERE id = $1";
            await using var cmd = _db.CreateCommand(sql);
            cmd.Parameters.AddWithValue(id);
            cmd.LogParameters(_logger);
            _logger.LogInformation("EDIT SQL: {Sql}", cmd.CommandText);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var customerId = reader.GetInt32(0);
                var name = reader.GetString(1);
                var email = reader.GetString(2);

                var html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Edit Customer - {name}</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
</head>
<body class='bg-light'>
    <div class='container mt-5'>
        <div class='row justify-content-center'>
            <div class='col-md-6'>
                <div class='card'>
                    <div class='card-header'>
                        <h1 class='h4 mb-0'>Edit Customer</h1>
                    </div>
                    <div class='card-body'>
                        <form id='editForm'>
                            <input type='hidden' id='customerId' value='{customerId}' />
                            <div class='mb-3'>
                                <label for='name' class='form-label'>Name:</label>
                                <input type='text' class='form-control' id='name' value='{name}' required />
                            </div>
                            <div class='mb-3'>
                                <label for='email' class='form-label'>Email:</label>
                                <input type='email' class='form-control' id='email' value='{email}' required />
                            </div>
                            <button type='submit' class='btn btn-primary'>Update Customer</button>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script src='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js'></script>
    <script>
        document.getElementById('editForm').addEventListener('submit', async (e) => {{
            e.preventDefault();
            const customerId = document.getElementById('customerId').value;
            const name = document.getElementById('name').value;
            const email = document.getElementById('email').value;
        
            const response = await fetch(`/order/${{customerId}}`, {{
                method: 'PUT',
                headers: {{ 'Content-Type': 'application/json' }},
                body: JSON.stringify({{ name, email }})
            }});
                  
            if (response.ok) {{
                alert('Customer updated successfully!');
                // Optionally redirect or refresh
            }} else {{
                alert('Error updating customer.');
            }}
        }});
    </script>
</body>
</html>";
                return Content(html, "text/html");
            }
            return NotFound();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("\n!!!!!INHTTP DELETE ID ");
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            const string sql = @"
                DELETE FROM customers
                WHERE id = @id;
            ";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            cmd.LogParameters(_logger);

            _logger.LogInformation("DELETE SQL: {Sql}", cmd.CommandText);

            return rows == 0
                ? NotFound($"Customer with id {id} not found.")
                :  Ok(new { message = $"Customer {id} was successfully removed." });

        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<CustomerDto>), 200)]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Search([FromQuery] string searchName, [FromQuery] string? format)
        {
            _logger.LogInformation("\n!!!!!INHTTPGET SEARCH ");
            var sql = @"
                SELECT customers.id, name, email,type_name
                FROM customers, customer_type
                WHERE name ILIKE @searchName
                and customers.customer_type_id=customer_type.id
                ORDER BY name;
            ";

            var customers = new List<CustomerDto>();

            await using var cmd = _db.CreateCommand(sql);
            cmd.Parameters.AddWithValue("searchName", $"%{searchName}%");
            await using var reader = await cmd.ExecuteReaderAsync();
            cmd.LogParameters(_logger);

            _logger.LogInformation("SEARCH SQL: {Sql}", cmd.CommandText);

            while (await reader.ReadAsync())
            {
                customers.Add(new OrdersApi.Dtos.CustomerDto
                {
                    customerid = reader.GetInt32(0),
                    customerName = reader.GetString(1),
                    customerEmail = reader.GetString(2),
                    customerType = reader.GetString(3)
                });
            }

            format = format?.ToLowerInvariant() ?? "json";

            IActionResult result = format switch
            {
                "json" => Ok(customers),
                "html" => Content(RenderHtml(customers), "text/html"),
                "csv" => Content(RenderCsv(customers), "text/csv"),
                "pdf" => File(RenderPdf(customers), "application/pdf", "customers.pdf"),
                _ => BadRequest("Invalid format")
            };

            return result;
        }

        private static byte[] RenderPdf(List<CustomerDto> customers)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Customer Report").FontSize(20).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .Background("#4A90E2")
                                .Padding(5)
                                .Text("ID")
                                .FontColor("#dde3ea")
                                .Bold();

                            header.Cell()
                                .Background("#4A90E2")
                                .Padding(5)
                                .Text("Name")
                                .FontColor("#dde3ea")
                                .Bold();

                            header.Cell()
                                .Background("#4A90E2")
                                .Padding(5)
                                .Text("Email")
                                .FontColor("#dde3ea")
                                .Bold();
                        });

                        foreach (var c in customers)
                        {
                            table.Cell().Text(c.customerid.ToString());
                            table.Cell().Text(c.customerName);
                            table.Cell().Text(c.customerEmail);
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static string RenderHtml(List<CustomerDto> customers)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body><table border='1'>");
            sb.Append("<tr><th>ID</th><th>Name</th><th>Email</th></tr>");

            foreach (var c in customers)
            {
                sb.Append($"<tr><td>{c.customerid}</td><td>{c.customerName}</td><td>{c.customerEmail}</td></tr>");
            }

            sb.Append("</table></body></html>");
            return sb.ToString();
        }

        private static string RenderCsv(List<CustomerDto> customers)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Email");

            foreach (var c in customers)
            {
                sb.AppendLine($"{c.customerid},{c.customerName},{c.customerEmail}");
            }

            return sb.ToString();
        }

        private static string RenderCustomersHtml(List<CustomerDto> customers, int page, int pageSize, int totalCount)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var sb = new StringBuilder();

            sb.Append(@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Customers List</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
</head>
<body class='bg-light'>
    <div class='container mt-5'>
        <div class='row'>
            <div class='col-12'>
                <div class='card'>
                    <div class='card-header d-flex justify-content-between align-items-center'>
                        <h1 class='h4 mb-0'>Customers</h1>
                        <span class='badge bg-primary'>Page " + page + @" of " + totalPages + @" (" + totalCount + @" total)</span>
                    </div>
                    <div class='card-body'>
                        <div class='table-responsive'>
                            <table class='table table-striped table-hover'>
                                <thead class='table-dark'>
                                    <tr>
                                        <th>ID</th>
                                        <th>Name</th>
                                        <th>Email</th>
                                        <th>Type</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>");

            foreach (var customer in customers)
            {
                sb.Append($@"
                                    <tr>
                                        <td>{customer.customerid}</td>
                                        <td>{customer.customerName}</td>
                                        <td>{customer.customerEmail}</td>
                                        <td><span class='badge bg-secondary'>{customer.customerType}</span></td>
                                        <td>
                                            <a href='/order/{customer.customerid}?format=html' class='btn btn-sm btn-outline-primary me-1'>View</a>
                                            <a href='/order/{customer.customerid}/edit' class='btn btn-sm btn-outline-warning me-1'>Edit</a>
                                        </td>
                                    </tr>");
            }

            sb.Append(@"
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div class='card-footer'>
                        <nav aria-label='Customer pagination'>
                            <ul class='pagination justify-content-center mb-0'>");

            // Previous button
            if (page > 1)
            {
                sb.Append($"<li class='page-item'><a class='page-link' href='?page={page - 1}&pageSize={pageSize}&format=html'>Previous</a></li>");
            }
            else
            {
                sb.Append("<li class='page-item disabled'><span class='page-link'>Previous</span></li>");
            }

            // Page numbers (show max 5 pages around current)
            var startPage = Math.Max(1, page - 2);
            var endPage = Math.Min(totalPages, page + 2);

            for (int i = startPage; i <= endPage; i++)
            {
                if (i == page)
                {
                    sb.Append($"<li class='page-item active'><span class='page-link'>{i}</span></li>");
                }
                else
                {
                    sb.Append($"<li class='page-item'><a class='page-link' href='?page={i}&pageSize={pageSize}&format=html'>{i}</a></li>");
                }
            }

            // Next button
            if (page < totalPages)
            {
                sb.Append($"<li class='page-item'><a class='page-link' href='?page={page + 1}&pageSize={pageSize}&format=html'>Next</a></li>");
            }
            else
            {
                sb.Append("<li class='page-item disabled'><span class='page-link'>Next</span></li>");
            }

            sb.Append(@"
                            </ul>
                        </nav>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script src='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js'></script>
</body>
</html>");

            return sb.ToString();
        }

        private static string RenderCustomerDetailHtml(CustomerResponse customer)
        {
            var sb = new StringBuilder();

            sb.Append($@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Customer Details - {customer.customerName}</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
</head>
<body class='bg-light'>
    <div class='container mt-5'>
        <div class='row'>
            <div class='col-12'>
                <div class='card'>
                    <div class='card-header d-flex justify-content-between align-items-center'>
                        <h1 class='h4 mb-0'>Customer Details</h1>
                        <div>
                            <a href='/order/{customer.customerId}/edit' class='btn btn-warning btn-sm me-2'>Edit Customer</a>
                            <a href='/order/?format=html' class='btn btn-secondary btn-sm'>Back to List</a>
                        </div>
                    </div>
                    <div class='card-body'>
                        <div class='row mb-4'>
                            <div class='col-md-6'>
                                <h5>Customer Information</h5>
                                <table class='table table-borderless'>
                                    <tr>
                                        <td class='fw-bold'>ID:</td>
                                        <td>{customer.customerId}</td>
                                    </tr>
                                    <tr>
                                        <td class='fw-bold'>Name:</td>
                                        <td>{customer.customerName}</td>
                                    </tr>
                                    <tr>
                                        <td class='fw-bold'>Email:</td>
                                        <td>{customer.customerEmail}</td>
                                    </tr>
                                    <tr>
                                        <td class='fw-bold'>Type:</td>
                                        <td><span class='badge bg-info'>{customer.customerType}</span></td>
                                    </tr>
                                </table>
                            </div>
                        </div>");

            if (customer.Orders.Any())
            {
                sb.Append(@"
                        <h5>Order History</h5>
                        <div class='accordion' id='ordersAccordion'>");

                int orderIndex = 0;
                foreach (var order in customer.Orders)
                {
                    sb.Append($@"
                            <div class='accordion-item'>
                                <h2 class='accordion-header' id='heading{orderIndex}'>
                                    <button class='accordion-button {(orderIndex == 0 ? "" : "collapsed")}' type='button' data-bs-toggle='collapse' data-bs-target='#collapse{orderIndex}' aria-expanded='{(orderIndex == 0 ? "true" : "false")}' aria-controls='collapse{orderIndex}'>
                                        Order #{order.Id} - {order.OrderDate:MMMM dd, yyyy}
                                    </button>
                                </h2>
                                <div id='collapse{orderIndex}' class='accordion-collapse collapse {(orderIndex == 0 ? "show" : "")}' aria-labelledby='heading{orderIndex}' data-bs-parent='#ordersAccordion'>
                                    <div class='accordion-body'>
                                        <div class='table-responsive'>
                                            <table class='table table-sm'>
                                                <thead>
                                                    <tr>
                                                        <th>Item ID</th>
                                                        <th>Product</th>
                                                        <th>Quantity</th>
                                                        <th>Unit Price</th>
                                                        <th>Total</th>
                                                    </tr>
                                                </thead>
                                                <tbody>");

                    foreach (var item in order.Items)
                    {
                        var total = item.Quantity * item.UnitPrice;
                        sb.Append($@"
                                                    <tr>
                                                        <td>{item.Id}</td>
                                                        <td>{item.ProductName}</td>
                                                        <td>{item.Quantity}</td>
                                                        <td>${item.UnitPrice:F2}</td>
                                                        <td>${total:F2}</td>
                                                    </tr>");
                    }

                    sb.Append(@"
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                            </div>");

                    orderIndex++;
                }

                sb.Append(@"
                        </div>");
            }
            else
            {
                sb.Append(@"
                        <div class='alert alert-info'>
                            <h5>No Orders Found</h5>
                            <p>This customer hasn't placed any orders yet.</p>
                        </div>");
            }

            sb.Append(@"
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script src='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js'></script>
</body>
</html>");

            return sb.ToString();
        }
    }
}