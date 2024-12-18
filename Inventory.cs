using System;
using System.Text.Json;
using System.Xml.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class InventoryTotalDetails
{
    private static readonly HttpClient client = new HttpClient();

    // Get metadata about inventory total details including available fields and relationships
    public static async Task<string> GetInventoryDetailDefinition()
    {
        var lookupXml = new XElement("lookup",
            new XElement("object", "INVENTORYTOTALDETAIL")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/details/lookup",
            new StringContent(lookupXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error looking up inventory details: {response.StatusCode}");
    }

    // Query inventory total details filtering for items marked as scrap/spoilage or damaged
    public static async Task<string> QueryInventoryTotalDetails()
    {
        var queryXml = new XElement("query",
            new XElement("object", "INVENTORYTOTALDETAIL"),
            new XElement("select",
                new XElement("field", "ITEMID"),
                new XElement("field", "TOTALNAME")
            ),
            new XElement("filter",
                new XElement("or",
                    new XElement("equalto",
                        new XElement("field", "TOTALNAME"),
                        new XElement("value", "SCRAP OR SPOILAGE")
                    ),
                    new XElement("equalto",
                        new XElement("field", "TOTALNAME"),
                        new XElement("value", "DAMAGED")
                    )
                )
            )
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/details/query",
            new StringContent(queryXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error querying inventory details: {response.StatusCode}");
    }

    // Retrieve a specific inventory total detail record by its ID
    public static async Task<string> GetInventoryTotalDetail(string recordId)
    {
        var readXml = new XElement("read",
            new XElement("object", "INVENTORYTOTALDETAIL"),
            new XElement("keys", recordId),
            new XElement("fields", "*")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/details/read",
            new StringContent(readXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error reading inventory detail: {response.StatusCode}");
    }
}
public class AvailableInventory
{
    private static readonly HttpClient client = new HttpClient();

    // Get metadata about available inventory including available fields and relationships
    public static async Task<string> GetAvailableInventoryDefinition()
    {
        var lookupXml = new XElement("lookup",
            new XElement("object", "AVAILABLEINVENTORY")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/available/lookup",
            new StringContent(lookupXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error looking up available inventory: {response.StatusCode}");
    }

    // Query available inventory for a specific item ID, returning warehouse location and quantity details
    public static async Task<string> QueryAvailableInventory(string itemId = "vit-multi-033")
    {
        var queryXml = new XElement("query",
            new XElement("object", "AVAILABLEINVENTORY"),
            new XElement("select",
                new XElement("field", "ITEMID"),
                new XElement("field", "WAREHOUSEID"),
                new XElement("field", "BINKEY"),
                new XElement("field", "LOTNO"),
                new XElement("field", "EXPIRATIONDATE"),
                new XElement("field", "QUANTITYLEFT")
            ),
            new XElement("filter",
                new XElement("equalto",
                    new XElement("field", "ITEMID"),
                    new XElement("value", itemId)
                )
            )
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/available/query",
            new StringContent(queryXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error querying available inventory: {response.StatusCode}");
    }

    // Retrieve a specific available inventory record by its ID
    public static async Task<string> GetAvailableInventory(string recordId)
    {
        var readXml = new XElement("read",
            new XElement("object", "AVAILABLEINVENTORY"),
            new XElement("keys", recordId),
            new XElement("fields", "*")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/available/read",
            new StringContent(readXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error reading available inventory: {response.StatusCode}");
    }
}
public class InventoryTransactions
{
    private static readonly HttpClient client = new HttpClient();

    // List fields and relationships for inventory transaction object
    public static async Task<string> GetInventoryTransactionDefinition()
    {
        var lookupXml = new XElement("lookup",
            new XElement("object", "INVDOCUMENT")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transactions/lookup",
            new StringContent(lookupXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error looking up inventory transaction definition: {response.StatusCode}");
    }

    // Query and list inventory transactions
    public static async Task<string> QueryInventoryTransactions()
    {
        var queryXml = new XElement("query",
            new XElement("object", "INVDOCUMENT"),
            new XElement("select",
                new XElement("field", "RECORDNO"),
                new XElement("field", "STATE"), 
                new XElement("field", "TOTAL")
            ),
            new XElement("docparid", "Beginning Balance")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transactions/query",
            new StringContent(queryXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error querying inventory transactions: {response.StatusCode}");
    }

    // Get specific inventory transaction
    public static async Task<string> GetInventoryTransaction(string recordId)
    {
        var readXml = new XElement("read",
            new XElement("object", "INVDOCUMENT"),
            new XElement("keys", recordId),
            new XElement("fields", "*"),
            new XElement("docparid", "Beginning Balance")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transactions/read",
            new StringContent(readXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error reading inventory transaction: {response.StatusCode}");
    }

    // Delete inventory transaction
    public static async Task<string> DeleteInventoryTransaction(string recordId)
    {
        var deleteXml = new XElement("delete",
            new XElement("object", "INVDOCUMENT"),
            new XElement("keys", recordId)
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transactions/delete",
            new StringContent(deleteXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error deleting inventory transaction: {response.StatusCode}");
    }

    // List fields and relationships for inventory transaction line object
    public static async Task<string> GetInventoryTransactionLineDefinition()
    {
        var lookupXml = new XElement("lookup",
            new XElement("object", "INVDOCUMENTENTRY")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transaction-lines/lookup",
            new StringContent(lookupXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error looking up inventory transaction line definition: {response.StatusCode}");
    }

    // Query and list inventory transaction lines
    public static async Task<string> QueryInventoryTransactionLines()
    {
        var queryXml = new XElement("query",
            new XElement("object", "INVDOCUMENTENTRY"),
            new XElement("select",
                new XElement("field", "DOCHDRNO"),
                new XElement("field", "LINE_NO"),
                new XElement("field", "ITEMID"),
                new XElement("field", "PRICE")
            ),
            new XElement("docparid", "Beginning Balance")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transaction-lines/query",
            new StringContent(queryXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error querying inventory transaction lines: {response.StatusCode}");
    }

    // Get specific inventory transaction line
    public static async Task<string> GetInventoryTransactionLine(string recordId)
    {
        var readXml = new XElement("read",
            new XElement("object", "INVDOCUMENTENTRY"),
            new XElement("keys", recordId),
            new XElement("fields", "*"),
            new XElement("docparid", "Beginning Balance")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transaction-lines/read",
            new StringContent(readXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error reading inventory transaction line: {response.StatusCode}");
    }

    // List fields and relationships for inventory transaction subtotal object
    public static async Task<string> GetInventoryTransactionSubtotalDefinition()
    {
        var lookupXml = new XElement("lookup",
            new XElement("object", "INVDOCUMENTSUBTOTALS")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transaction-subtotals/lookup",
            new StringContent(lookupXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error looking up inventory transaction subtotal definition: {response.StatusCode}");
    }

    // Query and list inventory transaction subtotals
    public static async Task<string> QueryInventoryTransactionSubtotals()
    {
        var queryXml = new XElement("query",
            new XElement("object", "INVDOCUMENTSUBTOTALS"),
            new XElement("select",
                new XElement("field", "ITEMID"),
                new XElement("field", "LOCATION"),
                new XElement("field", "TOTAL"),
                new XElement("field", "DESCRIPTION")
            ),
            new XElement("docparid", "Beginning Balance")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transaction-subtotals/query",
            new StringContent(queryXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error querying inventory transaction subtotals: {response.StatusCode}");
    }

    // Get specific inventory transaction subtotal
    public static async Task<string> GetInventoryTransactionSubtotal(string recordId)
    {
        var readXml = new XElement("read",
            new XElement("object", "INVDOCUMENTSUBTOTALS"),
            new XElement("keys", recordId),
            new XElement("fields", "*"),
            new XElement("docparid", "Beginning Balance")
        );

        var response = await client.PostAsync("https://api.sage.com/v1/inventory/transaction-subtotals/read",
            new StringContent(readXml.ToString()));

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        throw new HttpRequestException($"Error reading inventory transaction subtotal: {response.StatusCode}");
    }
}



