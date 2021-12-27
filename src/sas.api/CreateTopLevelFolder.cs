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
using Microsoft.Identity.Client;

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
                
                var resultAddOwnerExecuteRootContainer = ADLSOperations.AddsFolderOwnerToContainerACLAsExecute(bodyDeserialized.FolderOwner, bodyDeserialized.Container, true, bodyDeserialized.Container);

                if(!resultAddOwnerExecuteRootContainer)
                {
                    log.LogInformation("Error on trying to add Folder Owner as Execute on the root Container. Error 500.");
                }
                else
                {
                    log.LogInformation("Done. Moving to the next step.");

                    //Create Folder
                    log.LogInformation($"Creating the folder '{bodyDeserialized.Folder}' within the container '{bodyDeserialized.Container}'...");
                    
                    var resultFolderCreation = ADLSOperations.CreatesNewFolder(bodyDeserialized.Folder, bodyDeserialized.Container);

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

                        var resultPermissionsAssignment = ADLSOperations.AssignsRWXToFolderOwner(bodyDeserialized.FolderOwner, bodyDeserialized.Container, bodyDeserialized.Folder);

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

        public static async Task<IActionResult> AlternateRun([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
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
                var tlfp = GetTopLevelFolderParameters(req, out string error);
                if (error != null)
                {
                    log.LogInformation(error);
                    return new BadRequestObjectResult(error);
                }

                // Get object ID from UPN
                if (tlfp.FolderOwner.Contains('@'))
                {
                    var apiAccessToken = await GetApiToken(req, log, tlfp.FolderOwner);
                    tlfp.FolderOwner = await UserOperations.GetObjectIdFromUPN(apiAccessToken, tlfp.FolderOwner);
                }

                // Call each of the steps in order and error out if anytyhing fails
                var containerUri = new Uri($"https://{tlfp.StorageAcount}.dfs.core.windows.net/{tlfp.Container}");
                var accessToken = UserOperations.GetAccessTokenFromRequest(req);

                var adlOps = new ADLSOperations(containerUri, accessToken, log);
                if (adlOps.AddsFolderOwnerToContainerACLAsExecute(tlfp.FolderOwner, tlfp.Container, true, tlfp.Container, out error)
                    && adlOps.CreatesNewFolder(tlfp.Folder, tlfp.Container, out error)
                    && adlOps.AssignsRWXToFolderOwner(tlfp.FolderOwner, tlfp.Container, tlfp.Folder, out error)
                    && adlOps.SavesFundCodeIntoContainerMetadata(tlfp.FundCode, tlfp.Container, out error))
                {
                    log.LogInformation("success");
                }
                else
                {
                    log.LogError("Error: " + error);
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                log.LogInformation("Workflow completed successfully.");
                return new OkResult();
            }

            log.LogInformation("Internal Server Error - 500. Only GET and POST are acceptable.");
            return new BadRequestResult();
        }

        private static TopLevelFolderParameters GetTopLevelFolderParameters(HttpRequest req, out string error)
        {
            string body = string.Empty;
            error = null;

            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                body = reader.ReadToEnd();
            }

            var bodyDeserialized = JsonConvert.DeserializeObject<TopLevelFolderParameters>(body);

            //Performs call's body validation
            if (bodyDeserialized is null)
            {
                error = $"{nameof(TopLevelFolderParameters)} is missing.";
            }
            else
            {
                if (bodyDeserialized.Container == null || bodyDeserialized.Folder == null ||
                    bodyDeserialized.FolderOwner == null || bodyDeserialized.FundCode == null ||
                    bodyDeserialized.StorageAcount == null)
                {
                    error = $"{nameof(TopLevelFolderParameters)} is malformed.";
                }
            }

            return bodyDeserialized;
        }

        private static async Task<string> GetApiToken(HttpRequest req, ILogger log, string folderOwner)
        {
            var accessToken = UserOperations.GetAccessTokenFromRequest(req);
            var userAssertion = new UserAssertion(accessToken);
            var upn = UserOperations.GetObjectIdFromUPN(accessToken, folderOwner);
            ConfidentialClientApplicationOptions options = new()
            {
                TenantId = Environment.GetEnvironmentVariable("TENANT_ID"),
                ClientId = Environment.GetEnvironmentVariable("APP_REGISTRATION_CLIENT_ID"),
                ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET"),
            };
            var app = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(options)
                .Build();
            var scopes = new string[] {
                        "https://management.azure.com/user_impersonation",
                        "https://storage.azure.com/user_impersonation"};

            var authResult = await app.AcquireTokenOnBehalfOf(scopes, userAssertion)
                .ExecuteAsync();
            return authResult.AccessToken;
        }
    }
}