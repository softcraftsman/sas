using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using sas.api;
using System.Text;

namespace sas.api
{
    public static class CreateTopLevelFolder
    {
        [FunctionName("CreateTopLevelFolder")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            // GET - Send Instructions back to calling client
            if (req.Method == HttpMethods.Get) {
                log.LogInformation("CreateTopLevelFolder GET method called.");
                var tlfp = JsonConvert.SerializeObject(new TopLevelFolderParameters());
                var help = $"Use a POST method with the body containing the properties of {nameof(TopLevelFolderParameters)}\n{tlfp}";
                return new OkObjectResult(help);
            }

            // POST - Method
            if (req.Method == HttpMethods.Post)
            {   
                //Extracting body object from the call and deserializing it.
                string body = "";
                
                using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
                {  
                    body = reader.ReadToEnd();
                }

                var bodyDeserialized = JsonConvert.DeserializeObject<TopLevelFolderParameters>(body);

                //Performs call's body validation
                if (bodyDeserialized is null) 
                {
                    log.LogInformation("Body is null.");
                    return new BadRequestObjectResult($"{nameof(TopLevelFolderParameters)} is missing.");
                }
                else
                {
                    if(bodyDeserialized.Container == null || bodyDeserialized.Folder == null || 
                        bodyDeserialized.FolderOwner == null || bodyDeserialized.FundCode == null ||
                        bodyDeserialized.StorageAcount == null)
                    {
                        log.LogInformation("Body is filled but one of more of its key properties is/are null. Request fails.");
                        return new BadRequestObjectResult($"{nameof(TopLevelFolderParameters)} is malformed.");
                    }
                }

                log.LogInformation("Parameters validation passed.");

                //Add Folder Owner to Container ACL as Execute
                log.LogInformation($"Adding '{bodyDeserialized.FolderOwner}' (Fold Owner) to the container '{bodyDeserialized.Container}' as 'Execute'...");
                
                var resultAddOwnerExecuteRootContainer = ADLSOperations.AddsFolderOwnerToContainerACLAsExecute(bodyDeserialized.FolderOwner, bodyDeserialized.Container, true);

                if(!resultAddOwnerExecuteRootContainer)
                {
                    log.LogInformation("Error on trying to add Folder Owner as Execute on the root Container. Error 500.");
                }
                else
                {
                    log.LogInformation("Done. Moving to the next step.");

                    //Create Folder
                    log.LogInformation($"Creating the folder '{bodyDeserialized.Folder}' within the container '{bodyDeserialized.Container}'...");
                    
                    var resultFolderCreation = ADLSOperations.CreatesNewFolder(bodyDeserialized.Folder);

                    if(!resultFolderCreation)
                    {
                        log.LogInformation("Error trying to create the new folder. Error 500.");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }
                    else
                    {
                        log.LogInformation("Done. Moving to the next step.");

                        //Assign RWX for Folder to Folder Owner
                        log.LogInformation($"Assigning RWX permission to Folder Owner ({bodyDeserialized.FolderOwner}) at folder's ({bodyDeserialized.Folder}) level...");

                        var resultPermissionsAssignment = ADLSOperations.AssignsRWXToFolderOwner(bodyDeserialized.FolderOwner, bodyDeserialized.Folder);

                        if(!resultPermissionsAssignment)
                        {
                            log.LogInformation("Error trying to assign the RWX permission to the folder. Error 500.");
                            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                        }
                        else
                        {
                            log.LogInformation("Done. Moving to the next step.");

                            //Add Fund Code to metadata
                            log.LogInformation("Saving FundCode into container's metadata...");

                            var resultSavingFundCode = ADLSOperations.SavesFundCodeIntoContainerMetadata(bodyDeserialized.FundCode, bodyDeserialized.Container);

                            if(!resultSavingFundCode)
                            {
                                log.LogInformation("Error trying to save de Fund Code into Container's metadata. Error 500.");
                                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                            }
                        }
                    }
                }

                log.LogInformation("Workflow completed successfully.");
                return new OkResult();
            }

            log.LogInformation("Internal Server Error - 500. Only GET and POST are acceptable.");
            return new BadRequestResult();
        }
    }

// {
//     "StorageAcount": "test",
//     "Container": "test",
//     "Folder": "test",
//     "FundCode": "test",
//     "FolderOwner": "test"
// }


}