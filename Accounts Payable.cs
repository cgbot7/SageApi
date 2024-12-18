using System;
using System.Text.Json;
using System.Xml.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class APBillJointPayee
{
    public string RecordNo { get; set; }
    public string APBillKey { get; set; }
    public string JointPayeeName { get; set; }
    public string JointPayeePrintAs { get; set; }
}

public class APBill
{
    public string RecordNo { get; set; }
    public string RecordId { get; set; }
    public string VendorId { get; set; }
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
    public List<APBillItem> BillItems { get; set; }
}

public class APBillItem
{
    public string AccountNo { get; set; }
    public decimal TrxAmount { get; set; }
    public string EntryDescription { get; set; }
    public string LocationId { get; set; }
    public string DepartmentId { get; set; }
    public int LineNo { get; set; }
}

public class APTerm
{
    public string RecordNo { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public string DueFrom { get; set; }
}

public class APSummary
{
    public string RecordNo { get; set; }
    public string Status { get; set; }
    public string Open { get; set; }
}

public class APAdjustment
{
    public string VendorId { get; set; }
    public string RecordNo { get; set; }
    public string VendorName { get; set; }
}

public class APAdjustmentItem
{
    public string RecordNo { get; set; }
}

public class APAccountLabel
{
    public string AccountLabel { get; set; }
    public string Description { get; set; }
    public string GLAccountNo { get; set; }
    public string OffsetGLAccountNo { get; set; }
    public string Status { get; set; }
}

public class APPayment
{
    public string FinancialEntity { get; set; }
    public string PaymentMethod { get; set; }
    public string VendorId { get; set; }
    public DateTime PaymentDate { get; set; }
    public List<APPaymentDetail> PaymentDetails { get; set; }
    public string RecordNo { get; set; }
    public DateTime WhenCreated { get; set; }
}

public class APPaymentDetail
{
    public string RecordKey { get; set; }
    public decimal PaymentAmount { get; set; }
    public string EntryKey { get; set; }
}

public class APPaymentRequest
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
    public string VendorId { get; set; }
}

public class ProviderPaymentMethod
{
    public string RecordNo { get; set; }
    public string ProviderId { get; set; }
    public string PaymentType { get; set; }
}

public class ProviderVendor
{
    public string VendorId { get; set; }
    public string ProviderId { get; set; }
    public string RecordNo { get; set; }
    public string Status { get; set; }
}

public class APRecurringBill
{
    public string RecordNo { get; set; }
    public string VendorName { get; set; }
    public List<APRecurringBillLine> BillLines { get; set; }
}

public class APRecurringBillLine
{
    public string RecordNo { get; set; }
    public decimal Amount { get; set; }
}

public class AccountsPayableProgram
{
    private const string API_BASE_URL = "https://api.sage.com/v1";

    public static void ConfigureApp(WebApplication app)
    {
        app.Run(async (HttpContext context) =>
        {
            // GET /ap/bills/joint-payees/lookup - List all fields and relationships
            if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/bills/joint-payees/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APBILLJOINTPAYEE")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/joint-payees/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/bills/joint-payees - List joint payees
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/bills/joint-payees"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APBILLJOINTPAYEE"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "APBILLKEY")
                    ),
                    new XElement("filter",
                        new XElement("like",
                            new XElement("field", "JOINTPAYEENAME"),
                            new XElement("value", "DW Drywall")
                        )
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/joint-payees",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/bills/joint-payees/{id} - Get joint payee by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/bills/joint-payees/") == true)
            {
                var payeeId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APBILLJOINTPAYEE"),
                    new XElement("keys", payeeId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/joint-payees/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // POST /ap/bills/joint-payees - Create joint payee
            else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ap/bills/joint-payees"))
            {
                using var reader = new StreamReader(context.Request.Body);
                var requestData = await reader.ReadToEndAsync();
                var payeeData = JsonSerializer.Deserialize<APBillJointPayee>(requestData);

                var createXml = new XElement("create",
                    new XElement("APBILLJOINTPAYEE",
                        new XElement("APBILLKEY", "14"),
                        new XElement("JOINTPAYEENAME", "DW Drywall"),
                        new XElement("JOINTPAYEEPRINTAS", "Johnson Construction and DW Drywall")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/joint-payees/create",
                    new StringContent(createXml.ToString()));

                await HandleResponse(context, response);
            }
            // PUT /ap/bills/joint-payees/{id} - Update joint payee
            else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ap/bills/joint-payees/") == true)
            {
                using var reader = new StreamReader(context.Request.Body);
                var requestData = await reader.ReadToEndAsync();
                var payeeData = JsonSerializer.Deserialize<APBillJointPayee>(requestData);

                var updateXml = new XElement("update",
                    new XElement("APBILLJOINTPAYEE",
                        new XElement("RECORDNO", "5"),
                        new XElement("JOINTPAYEENAME", "Mac Air"),
                        new XElement("JOINTPAYEEPRINTAS", "Sunset Hardware AND Mac Air")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/joint-payees/update",
                    new StringContent(updateXml.ToString()));

                await HandleResponse(context, response);
            }
            // DELETE /ap/bills/joint-payees/{id} - Delete joint payee
            else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ap/bills/joint-payees/") == true)
            {
                var payeeId = context.Request.Path.Value.Split('/').Last();

                var deleteXml = new XElement("delete",
                    new XElement("object", "APBILLJOINTPAYEE"),
                    new XElement("keys", payeeId)
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/joint-payees/delete",
                    new StringContent(deleteXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/bills/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/bills/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APBILL")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/bills - List AP bills
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/bills"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APBILL"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "RECORDID"),
                        new XElement("field", "VENDORID"),
                        new XElement("field", "STATE")
                    ),
                    new XElement("filter",
                        new XElement("and",
                            new XElement("equalto",
                                new XElement("field", "VENDORID"),
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
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/bills/{id} - Get AP bill by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/bills/") == true)
            {
                var billId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APBILL"),
                    new XElement("keys", billId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // POST /ap/bills - Create AP bill
            else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ap/bills"))
            {
                using var reader = new StreamReader(context.Request.Body);
                var requestData = await reader.ReadToEndAsync();
                var billData = JsonSerializer.Deserialize<APBill>(requestData);

                var createXml = new XElement("create",
                    new XElement("APBILL",
                        new XElement("WHENCREATED", "12/15/2016"),
                        new XElement("WHENPOSTED", "12/15/2016"),
                        new XElement("VENDORID", "20000"),
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
                        new XElement("APBILLITEMS",
                            new XElement("APBILLITEM",
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
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/create",
                    new StringContent(createXml.ToString()));

                await HandleResponse(context, response);
            }
            // PUT /ap/bills/{id} - Update AP bill
            else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ap/bills/") == true)
            {
                var billId = context.Request.Path.Value.Split('/').Last();

                using var reader = new StreamReader(context.Request.Body);
                var requestData = await reader.ReadToEndAsync();
                var billData = JsonSerializer.Deserialize<APBill>(requestData);

                var updateXml = new XElement("update",
                    new XElement("APBILL",
                        new XElement("RECORDNO", "65"),
                        new XElement("DESCRIPTION", "Changing the description"),
                        new XElement("RECPAYMENTDATE", "12/25/2016"),
                        new XElement("WHENDUE", "12/31/2016"),
                        new XElement("APBILLITEMS",
                            new XElement("APBILLITEM",
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
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/update",
                    new StringContent(updateXml.ToString()));

                await HandleResponse(context, response);
            }
            // DELETE /ap/bills/{id} - Delete AP bill
            else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ap/bills/") == true)
            {
                var billId = context.Request.Path.Value.Split('/').Last();

                var deleteXml = new XElement("delete",
                    new XElement("object", "APBILL"),
                    new XElement("keys", billId)
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/delete",
                    new StringContent(deleteXml.ToString()));

                await HandleResponse(context, response);
            }
            // POST /ap/bills/{id}/recall - Recall AP bill
            else if (context.Request.Method == "POST" && context.Request.Path.Value?.EndsWith("/recall") == true)
            {
                var billId = context.Request.Path.Value.Split('/')[^2];

                var recallXml = new XElement("recallApBill",
                    new XElement("recordno", billId)
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/recall",
                    new StringContent(recallXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/bills/lines/lookup - List all fields and relationships for bill lines
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/bills/lines/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APBILLITEM")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/lines/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/bills/lines - List AP bill lines
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/bills/lines"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APBILLITEM"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "AMOUNT"),
                        new XElement("field", "GLACCOUNT.TITLE")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/lines",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/bills/lines/{id} - Get AP bill line by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/bills/lines/") == true)
            {
                var lineId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APBILLITEM"),
                    new XElement("keys", lineId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/bills/lines/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/terms/lookup - List all fields and relationships
            if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/terms/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APTERM")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/terms/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/terms - List AP terms
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/terms"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APTERM"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "DESCRIPTION"),
                        new XElement("field", "DUEDATE"),
                        new XElement("field", "DUEFROM")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/terms",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/terms/{id} - Get AP term by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/terms/") == true)
            {
                var termId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APTERM"),
                    new XElement("keys", termId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/terms/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/terms/name/{name} - Get AP term by name
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/terms/name/") == true)
            {
                var termName = context.Request.Path.Value.Split('/').Last();

                var readByNameXml = new XElement("readByName",
                    new XElement("object", "APTERM"),
                    new XElement("keys", termName),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/terms/readbyname",
                    new StringContent(readByNameXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/summaries/lookup - List all fields and relationships
            if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/summaries/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APBILLBATCH")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/summaries/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/summaries - List bill summaries
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/summaries"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APBILLBATCH"),
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
                var response = await client.PostAsync($"{API_BASE_URL}/ap/summaries",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/summaries/{id} - Get bill summary by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/summaries/") == true)
            {
                var summaryId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APBILLBATCH"),
                    new XElement("keys", summaryId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/summaries/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/adjustments/lookup - List all fields and relationships
            if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/adjustments/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APADJUSTMENT")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/adjustments/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/adjustments - List AP adjustments
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/adjustments"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APADJUSTMENT"),
                    new XElement("select",
                        new XElement("field", "VENDORID"),
                        new XElement("field", "RECORDNO")
                    ),
                    new XElement("filter",
                        new XElement("equalto",
                            new XElement("field", "VENDORNAME"),
                            new XElement("value", "Acme Supply")
                        )
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/adjustments",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/adjustments/{id} - Get AP adjustment by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/adjustments/") == true)
            {
                var adjustmentId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APADJUSTMENT"),
                    new XElement("keys", adjustmentId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/adjustments/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // DELETE /ap/adjustments/{id} - Delete AP adjustment
            else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ap/adjustments/") == true)
            {
                var adjustmentId = context.Request.Path.Value.Split('/').Last();

                var deleteXml = new XElement("delete",
                    new XElement("object", "APADJUSTMENT"),
                    new XElement("keys", adjustmentId)
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/adjustments/delete",
                    new StringContent(deleteXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/adjustments/lines - List AP adjustment lines
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/adjustments/lines"))
            {
                var readByQueryXml = new XElement("readByQuery",
                    new XElement("object", "APADJUSTMENTITEM"),
                    new XElement("fields", "*"),
                    new XElement("query"),
                    new XElement("pagesize", "100")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/adjustments/lines",
                    new StringContent(readByQueryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/adjustments/lines/{id} - Get AP adjustment line by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/adjustments/lines/") == true)
            {
                var lineId = context.Request.Path.Value.Split('/').Last();

                var readByQueryXml = new XElement("readByQuery",
                    new XElement("object", "APADJUSTMENTITEM"),
                    new XElement("fields", "*"),
                    new XElement("query"),
                    new XElement("pagesize", "100")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/adjustments/lines/{lineId}",
                    new StringContent(readByQueryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/labels/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/labels/lookup"))
            {
                // Create lookup XML for AP account labels
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APACCOUNTLABEL")
                );

                // Send to Sage endpoint
                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/labels/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/labels - List all AP account labels
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/labels"))
            {
                // Create query XML to get active AP account labels
                var queryXml = new XElement("query",
                    new XElement("object", "APACCOUNTLABEL"),
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
            // GET /ap/labels/{id} - Get AP account label by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/labels/") == true)
            {
                var labelId = context.Request.Path.Value.Split('/').Last();

                // Create read XML for specific AP account label
                var readXml = new XElement("read",
                    new XElement("object", "APACCOUNTLABEL"),
                    new XElement("keys", labelId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/labels/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/labels/name/{name} - Get AP account label by name
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/labels/name/") == true)
            {
                var labelName = context.Request.Path.Value.Split('/').Last();

                // Create readByName XML
                var readByNameXml = new XElement("readByName",
                    new XElement("object", "APACCOUNTLABEL"),
                    new XElement("keys", labelName),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/labels/readbyname",
                    new StringContent(readByNameXml.ToString()));

                await HandleResponse(context, response);
            }
            // POST /ap/labels - Create new AP account label
            else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ap/labels"))
            {
                // Read request body
                using var reader = new StreamReader(context.Request.Body);
                var requestData = await reader.ReadToEndAsync();
                var labelData = JsonSerializer.Deserialize<APAccountLabel>(requestData);

                // Create XML for new AP account label
                var createXml = new XElement("create",
                    new XElement("APACCOUNTLABEL",
                        new XElement("ACCOUNTLABEL", labelData.AccountLabel),
                        new XElement("DESCRIPTION", labelData.Description),
                        new XElement("GLACCOUNTNO", labelData.GLAccountNo),
                        new XElement("OFFSETGLACCOUNTNO", labelData.OffsetGLAccountNo),
                        new XElement("STATUS", labelData.Status)
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/labels/create",
                    new StringContent(createXml.ToString()));

                await HandleResponse(context, response);
            }
            // PUT /ap/labels/{id} - Update AP account label
            else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ap/labels/") == true)
            {
                var labelId = context.Request.Path.Value.Split('/').Last();

                // Read request body
                using var reader = new StreamReader(context.Request.Body);
                var requestData = await reader.ReadToEndAsync();
                var labelData = JsonSerializer.Deserialize<APAccountLabel>(requestData);

                // Create update XML
                var updateXml = new XElement("update",
                    new XElement("APACCOUNTLABEL",
                        new XElement("RECORDNO", labelId),
                        new XElement("DESCRIPTION", labelData.Description),
                        new XElement("GLACCOUNTNO", labelData.GLAccountNo),
                        new XElement("OFFSETGLACCOUNTNO", labelData.OffsetGLAccountNo),
                        new XElement("STATUS", labelData.Status)
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/labels/update",
                    new StringContent(updateXml.ToString()));

                await HandleResponse(context, response);
            }
            // DELETE /ap/labels/{id} - Delete AP account label
            else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ap/labels/") == true)
            {
                var labelId = context.Request.Path.Value.Split('/').Last();

                // Create delete XML
                var deleteXml = new XElement("delete",
                    new XElement("object", "APACCOUNTLABEL"),
                    new XElement("keys", labelId)
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/labels/delete",
                    new StringContent(deleteXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/payments/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/payments/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APPYMT")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/payments - List AP payments
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/payments"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APPYMT"),
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
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/payments/{id} - Get AP payment by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/payments/") == true)
            {
                var paymentId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APPYMT"),
                    new XElement("keys", paymentId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // POST /ap/payments - Create AP payment
            else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ap/payments"))
            {
                using var reader = new StreamReader(context.Request.Body);
                var requestData = await reader.ReadToEndAsync();
                var paymentData = JsonSerializer.Deserialize<APPayment>(requestData);

                var createXml = new XElement("create",
                    new XElement("APPYMT",
                        new XElement("FINANCIALENTITY", "CHK-BA1145"),
                        new XElement("PAYMENTMETHOD", "Printed Check"),
                        new XElement("VENDORID", "V0001"),
                        new XElement("PAYMENTDATE", "08/07/2017"),
                        new XElement("APPYMTDETAILS",
                            new XElement("APPYMTDETAIL",
                                new XElement("RECORDKEY", "90"),
                                new XElement("TRX_PAYMENTAMOUNT", "50.00")
                            )
                        )
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/create",
                    new StringContent(createXml.ToString()));

                await HandleResponse(context, response);
            }
            // PUT /ap/payments/{id} - Update AP payment
            else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ap/payments/") == true)
            {
                var paymentId = context.Request.Path.Value.Split('/').Last();

                using var reader = new StreamReader(context.Request.Body);
                var requestData = await reader.ReadToEndAsync();
                var paymentData = JsonSerializer.Deserialize<APPayment>(requestData);

                var updateXml = new XElement("update",
                    new XElement("APPYMT",
                        new XElement("RECORDNO", "103"),
                        new XElement("WHENCREATED", "08/09/2017"),
                        new XElement("APPYMTDETAILS",
                            new XElement("APPYMTDETAIL",
                                new XElement("RECORDKEY", "90"),
                                new XElement("ENTRYKEY", "375"),
                                new XElement("TRX_PAYMENTAMOUNT", "50.00")
                            )
                        )
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/update",
                    new StringContent(updateXml.ToString()));

                await HandleResponse(context, response);
            }
            // DELETE /ap/payments/{id} - Delete AP payment
            else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ap/payments/") == true)
            {
                var paymentId = context.Request.Path.Value.Split('/').Last();

                var deleteXml = new XElement("delete",
                    new XElement("object", "APPYMT"),
                    new XElement("keys", paymentId)
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/delete",
                    new StringContent(deleteXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/payments/details/lookup - List all fields and relationships for payment details
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/payments/details/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APPYMTDETAIL")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/details/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/payments/details - List AP payment details
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/payments/details"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APPYMTDETAIL"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "PAYMENTAMOUNT")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/details",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/payments/details/{id} - Get AP payment detail by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/payments/details/") == true)
            {
                var detailId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APPYMTDETAIL"),
                    new XElement("keys", detailId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/details/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/payments/requests/lookup - List all fields and relationships for payment requests
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/payments/requests/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APPAYMENTREQUEST")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/requests/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/payments/requests - List AP payment requests
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/payments/requests"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APPAYMENTREQUEST"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "PAYMENTAMOUNT")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/payments/requests",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/provider-bank-accounts/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/provider-bank-accounts/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "PROVIDERBANKACCOUNT")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-bank-accounts/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/provider-bank-accounts - List provider bank accounts
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/provider-bank-accounts"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "PROVIDERBANKACCOUNT"),
                    new XElement("select",
                        new XElement("field", "BANKACCOUNTID"),
                        new XElement("field", "PROVIDERID")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-bank-accounts",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/provider-bank-accounts/{id} - Get provider bank account by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/provider-bank-accounts/") == true)
            {
                var accountId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "PROVIDERBANKACCOUNT"),
                    new XElement("keys", accountId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-bank-accounts/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // POST /ap/provider-bank-accounts - Create provider bank account
            else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ap/provider-bank-accounts"))
            {
                var createXml = new XElement("create",
                    new XElement("PROVIDERBANKACCOUNT",
                        new XElement("PROVIDERID", "CSI"),
                        new XElement("VENDORID", "Acme")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-bank-accounts",
                    new StringContent(createXml.ToString()));

                await HandleResponse(context, response);
            }
            // PUT /ap/provider-bank-accounts/{id} - Update provider bank account
            else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ap/provider-bank-accounts/") == true)
            {
                var updateXml = new XElement("update",
                    new XElement("PROVIDERBANKACCOUNT",
                        new XElement("RECORDNO", "5"),
                        new XElement("STATUS", "inactive")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-bank-accounts/update",
                    new StringContent(updateXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/provider-payment-methods/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/provider-payment-methods/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "PROVIDERPAYMENTMETHOD")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-payment-methods/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/provider-payment-methods - List payment provider payment methods
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/provider-payment-methods"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "PROVIDERPAYMENTMETHOD"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "PROVIDERID"),
                        new XElement("field", "PAYMENTTYPE")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-payment-methods",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/provider-payment-methods/{id} - Get payment provider payment method by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/provider-payment-methods/") == true)
            {
                var methodId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "PROVIDERPAYMENTMETHOD"),
                    new XElement("keys", methodId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-payment-methods/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/provider-vendors/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/provider-vendors/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "PROVIDERVENDOR")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-vendors/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/provider-vendors - List payment provider vendors
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/provider-vendors"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "PROVIDERVENDOR"),
                    new XElement("select",
                        new XElement("field", "VENDORID"),
                        new XElement("field", "PROVIDERID")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-vendors",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/provider-vendors/{id} - Get payment provider vendor by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/provider-vendors/") == true)
            {
                var vendorId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "PROVIDERVENDOR"),
                    new XElement("keys", vendorId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-vendors/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // POST /ap/provider-vendors - Create provider vendor
            else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ap/provider-vendors"))
            {
                var createXml = new XElement("create",
                    new XElement("PROVIDERVENDOR",
                        new XElement("PROVIDERID", "CSI"),
                        new XElement("VENDORID", "Acme"),
                        new XElement("STATUS", "active")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-vendors",
                    new StringContent(createXml.ToString()));

                await HandleResponse(context, response);
            }
            // PUT /ap/provider-vendors/{id} - Update provider vendor
            else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ap/provider-vendors/") == true)
            {
                var updateXml = new XElement("update",
                    new XElement("PROVIDERVENDOR",
                        new XElement("RECORDNO", "5"),
                        new XElement("STATUS", "inactive")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/provider-vendors/update",
                    new StringContent(updateXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/recurring-bills/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/recurring-bills/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APRECURBILL")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/recurring-bills/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/recurring-bills - List recurring bills
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/recurring-bills"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APRECURBILL"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "VENDORNAME")
                    ),
                    new XElement("filter",
                        new XElement("like",
                            new XElement("field", "VENDORNAME"),
                            new XElement("value", "B%")
                        )
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/recurring-bills",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/recurring-bills/{id} - Get recurring bill by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/recurring-bills/") == true)
            {
                var billId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APRECURBILL"),
                    new XElement("keys", billId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/recurring-bills/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/recurring-bill-lines/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/recurring-bill-lines/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "APRECURBILLENTRY")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/recurring-bill-lines/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/recurring-bill-lines - List recurring bill lines
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/recurring-bill-lines"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "APRECURBILLENTRY"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "AMOUNT")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/recurring-bill-lines",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/recurring-bill-lines/{id} - Get recurring bill line by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/recurring-bill-lines/") == true)
            {
                var lineId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "APRECURBILLENTRY"),
                    new XElement("keys", lineId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/recurring-bill-lines/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendor-groups/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/vendor-groups/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "VENDORGROUP")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendor-groups/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendor-groups - List vendor groups
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/vendor-groups"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "VENDORGROUP"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "NAME")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendor-groups",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendor-groups/{id} - Get vendor group by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/vendor-groups/") == true)
            {
                var groupId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "VENDORGROUP"),
                    new XElement("keys", groupId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendor-groups/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendor-groups/name/{name} - Get vendor group by name
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/vendor-groups/name/") == true)
            {
                var groupName = context.Request.Path.Value.Split('/').Last();

                var readByNameXml = new XElement("readByName",
                    new XElement("object", "VENDORGROUP"),
                    new XElement("keys", groupName),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendor-groups/read-by-name",
                    new StringContent(readByNameXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendor-types/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/vendor-types/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "VENDTYPE")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendor-types/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendor-types - List vendor types
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/vendor-types"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "VENDTYPE"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "NAME")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendor-types",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendors/lookup - List all fields and relationships
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/vendors/lookup"))
            {
                var lookupXml = new XElement("lookup",
                    new XElement("object", "VENDOR")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendors/lookup",
                    new StringContent(lookupXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendors - List vendors
            else if (context.Request.Method == "GET" && context.Request.Path.StartsWithSegments("/ap/vendors"))
            {
                var queryXml = new XElement("query",
                    new XElement("object", "VENDOR"),
                    new XElement("select",
                        new XElement("field", "RECORDNO"),
                        new XElement("field", "PAYTOCONTACT.CONTACTNAME"),
                        new XElement("field", "TOTALDUE")
                    ),
                    new XElement("filter",
                        new XElement("greaterthan",
                            new XElement("field", "TOTALDUE"),
                            new XElement("value", "100")
                        )
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendors",
                    new StringContent(queryXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendors/{id} - Get vendor by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/vendors/") == true)
            {
                var vendorId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "VENDOR"),
                    new XElement("keys", vendorId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendors/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendors/name/{name} - Get vendor by name
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/vendors/name/") == true)
            {
                var vendorName = context.Request.Path.Value.Split('/').Last();

                var readByNameXml = new XElement("readByName",
                    new XElement("object", "VENDOR"),
                    new XElement("keys", vendorName),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendors/read-by-name",
                    new StringContent(readByNameXml.ToString()));

                await HandleResponse(context, response);
            }
            // POST /ap/vendors - Create vendor
            else if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/ap/vendors"))
            {
                var createXml = new XElement("create",
                    new XElement("VENDOR",
                        new XElement("VENDORID", "V1234"),
                        new XElement("NAME", "SaaS Company Inc"),
                        new XElement("DISPLAYCONTACT",
                            new XElement("PRINTAS", "SaaS Company Inc"),
                            new XElement("COMPANYNAME", "SaaS Co"),
                            new XElement("TAXABLE", "true"),
                            new XElement("TAXGROUP", "CA"),
                            new XElement("TAXSOLUTIONID", "US Sales Tax - SYS"),
                            new XElement("TAXSCHEDULE", "US Sold Goods Standard"),
                            new XElement("PREFIX", "Mr"),
                            new XElement("FIRSTNAME", "Bill"),
                            new XElement("LASTNAME", "Smith"),
                            new XElement("INITIAL", "G"),
                            new XElement("PHONE1", "12"),
                            new XElement("PHONE2", "34"),
                            new XElement("CELLPHONE", "56"),
                            new XElement("PAGER", "78"),
                            new XElement("FAX", "90"),
                            new XElement("EMAIL1", "noreply@intacct.com"),
                            new XElement("EMAIL2", ""),
                            new XElement("URL1", ""),
                            new XElement("URL2", ""),
                            new XElement("MAILADDRESS",
                                new XElement("ADDRESS1", "300 Park Ave"),
                                new XElement("ADDRESS2", "Ste 1400"),
                                new XElement("CITY", "San Jose"),
                                new XElement("STATE", "CA"),
                                new XElement("ZIP", "95110"),
                                new XElement("COUNTRY", "United States")
                            )
                        ),
                        new XElement("ONETIME", "false"),
                        new XElement("STATUS", "active"),
                        new XElement("HIDEDISPLAYCONTACT", "false"),
                        new XElement("VENDTYPE", "SaaS"),
                        new XElement("PARENTID", "V5678"),
                        new XElement("GLGROUP", "Group"),
                        new XElement("TAXID", "12-3456789"),
                        new XElement("NAME1099", "SAAS CO INC"),
                        new XElement("FORM1099TYPE", "MISC"),
                        new XElement("FORM1099BOX", "3"),
                        new XElement("SUPDOCID", "A1234"),
                        new XElement("APACCOUNT", "2000"),
                        new XElement("CREDITLIMIT", "1234.56"),
                        new XElement("ONHOLD", "false"),
                        new XElement("DONOTCUTCHECK", "false"),
                        new XElement("COMMENTS", "my comment"),
                        new XElement("CURRENCY", "USD"),
                        new XElement("CONTACTINFO",
                            new XElement("CONTACTNAME", "primary")
                        ),
                        new XElement("PAYTO",
                            new XElement("CONTACTNAME", "pay to")
                        ),
                        new XElement("RETURNTO",
                            new XElement("CONTACTNAME", "return to")
                        ),
                        new XElement("PAYMETHODKEY", "Printed Check"),
                        new XElement("MERGEPAYMENTREQ", "true"),
                        new XElement("PAYMENTNOTIFY", "true"),
                        new XElement("BILLINGTYPE", "openitem"),
                        new XElement("PAYMENTPRIORITY", "Normal"),
                        new XElement("TERMNAME", "N30"),
                        new XElement("DISPLAYTERMDISCOUNT", "false"),
                        new XElement("ACHENABLED", "true"),
                        new XElement("ACHBANKROUTINGNUMBER", "123456789"),
                        new XElement("ACHACCOUNTNUMBER", "1111222233334444"),
                        new XElement("ACHACCOUNTTYPE", "Checking Account"),
                        new XElement("ACHREMITTANCETYPE", "CTX"),
                        new XElement("VENDORACCOUNTNO", "9999999"),
                        new XElement("DISPLAYACCTNOCHECK", "false"),
                        new XElement("OBJECTRESTRICTION", "Restricted"),
                        new XElement("RESTRICTEDLOCATIONS", "100#~#200"),
                        new XElement("RESTRICTEDDEPARTMENTS", "D100#~#D200"),
                        new XElement("CUSTOMFIELD1", "Hello")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendors",
                    new StringContent(createXml.ToString()));

                await HandleResponse(context, response);
            }
            // PUT /ap/vendors/{id} - Update vendor
            else if (context.Request.Method == "PUT" && context.Request.Path.Value?.StartsWith("/ap/vendors/") == true)
            {
                var updateXml = new XElement("update",
                    new XElement("VENDOR",
                        new XElement("RECORDNO", "59"),
                        new XElement("TERMNAME", "NET15")
                    )
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendors/update",
                    new StringContent(updateXml.ToString()));

                await HandleResponse(context, response);
            }
            // DELETE /ap/vendors/{id} - Delete vendor
            else if (context.Request.Method == "DELETE" && context.Request.Path.Value?.StartsWith("/ap/vendors/") == true)
            {
                var vendorId = context.Request.Path.Value.Split('/').Last();

                var deleteXml = new XElement("delete",
                    new XElement("object", "VENDOR"),
                    new XElement("keys", vendorId)
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendors/delete",
                    new StringContent(deleteXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendor-types/{id} - Get vendor type by ID
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/vendor-types/") == true)
            {
                var typeId = context.Request.Path.Value.Split('/').Last();

                var readXml = new XElement("read",
                    new XElement("object", "VENDTYPE"),
                    new XElement("keys", typeId),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendor-types/read",
                    new StringContent(readXml.ToString()));

                await HandleResponse(context, response);
            }
            // GET /ap/vendor-types/name/{name} - Get vendor type by name
            else if (context.Request.Method == "GET" && context.Request.Path.Value?.StartsWith("/ap/vendor-types/name/") == true)
            {
                var typeName = context.Request.Path.Value.Split('/').Last();

                var readByNameXml = new XElement("readByName",
                    new XElement("object", "VENDTYPE"),
                    new XElement("keys", typeName),
                    new XElement("fields", "*")
                );

                using var client = new HttpClient();
                var response = await client.PostAsync($"{API_BASE_URL}/ap/vendor-types/read-by-name",
                    new StringContent(readByNameXml.ToString()));

                await HandleResponse(context, response);
            }

        });
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
