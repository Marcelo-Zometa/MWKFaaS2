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
    public static class ReadDB
    {
        [FunctionName("ReadDB")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];
            string returnName = "";
            string returnLastName = "";
            int i = 0;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            // Get the connection string from app settings and use it to create a connection.
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                conn.Open();
                var text = "SELECT * FROM dbo.Person2 ORDER BY newid();";//WHERE Name = '" + name +"';";



                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            returnName = (reader["Name"].ToString());
                            returnLastName = (reader["LastName"].ToString());
                            //new OkObjectResult($"Hello, {returnName} {returnLastName}");
                        }
                    }
                }
            }

            return returnName != null || returnLastName != null
                ? (ActionResult)new OkObjectResult($"Janathan called him Rusty but his real name is: {returnName} {returnLastName}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
