using System;
using System.Text.Json;
using System.Xml.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ARBillJointPayee
{
    public string RecordNo { get; set; }
    public string ARBillKey { get; set; }
    public string JointPayeeName { get; set; }
    public string JointPayeePrintAs { get; set; }
}

public class ARBill
{
    public string RecordNo { get; set; }
    public string RecordId { get; set; }
    public string CustomerId { get; set; }
    public string State { get; set; }
    public DateTime WhenCreated { get; set; }
    public DateTime WhenPosted { get; set; }
    public string DocNumber { get; set; }
    public string Description { get; set; }
    public string TermName { get; set; }
    public DateTime RecPaymentDate { get; set; }
    public string SupDocId { get; set; }
    public DateTime WhenDue { get; set; }
    public string PaymentPriority { get; set; }
    public bool OnHold { get; set; }
    public string Currency { get; set; }
    public string BaseCurr { get; set; }
    public List<ARBillItem> BillItems { get; set; }
}

public class ARBillItem
{
    public string AccountNo { get; set; }
    public decimal TrxAmount { get; set; }
    public string EntryDescription { get; set; }
    public string LocationId { get; set; }
    public string DepartmentId { get; set; }
    public int LineNo { get; set; }
}

public class ARTerm
{
    public string RecordNo { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public string DueFrom { get; set; }
}

public class ARSummary
{
    public string RecordNo { get; set; }
    public string Status { get; set; }
    public string Open { get; set; }
}

public class ARAdjustment
{
    public string CustomerId { get; set; }
    public string RecordNo { get; set; }
    public string CustomerName { get; set; }
}

public class ARAdjustmentItem
{
    public string RecordNo { get; set; }
}

public class ARAccountLabel
{
    public string AccountLabel { get; set; }
    public string Description { get; set; }
    public string GLAccountNo { get; set; }
    public string OffsetGLAccountNo { get; set; }
    public string Status { get; set; }
}

public class ARPayment
{
    public string FinancialEntity { get; set; }
    public string PaymentMethod { get; set; }
    public string CustomerId { get; set; }
    public DateTime PaymentDate { get; set; }
    public List<ARPaymentDetail> PaymentDetails { get; set; }
    public string RecordNo { get; set; }
    public DateTime WhenCreated { get; set; }
}

public class ARPaymentDetail
{
    public string RecordKey { get; set; }
    public decimal PaymentAmount { get; set; }
    public string EntryKey { get; set; }
}

public class ARPaymentRequest
{
    public string RecordNo { get; set; }
    public decimal PaymentAmount { get; set; }
}

public class ProviderBankAccount
{
    public string BankAccountId { get; set; }
    public string ProviderId { get; set; }
    public string RecordNo { get; set; }
    public string Status { get; set; }
    public string CustomerId { get; set; }
}

public class ProviderPaymentMethod
{
    public string RecordNo { get; set; }
    public string ProviderId { get; set; }
    public string PaymentType { get; set; }
}

public class ProviderCustomer
{
    public string CustomerId { get; set; }
    public string ProviderId { get; set; }
    public string RecordNo { get; set; }
    public string Status { get; set; }
}

public class ARRecurringBill
{
    public string RecordNo { get; set; }
    public string CustomerName { get; set; }
    public List<ARRecurringBillLine> BillLines { get; set; }
}

public class ARRecurringBillLine
{
    public string RecordNo { get; set; }
    public decimal Amount { get; set; }
}

public class AccountsReceivableProgram
{
    private const string API_BASE_URL = "https://api.sage.com/v1";

    public static void ConfigureApp(WebApplication app)
    {
        app.Run(async (HttpContext context) =>
        {
        // GET /ar/bills/joint-payees/lookup - List all fields and relationships
        if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/bills/joint-payees/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARBILLJOINTPAYEE")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/joint-payees/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/bills/joint-payees - List joint payees
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/bills/joint-payees"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "ARBILLJOINTPAYEE"),
                new XElement("select",
                    new XElement("field", "RECORDNO"),
                    new XElement("field", "ARBILLKEY")
                ),
                new XElement("filter",
                    new XElement("like",
                        new XElement("field", "JOINTPAYEENAME"),
                        new XElement("value", "DW Drywall")
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/joint-payees",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/bills/joint-payees/{id} - Get joint payee by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/bills/joint-payees/") == true)
        {
            var payeeId = context.Request.Path.Value.Split('/').Last();

            var readXml = new XElement("read",
                new XElement("object", "ARBILLJOINTPAYEE"),
                new XElement("keys", payeeId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/joint-payees/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // POST /ar/bills/joint-payees - Create joint payee
        else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ar/bills/joint-payees"))
        {
            using var reader = new StreamReader(context.Request.Body);
            var requestData = await reader.ReadToEndAsync();
            var payeeData = JsonSerializer.Deserialize<ARBillJointPayee>(requestData);

            var createXml = new XElement("create",
                new XElement("ARBILLJOINTPAYEE",
                    new XElement("ARBILLKEY", "14"),
                    new XElement("JOINTPAYEENAME", "DW Drywall"),
                    new XElement("JOINTPAYEEPRINTAS", "Johnson Construction and DW Drywall")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/joint-payees/create",
                new StringContent(createXml.ToString()));

            await HandleResponse(context, response);
        }
        // PUT /ar/bills/joint-payees/{id} - Update joint payee
        else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ar/bills/joint-payees/") == true)
        {
            using var reader = new StreamReader(context.Request.Body);
            var requestData = await reader.ReadToEndAsync();
            var payeeData = JsonSerializer.Deserialize<ARBillJointPayee>(requestData);

            var updateXml = new XElement("update",
                new XElement("ARBILLJOINTPAYEE",
                    new XElement("RECORDNO", "5"),
                    new XElement("JOINTPAYEENAME", "Mac Air"),
                    new XElement("JOINTPAYEEPRINTAS", "Sunset Hardware AND Mac Air")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/joint-payees/update",
                new StringContent(updateXml.ToString()));

            await HandleResponse(context, response);
        }
        // DELETE /ar/bills/joint-payees/{id} - Delete joint payee
        else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ar/bills/joint-payees/") == true)
        {
            var payeeId = context.Request.Path.Value.Split('/').Last();

            var deleteXml = new XElement("delete",
                new XElement("object", "ARBILLJOINTPAYEE"),
                new XElement("keys", payeeId)
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/joint-payees/delete",
                new StringContent(deleteXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/bills/lookup - List all fields and relationships
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/bills/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARBILL")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/bills - List AR bills
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/bills"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "ARBILL"),
                new XElement("select",
                    new XElement("field", "RECORDNO"),
                    new XElement("field", "RECORDID"),
                    new XElement("field", "CUSTOMERID"),
                    new XElement("field", "STATE")
                ),
                new XElement("filter",
                    new XElement("and",
                        new XElement("equalto",
                            new XElement("field", "CUSTOMERID"),
                            new XElement("value", "Acme")
                        ),
                        new XElement("in",
                            new XElement("field", "STATE"),
                            new XElement("value", "Selected"),
                            new XElement("value", "Posted")
                        )
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/bills/{id} - Get AR bill by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/bills/") == true)
        {
            var billId = context.Request.Path.Value.Split('/').Last();

            var readXml = new XElement("read",
                new XElement("object", "ARBILL"),
                new XElement("keys", billId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // POST /ar/bills - Create AR bill
        else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ar/bills"))
        {
            using var reader = new StreamReader(context.Request.Body);
            var requestData = await reader.ReadToEndAsync();
            var billData = JsonSerializer.Deserialize<ARBill>(requestData);

            var createXml = new XElement("create",
                new XElement("ARBILL",
                    new XElement("WHENCREATED", "12/15/2016"),
                    new XElement("WHENPOSTED", "12/15/2016"),
                    new XElement("CUSTOMERID", "20000"),
                    new XElement("RECORDID", "1234"),
                    new XElement("DOCNUMBER", "Ref 5678"),
                    new XElement("DESCRIPTION", "My bill"),
                    new XElement("TERMNAME", "N30"),
                    new XElement("RECPAYMENTDATE", "12/23/2016"),
                    new XElement("SUPDOCID", "A1234"),
                    new XElement("WHENDUE", "12/25/2016"),
                    new XElement("PAYMENTPRIORITY", "high"),
                    new XElement("ONHOLD", "false"),
                    new XElement("CURRENCY", "USD"),
                    new XElement("BASECURR", "USD"),
                    new XElement("ARBILLITEMS",
                        new XElement("ARBILLITEM",
                            new XElement("ACCOUNTNO", "6220"),
                            new XElement("TRX_AMOUNT", "100.12"),
                            new XElement("ENTRYDESCRIPTION", "Line 1 of my bill"),
                            new XElement("LOCATIONID", "110"),
                            new XElement("DEPARTMENTID", "D300")
                        )
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/create",
                new StringContent(createXml.ToString()));

            await HandleResponse(context, response);
        }
        // PUT /ar/bills/{id} - Update AR bill
        else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ar/bills/") == true)
        {
            var billId = context.Request.Path.Value.Split('/').Last();

            using var reader = new StreamReader(context.Request.Body);
            var requestData = await reader.ReadToEndAsync();
            var billData = JsonSerializer.Deserialize<ARBill>(requestData);

            var updateXml = new XElement("update",
                new XElement("ARBILL",
                    new XElement("RECORDNO", "65"),
                    new XElement("DESCRIPTION", "Changing the description"),
                    new XElement("RECPAYMENTDATE", "12/25/2016"),
                    new XElement("WHENDUE", "12/31/2016"),
                    new XElement("ARBILLITEMS",
                        new XElement("ARBILLITEM",
                            new XElement("LINE_NO", "1"),
                            new XElement("ACCOUNTNO", "6225"),
                            new XElement("TRX_AMOUNT", "100.12"),
                            new XElement("ENTRYDESCRIPTION", "Line 1 of my bill"),
                            new XElement("LOCATIONID", "110"),
                            new XElement("DEPARTMENTID", "D300")
                        )
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/update",
                new StringContent(updateXml.ToString()));

            await HandleResponse(context, response);
        }
        // DELETE /ar/bills/{id} - Delete AR bill
        else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ar/bills/") == true)
        {
            var billId = context.Request.Path.Value.Split('/').Last();

            var deleteXml = new XElement("delete",
                new XElement("object", "ARBILL"),
                new XElement("keys", billId)
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/delete",
                new StringContent(deleteXml.ToString()));

            await HandleResponse(context, response);
        }
        // POST /ar/bills/{id}/recall - Recall AR bill
        else if (context.Request.Method == "POST" && context.Request.Path.Value?.EndsWith("/recall") == true)
        {
            var billId = context.Request.Path.Value.Split('/')[^2];

            var recallXml = new XElement("recallArBill",
                new XElement("recordno", billId)
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/recall",
                new StringContent(recallXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/bills/lines/lookup - List all fields and relationships for bill lines
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/bills/lines/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARBILLITEM")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/lines/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/bills/lines - List AR bill lines
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/bills/lines"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "ARBILLITEM"),
                new XElement("select",
                    new XElement("field", "RECORDNO"),
                    new XElement("field", "AMOUNT"),
                    new XElement("field", "GLACCOUNT.TITLE")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/lines",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/bills/lines/{id} - Get AR bill line by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/bills/lines/") == true)
        {
            var lineId = context.Request.Path.Value.Split('/').Last();

            var readXml = new XElement("read",
                new XElement("object", "ARBILLITEM"),
                new XElement("keys", lineId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/bills/lines/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/terms/lookup - List all fields and relationships
        if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/terms/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARTERM")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/terms/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/terms - List AR terms
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/terms"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "ARTERM"),
                new XElement("select",
                    new XElement("field", "RECORDNO"),
                    new XElement("field", "DESCRIPTION"),
                    new XElement("field", "DUEDATE"),
                    new XElement("field", "DUEFROM")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/terms",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/terms/{id} - Get AR term by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/terms/") == true)
        {
            var termId = context.Request.Path.Value.Split('/').Last();

            var readXml = new XElement("read",
                new XElement("object", "ARTERM"),
                new XElement("keys", termId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/terms/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/terms/name/{name} - Get AR term by name
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/terms/name/") == true)
        {
            var termName = context.Request.Path.Value.Split('/').Last();

            var readByNameXml = new XElement("readByName",
                new XElement("object", "ARTERM"),
                new XElement("keys", termName),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/terms/readbyname",
                new StringContent(readByNameXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/summaries/lookup - List all fields and relationships
        if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/summaries/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARBILLBATCH")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/summaries/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/summaries - List bill summaries
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/summaries"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "ARBILLBATCH"),
                new XElement("select",
                    new XElement("field", "RECORDNO")
                ),
                new XElement("filter",
                    new XElement("and",
                        new XElement("equalto",
                            new XElement("field", "STATUS"),
                            new XElement("value", "active")
                        ),
                        new XElement("equalto",
                            new XElement("field", "OPEN"),
                            new XElement("value", "Open")
                        )
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/summaries",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/summaries/{id} - Get bill summary by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/summaries/") == true)
        {
            var summaryId = context.Request.Path.Value.Split('/').Last();

            var readXml = new XElement("read",
                new XElement("object", "ARBILLBATCH"),
                new XElement("keys", summaryId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/summaries/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/adjustments/lookup - List all fields and relationships
        if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/adjustments/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARADJUSTMENT")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/adjustments/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/adjustments - List AR adjustments
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/adjustments"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "ARADJUSTMENT"),
                new XElement("select",
                    new XElement("field", "CUSTOMERID"),
                    new XElement("field", "RECORDNO")
                ),
                new XElement("filter",
                    new XElement("equalto",
                        new XElement("field", "CUSTOMERNAME"),
                        new XElement("value", "Acme Supply")
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/adjustments",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/adjustments/{id} - Get AR adjustment by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/adjustments/") == true)
        {
            var adjustmentId = context.Request.Path.Value.Split('/').Last();

            var readXml = new XElement("read",
                new XElement("object", "ARADJUSTMENT"),
                new XElement("keys", adjustmentId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/adjustments/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // DELETE /ar/adjustments/{id} - Delete AR adjustment
        else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ar/adjustments/") == true)
        {
            var adjustmentId = context.Request.Path.Value.Split('/').Last();

            var deleteXml = new XElement("delete",
                new XElement("object", "ARADJUSTMENT"),
                new XElement("keys", adjustmentId)
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/adjustments/delete",
                new StringContent(deleteXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/adjustments/lines - List AR adjustment lines
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/adjustments/lines"))
        {
            var readByQueryXml = new XElement("readByQuery",
                new XElement("object", "ARADJUSTMENTITEM"),
                new XElement("fields", "*"),
                new XElement("query"),
                new XElement("pagesize", "100")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/adjustments/lines",
                new StringContent(readByQueryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/adjustments/lines/{id} - Get AR adjustment line by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/adjustments/lines/") == true)
        {
            var lineId = context.Request.Path.Value.Split('/').Last();

            var readByQueryXml = new XElement("readByQuery",
                new XElement("object", "ARADJUSTMENTITEM"),
                new XElement("fields", "*"),
                new XElement("query"),
                new XElement("pagesize", "100")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/adjustments/lines/{lineId}",
                new StringContent(readByQueryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/labels/lookup - List all fields and relationships
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/labels/lookup"))
        {
            // Create lookup XML for AR account labels
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARACCOUNTLABEL")
            );

            // Send to Sage endpoint
            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/labels/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/labels - List all AR account labels
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/labels"))
        {
            // Create query XML to get active AR account labels
            var queryXml = new XElement("query",
                new XElement("object", "ARACCOUNTLABEL"),
                new XElement("select",
                    new XElement("field", "ACCOUNTLABEL")
                ),
                new XElement("filter",
                    new XElement("equalto",
                        new XElement("field", "STATUS"),
                        new XElement("value", "active")
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ap/labels",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/labels/{id} - Get AR account label by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/labels/") == true)
        {
            var labelId = context.Request.Path.Value.Split('/').Last();

            // Create read XML for specific AR account label
            var readXml = new XElement("read",
                new XElement("object", "ARACCOUNTLABEL"),
                new XElement("keys", labelId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/labels/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/labels/name/{name} - Get AR account label by name
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/labels/name/") == true)
        {
            var labelName = context.Request.Path.Value.Split('/').Last();

            // Create readByName XML
            var readByNameXml = new XElement("readByName",
                new XElement("object", "ARACCOUNTLABEL"),
                new XElement("keys", labelName),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/labels/readbyname",
                new StringContent(readByNameXml.ToString()));

            await HandleResponse(context, response);
        }
        // POST /ar/labels - Create new AR account label
        else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ar/labels"))
        {
            // Read request body
            using var reader = new StreamReader(context.Request.Body);
            var requestData = await reader.ReadToEndAsync();
            var labelData = JsonSerializer.Deserialize<ARAccountLabel>(requestData);

            // Create XML for new AR account label
            var createXml = new XElement("create",
                new XElement("ARACCOUNTLABEL",
                    new XElement("ACCOUNTLABEL", labelData.AccountLabel),
                    new XElement("DESCRIPTION", labelData.Description),
                    new XElement("GLACCOUNTNO", labelData.GLAccountNo),
                    new XElement("OFFSETGLACCOUNTNO", labelData.OffsetGLAccountNo),
                    new XElement("STATUS", labelData.Status)
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/labels/create",
                new StringContent(createXml.ToString()));

            await HandleResponse(context, response);
        }
        // PUT /ar/labels/{id} - Update AR account label
        else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ar/labels/") == true)
        {
            var labelId = context.Request.Path.Value.Split('/').Last();

            // Read request body
            using var reader = new StreamReader(context.Request.Body);
            var requestData = await reader.ReadToEndAsync();
            var labelData = JsonSerializer.Deserialize<ARAccountLabel>(requestData);

            // Create update XML
            var updateXml = new XElement("update",
                new XElement("ARACCOUNTLABEL",
                    new XElement("RECORDNO", labelId),
                    new XElement("DESCRIPTION", labelData.Description),
                    new XElement("GLACCOUNTNO", labelData.GLAccountNo),
                    new XElement("OFFSETGLACCOUNTNO", labelData.OffsetGLAccountNo),
                    new XElement("STATUS", labelData.Status)
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/labels/update",
                new StringContent(updateXml.ToString()));

            await HandleResponse(context, response);
        }
        // DELETE /ar/labels/{id} - Delete AR account label
        else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ar/labels/") == true)
        {
            var labelId = context.Request.Path.Value.Split('/').Last();

            // Create delete XML
            var deleteXml = new XElement("delete",
                new XElement("object", "ARACCOUNTLABEL"),
                new XElement("keys", labelId)
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/labels/delete",
                new StringContent(deleteXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/payments/lookup - List all fields and relationships
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/payments/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARPYMT")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/payments - List AR payments
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/payments"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "ARPYMT"),
                new XElement("select",
                    new XElement("field", "RECORDNO")
                ),
                new XElement("filter",
                    new XElement("isnull",
                        new XElement("field", "DESCRIPTION")
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/payments/{id} - Get AR payment by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/payments/") == true)
        {
            var paymentId = context.Request.Path.Value.Split('/').Last();

            var readXml = new XElement("read",
                new XElement("object", "ARPYMT"),
                new XElement("keys", paymentId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // POST /ar/payments - Create AR payment
        else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ar/payments"))
        {
            using var reader = new StreamReader(context.Request.Body);
            var requestData = await reader.ReadToEndAsync();
            var paymentData = JsonSerializer.Deserialize<ARPayment>(requestData);

            var createXml = new XElement("create",
                new XElement("ARPYMT",
                    new XElement("FINANCIALENTITY", "CHK-BA1145"),
                    new XElement("PAYMENTMETHOD", "Cash"),
                    new XElement("CUSTOMERID", "C0001"),
                    new XElement("PAYMENTDATE", "08/07/2017"),
                    new XElement("ARPYMTDETAILS",
                        new XElement("ARPYMTDETAIL",
                            new XElement("RECORDKEY", "90"),
                            new XElement("TRX_PAYMENTAMOUNT", "50.00")
                        )
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/create",
                new StringContent(createXml.ToString()));

            await HandleResponse(context, response);
        }
        // PUT /ar/payments/{id} - Update AR payment
        else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ar/payments/") == true)
        {
            var paymentId = context.Request.Path.Value.Split('/').Last();

            using var reader = new StreamReader(context.Request.Body);
            var requestData = await reader.ReadToEndAsync();
            var paymentData = JsonSerializer.Deserialize<ARPayment>(requestData);

            var updateXml = new XElement("update",
                new XElement("ARPYMT",
                    new XElement("RECORDNO", "103"),
                    new XElement("WHENCREATED", "08/09/2017"),
                    new XElement("ARPYMTDETAILS",
                        new XElement("ARPYMTDETAIL",
                            new XElement("RECORDKEY", "90"),
                            new XElement("ENTRYKEY", "375"),
                            new XElement("TRX_PAYMENTAMOUNT", "50.00")
                        )
                    )
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/update",
                new StringContent(updateXml.ToString()));

            await HandleResponse(context, response);
        }
        // DELETE /ar/payments/{id} - Delete AR payment
        else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ar/payments/") == true)
        {
            var paymentId = context.Request.Path.Value.Split('/').Last();

            var deleteXml = new XElement("delete",
                new XElement("object", "ARPYMT"),
                new XElement("keys", paymentId)
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/delete",
                new StringContent(deleteXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/payments/details/lookup - List all fields and relationships for payment details
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/payments/details/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARPYMTDETAIL")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/details/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/payments/details - List AR payment details
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/payments/details"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "ARPYMTDETAIL"),
                new XElement("select",
                    new XElement("field", "RECORDNO"),
                    new XElement("field", "PAYMENTAMOUNT")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/details",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/payments/details/{id} - Get AR payment detail by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/payments/details/") == true)
        {
            var detailId = context.Request.Path.Value.Split('/').Last();

            var readXml = new XElement("read",
                new XElement("object", "ARPYMTDETAIL"),
                new XElement("keys", detailId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/details/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/payments/requests/lookup - List all fields and relationships for payment requests
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/payments/requests/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "ARPAYMENTREQUEST")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/requests/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/payments/requests - List AR payment requests
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/payments/requests"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "ARPAYMENTREQUEST"),
                new XElement("select",
                    new XElement("field", "RECORDNO"),
                    new XElement("field", "PAYMENTAMOUNT")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/payments/requests",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/customer-bank-accounts/lookup - List all fields and relationships
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/customer-bank-accounts/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "CUSTOMERBANKACCOUNT")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/customer-bank-accounts/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/customer-bank-accounts - List customer bank accounts
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/customer-bank-accounts"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "CUSTOMERBANKACCOUNT"),
                new XElement("select",
                    new XElement("field", "BANKACCOUNTID"),
                    new XElement("field", "CUSTOMERID")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/customer-bank-accounts",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/customer-bank-accounts/{id} - Get customer bank account by ID
        else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ar/customer-bank-accounts/") == true)
        {
            var accountId = context.Request.Path.Value.Split('/').Last();

            var readXml = new XElement("read",
                new XElement("object", "CUSTOMERBANKACCOUNT"),
                new XElement("keys", accountId),
                new XElement("fields", "*")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/customer-bank-accounts/read",
                new StringContent(readXml.ToString()));

            await HandleResponse(context, response);
        }
        // POST /ar/customer-bank-accounts - Create customer bank account
        else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ar/customer-bank-accounts"))
        {
            var createXml = new XElement("create",
                new XElement("CUSTOMERBANKACCOUNT",
                    new XElement("CUSTOMERID", "CSI"),
                    new XElement("CUSTOMERID", "Acme")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/customer-bank-accounts",
                new StringContent(createXml.ToString()));

            await HandleResponse(context, response);
        }
        // PUT /ar/customer-bank-accounts/{id} - Update customer bank account
        else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ar/customer-bank-accounts/") == true)
        {
            var updateXml = new XElement("update",
                new XElement("CUSTOMERBANKACCOUNT",
                    new XElement("RECORDNO", "5"),
                    new XElement("STATUS", "inactive")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/customer-bank-accounts/update",
                new StringContent(updateXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/customer-payment-methods/lookup - List all fields and relationships
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/customer-payment-methods/lookup"))
        {
            var lookupXml = new XElement("lookup",
                new XElement("object", "CUSTOMERPAYMENTMETHOD")
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/customer-payment-methods/lookup",
                new StringContent(lookupXml.ToString()));

            await HandleResponse(context, response);
        }
        // GET /ar/customer-payment-methods - List payment customer payment methods
        else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ar/customer-payment-methods"))
        {
            var queryXml = new XElement("query",
                new XElement("object", "CUSTOMERPAYMENTMETHOD"),
                new XElement("select",
                    new XElement("field", "RECORDNO"),
                    new XElement("field", "CUSTOMERID"),
                    new XElement("field", "PAYMENTTYPE")
                )
            );

            using var client = new HttpClient();
            var response = await client.PostAsync($"{API_BASE_URL}/ar/customer-payment-methods",
                new StringContent(queryXml.ToString()));

            await HandleResponse(context, response);
        }
        private static async Task HandleResponse(HttpContext context, HttpResponseMessage response)
    {
        context.Response.StatusCode = (int)response.StatusCode;
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            await context.Response.WriteAsync(content);
        }
        else
        {
            await context.Response.WriteAsync($"Error processing request: {response.StatusCode}");
        }
    }
}