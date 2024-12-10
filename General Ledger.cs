using System.Text.Json;
using System.Xml.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (HttpContext context) =>
{
    // GET /employees - List all employees and convert to Sage XML format
    if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/employees"))
    {
        var employees = EmployeesRepository.GetEmployees();
        
        // Create Sage XML query
        var sageXml = new XElement("query",
            new XElement("object", "GLACCOUNT"),
            new XElement("select",
                new XElement("field", "RECORDNO"),
                new XElement("field", "ACCOUNTNO"),
                new XElement("field", "ACCOUNTTYPE")
            ),
            new XElement("filter",
                new XElement("equalto",
                    new XElement("field", "ACCOUNTTYPE"),
                    new XElement("value", "incomestatement")
                )
            )
        );

        // Send to Sage endpoint
        using var client = new HttpClient();
        var response = await client.PostAsync("https://api.sage.com/v1/employees", 
            new StringContent(sageXml.ToString()));

        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsync(response.IsSuccessStatusCode ? 
            "Data successfully sent to Sage" : 
            "Error sending data to Sage");
    }
    // GET /accounts/{id} - Get account by ID
else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/accounts/") == true)
{
    var accountId = context.Request.Path.Value.Split('/').Last();
    
    // Create Sage XML query
    var sageXml = new XElement("readByName",
        new XElement("object", "GLACCOUNT"),
        new XElement("keys", accountId),
        new XElement("fields", "*")
    );

    // Send to Sage endpoint
    using var client = new HttpClient();
    var response = await client.PostAsync("https://api.sage.com/v1/accounts/read", 
        new StringContent(sageXml.ToString()));

    context.Response.StatusCode = (int)response.StatusCode;
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        await context.Response.WriteAsync(content);
    }
    else
    {
        await context.Response.WriteAsync("Error retrieving account from Sage");
    }
}
    // GET / - Show request details
    else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/"))
    {
        await context.Response.WriteAsync($"The method is: {context.Request.Method}\r\n");
        await context.Response.WriteAsync($"The Url is: {context.Request.Path}\r\n");

        await context.Response.WriteAsync($"\r\nHeaders:\r\n");
        foreach (var key in context.Request.Headers.Keys)
        {
            await context.Response.WriteAsync($"{key}: {context.Request.Headers[key]}\r\n");
        }
    }
    // POST /employees - Add new employee with Sage integration
    else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/employees"))
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var employee = JsonSerializer.Deserialize<Employee>(body);
        
        // Create Sage XML query
        var sageXml = new XElement("query",
            new XElement("object", "GLACCOUNT"),
            new XElement("select",
                new XElement("field", "RECORDNO"),
                new XElement("field", "ACCOUNTNO"),
                new XElement("field", "ACCOUNTTYPE")
            ),
            new XElement("filter",
                new XElement("equalto",
                    new XElement("field", "ACCOUNTTYPE"),
                    new XElement("value", "incomestatement")
                )
            )
        );

        // Send to Sage endpoint
        using var client = new HttpClient();
        var response = await client.PostAsync("https://api.sage.com/v1/employees/create", 
            new StringContent(sageXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            EmployeesRepository.AddEmployee(employee);
            context.Response.StatusCode = 201;
            await context.Response.WriteAsync("Employee added successfully to local and Sage systems");
        }
        else
        {
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsync("Failed to add employee to Sage system");
        }
    }
    // PUT /employees - Update employee with Sage integration
    else if (context.Request.Method == "PUT" && context.Request.Path.StartsWithSegments("/employees"))
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var employee = JsonSerializer.Deserialize<Employee>(body);
        
        // Create Sage XML query
        var sageXml = new XElement("query",
            new XElement("object", "GLACCOUNT"),
            new XElement("select",
                new XElement("field", "RECORDNO"),
                new XElement("field", "ACCOUNTNO"),
                new XElement("field", "ACCOUNTTYPE")
            ),
            new XElement("filter",
                new XElement("equalto",
                    new XElement("field", "ACCOUNTTYPE"),
                    new XElement("value", "incomestatement")
                )
            )
        );

        // Send to Sage endpoint
        using var client = new HttpClient();
        var response = await client.PutAsync($"https://api.sage.com/v1/employees/{employee.Id}", 
            new StringContent(sageXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            var result = EmployeesRepository.UpdateEmployee(employee);
            await context.Response.WriteAsync(result ? 
                "Employee updated successfully in local and Sage systems" : 
                "Employee not found locally");
        }
        else
        {
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsync("Failed to update employee in Sage system");
        }
    }
    // DELETE /employees?id={id} - Delete employee with Sage integration
    else if (context.Request.Method == "DELETE" && context.Request.Path.StartsWithSegments("/employees"))
    {
        if (!context.Request.Query.ContainsKey("id"))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Missing employee id");
            return;
        }

        if (!int.TryParse(context.Request.Query["id"], out int employeeId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid employee id format");
            return;
        }

        if (context.Request.Headers["Authorization"] != "frank")
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Not Authorized");
            return;
        }

        // Send delete request to Sage endpoint
        using var client = new HttpClient();
        var response = await client.DeleteAsync($"https://api.sage.com/v1/employees/{employeeId}");

        if (response.IsSuccessStatusCode)
        {
            var result = EmployeesRepository.DeleteEmployee(employeeId);
            await context.Response.WriteAsync(result ?
                "Employee deleted successfully from local and Sage systems" :
                "Employee not found locally");
        }
        else
        {
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsync("Failed to delete employee from Sage system");
        }
    }
});

app.Run();

static class EmployeesRepository
{
    private static List<Employee> employees = new List<Employee>
    {
        new Employee(1, "John Doe", "Engineer", 60000),
        new Employee(2, "Jane Smith", "Manager", 75000),
        new Employee(3, "Sam Brown", "Technician", 50000)
    };

    public static List<Employee> GetEmployees() => employees;

    public static void AddEmployee(Employee? employee)
    {
        if (employee is not null)
        {
            employees.Add(employee);
        }
    }

    public static bool UpdateEmployee(Employee? employee)
    {
        if (employee is not null)
        {
            var emp = employees.FirstOrDefault(x => x.Id == employee.Id);
            if (emp != null)
            {
                emp.Name = employee.Name;
                emp.Position = employee.Position;
                emp.Salary = employee.Salary;

                return true;
            }
        }
        return false;
    }

    public static bool DeleteEmployee(int id)
    {
        var employee = employees.FirstOrDefault(x => x.Id == id);
        if (employee is not null)
        {
            employees.Remove(employee);
            return true;
        }
        return false;
    }
}

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public double Salary { get; set; }

    public Employee(int id, string name, string position, double salary)
    {
        Id = id;
        Name = name;
        Position = position;
        Salary = salary;
    }
}
