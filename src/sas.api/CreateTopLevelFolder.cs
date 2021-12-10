using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace sas.api.CreateTopLevelFolder
{
    public static class CreateTopLevelFolder
    {
        [FunctionName("CreateTopLevelFolder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // Requies Storage Blob Data Contributor or Owner

            // Pass the authorization bearer token through form the from end to the back end API services

            /*
                [] Add Folder Owner to Container ACL as Execute
                [] Create Folder
                [] Assign RWX for Folder to Folder Owner
                [] Add Fund Code to metadata
            */



            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            var user = StaticWebAppsAuth.Parse(req);

            name = user.Identity.Name;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Login or pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        public static async Task ManageDirectoryACLs(DataLakeFileSystemClient fileSystemClient)
        {
            var directoryClient = fileSystemClient.GetDirectoryClient("");
            var directoryAccessControl = await directoryClient.GetAccessControlAsync();
            foreach (var item in directoryAccessControl.Value.AccessControlList)
            {
                Console.WriteLine(item.ToString());
            }

            var accessControlList = PathAccessControlExtensions.ParseAccessControlList("user::rwx,group::r-x,other::rw-");

            directoryClient.SetAccessControlList(accessControlList);

        }
    }


}
