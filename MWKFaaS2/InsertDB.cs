using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace MWKFaaS2
{
    public static class InsertDB
    {
        [FunctionName("InsertDB")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];
            string lastName = req.Query["lastName"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            lastName = lastName ?? data?.lastName;

            if (name == "" || lastName == "")
                return  new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            else
            {
                // Get the connection string from app settings and use it to create a connection.
                var str = Environment.GetEnvironmentVariable("sqldb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var text = "INSERT INTO dbo.Person2 " +
                            "VALUES ('" + name + "', '" + lastName + "')";



                    using (SqlCommand cmd = new SqlCommand(text, conn))
                    {
                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");
                    }
                }

                //This is not for displaying to the user. Final product must not have it
                return name != null
                    ? (ActionResult)new OkObjectResult($"Successful insertion")
                    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }
            /*// Get the connection string from app settings and use it to create a connection.
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var text = "INSERT INTO dbo.Person2 " +
                        "VALUES ('" + name + "', '" + lastName + "')";



                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    // Execute the command and log the # rows affected.
                    var rows = await cmd.ExecuteNonQueryAsync();
                    log.LogInformation($"{rows} rows were updated");
                }
            }

            //This is not for displaying to the user. Final product must not have it
            return name != null
                ? (ActionResult)new OkObjectResult($"Successful insertion")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");*/
        }
    }
}