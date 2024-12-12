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
    // GET /accounts - List all checking accounts
    if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/accounts"))
    {
        // Create query XML for checking accounts
        var queryXml = new XElement("query",
            new XElement("object", "CHECKINGACCOUNT"),
            new XElement("select",
                new XElement("field", "RECORDNO"),
                new XElement("field", "DESCRIPTION")
            )
        );

        // Send to Sage endpoint
        using var client = new HttpClient();
        var response = await client.PostAsync("https://api.sage.com/v1/accounts", 
            new StringContent(queryXml.ToString()));

        context.Response.StatusCode = (int)response.StatusCode;
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            await context.Response.WriteAsync(content);
        }
        else
        {
            await context.Response.WriteAsync("Error retrieving checking accounts");
        }
    }
    // GET /accounts/{id} - Get checking account by ID
    else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/accounts/") == true)
    {
        var accountId = context.Request.Path.Value.Split('/').Last();
        
        // Create read XML for specific checking account
        var readXml = new XElement("readByName",
            new XElement("object", "CHECKINGACCOUNT"),
            new XElement("keys", accountId),
            new XElement("fields", "*")
        );

        // Send to Sage endpoint
        using var client = new HttpClient();
        var response = await client.PostAsync("https://api.sage.com/v1/accounts/read", 
            new StringContent(readXml.ToString()));

        context.Response.StatusCode = (int)response.StatusCode;
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            await context.Response.WriteAsync(content);
        }
        else
        {
            await context.Response.WriteAsync("Error retrieving checking account");
        }
    }
    // GET /reconciliations - List checking account reconciliations
    else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/reconciliations"))
    {
        // Create query XML for reconciliations
        var queryXml = new XElement("query",
            new XElement("object", "BANKACCTRECON"),
            new XElement("select",
                new XElement("field", "RECORDNO"),
                new XElement("field", "STATE"),
                new XElement("field", "FINANCIALENTITY")
            ),
            new XElement("filter",
                new XElement("equalto",
                    new XElement("field", "STATE"),
                    new XElement("value", "initiated")
                )
            )
        );

        // Send to Sage endpoint
        using var client = new HttpClient();
        var response = await client.PostAsync("https://api.sage.com/v1/reconciliations", 
            new StringContent(queryXml.ToString()));

        context.Response.StatusCode = (int)response.StatusCode;
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            await context.Response.WriteAsync(content);
        }
        else
        {
            await context.Response.WriteAsync("Error retrieving reconciliations");
        }
    }
    // GET /reconciliations/{id} - Get specific reconciliation
    else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/reconciliations/") == true)
    {
        var reconId = context.Request.Path.Value.Split('/').Last();

        // Create query XML for specific reconciliation
        var queryXml = new XElement("query",
            new XElement("object", "BANKACCTRECON"),
            new XElement("select",
                new XElement("field", "RECORDNO"),
                new XElement("field", "STATE"),
                new XElement("field", "FINANCIALENTITY")
            ),
            new XElement("filter",
                new XElement("equalto",
                    new XElement("field", "STATE"),
                    new XElement("value", "initiated")
                )
            )
        );

        // Send to Sage endpoint
        using var client = new HttpClient();
        var response = await client.PostAsync($"https://api.sage.com/v1/reconciliations/{reconId}", 
            new StringContent(queryXml.ToString()));

        context.Response.StatusCode = (int)response.StatusCode;
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            await context.Response.WriteAsync(content);
        }
        else
        {
            await context.Response.WriteAsync("Error retrieving reconciliation");
        }
    }
    // POST /reconciliations - Create new reconciliation
    else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/reconciliations"))
    {
        // Create XML for new reconciliation
        var createXml = new XElement("create",
            new XElement("BANKACCTRECON",
                new XElement("FINANCIALENTITY", "BOA"),
                new XElement("STMTENDINGDATE", "04/01/2020"),
                new XElement("CUTOFFDATE", "01/01/2020"),
                new XElement("STMTENDINGBALANCE", "1842"),
                new XElement("MODE", "Automatch")
            )
        );

        // Send to Sage endpoint
        using var client = new HttpClient();
        var response = await client.PostAsync("https://api.sage.com/v1/reconciliations/create", 
            new StringContent(createXml.ToString()));

        context.Response.StatusCode = (int)response.StatusCode;
        if (response.IsSuccessStatusCode)
        {
            await context.Response.WriteAsync("Reconciliation created successfully");
        }
        else
        {
            await context.Response.WriteAsync("Error creating reconciliation");
        }
    }
    // DELETE /reconciliations/{id} - Delete reconciliation
    else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/reconciliations/") == true)
    {
        var reconId = context.Request.Path.Value.Split('/').Last();

        // Create delete XML
        var deleteXml = new XElement("delete",
            new XElement("object", "BANKACCTRECON"),
            new XElement("keys", reconId)
        );

        // Send to Sage endpoint
        using var client = new HttpClient();
        var response = await client.PostAsync($"https://api.sage.com/v1/reconciliations/{reconId}/delete", 
            new StringContent(deleteXml.ToString()));

        context.Response.StatusCode = (int)response.StatusCode;
        if (response.IsSuccessStatusCode)
        {
            await context.Response.WriteAsync("Reconciliation deleted successfully");
        }
        else
        {
            await context.Response.WriteAsync("Error deleting reconciliation");
        }
    }
});

app.Run();

public class CheckingAccount
{
    public string Id { get; set; }
    public string Description { get; set; }

    public CheckingAccount(string id, string description)
    {
        Id = id;
        Description = description;
    }
}

public class Reconciliation
{
    public int Id { get; set; }
    public string State { get; set; }
    public string FinancialEntity { get; set; }
    public DateTime StatementEndingDate { get; set; }
    public DateTime CutoffDate { get; set; }
    public decimal StatementEndingBalance { get; set; }
    public string Mode { get; set; }

    public Reconciliation(int id, string state, string financialEntity, 
        DateTime stmtEndDate, DateTime cutoffDate, decimal balance, string mode)
    {
        Id = id;
        State = state;
        FinancialEntity = financialEntity;
        StatementEndingDate = stmtEndDate;
        CutoffDate = cutoffDate;
        StatementEndingBalance = balance;
        Mode = mode;
    }
}
