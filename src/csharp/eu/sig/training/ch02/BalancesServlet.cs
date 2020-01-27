using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace eu.sig.training.ch02
{
    public class BalancesServlet
    {
        // Visual Studio Code Analysis is right in pointing out that the following method has
        // security flaws. However, that's beside the point for this example.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        // tag::doGet[]
        //handling HTTP request
        //DB access
        //business logic
        //formatting data (JSON)
        public void DoGet(HttpRequest req, HttpResponse resp)
        {
            resp.ContentType = "application/json";
            var dataAdapter = GetAdapter(req);
            var dataTable = GetAccountData(dataAdapter);
            try
            {
                CheckAccount(dataTable,resp);
            }
            catch (SqlException e)
            {
                Console.WriteLine($"SQL exception: {e.Message}");
            }
        }
        // end::doGet[]
        private SqlDataAdapter GetAdapter(HttpRequest req)
        {
            string command = "SELECT account, balance " +
                "FROM ACCTS WHERE id=" + req.Params[
                    ConfigurationManager.AppSettings["request.parametername"]];
            return new SqlDataAdapter(command,
                ConfigurationManager.AppSettings["handler.serverstring"]);
        }
        private DataTable GetAccountData(SqlDataAdapter dataAdapter)
        {
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet, "ACCTS");
            DataTable dataTable = dataSet.Tables[0];
            return dataTable;
        }
         private void CheckAccount(DataTable dataTable, HttpResponse resp)
         {
                float totalBalance = 0;
                int rowNum = 0;
                resp.Write("{\"balances\":[");
                while (dataTable.Rows.GetEnumerator().MoveNext())
                {
                    rowNum++;
                    DataRow results = (DataRow)dataTable.Rows.GetEnumerator().Current;
                    CheckAccountLength(results, totalBalance, resp);
                    WriteResponse(rowNum, dataTable, resp);
                }
                resp.Write($"\"total\":{totalBalance}}}\n");
         }
             // Assuming result is 9-digit bank account number,
            // validate with 11-test:
         private void CheckAccountLength(DataRow results, float totalBalance, HttpResponse resp)
         {
             int sum = 0;
                    for (int i = 0; i < ((string)results["account"]).Length; i++)
                    {
                        sum = sum + (9 - i) *
                            (int)Char.GetNumericValue(((string)results["account"])[i]);
                    }
                    if (sum % 11 == 0)
                    {
                        totalBalance += (float)results["balance"];
                        resp.Write($"{{\"{results["account"]}\":{results["balance"]}}}");
                    }
         }
        private void WriteResponse(int rowNum, DataTable dataTable, HttpResponse resp)
        {
            if (rowNum == dataTable.Rows.Count)
            {
                resp.Write("],\n");
            }
            else
            {
                resp.Write(",");
            }
        }
    } 
}
