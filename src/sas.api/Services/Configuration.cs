using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace sas.api
{
    public static class SasConfiguration
    {
        internal static ConfigurationResult GetConfiguration()
        {
            var dlsa = Environment.GetEnvironmentVariable("DATALAKE_STORAGE_ACCOUNTS");
            var accounts = dlsa.Replace(',', ';').Split(';');
            Array.ForEach(accounts, x => x.Trim());
            accounts = accounts.Where(x => x.Length > 0).ToArray();

            // Config
            var result = new ConfigurationResult()
            {
                TenantId = Environment.GetEnvironmentVariable("TENANT_ID"),
                ClientId = Environment.GetEnvironmentVariable("APP_REGISTRATION_CLIENT_ID"),
                StorageAccounts = accounts
            };
            return result;
        }

        internal class ConfigurationResult
        {
            public string[] StorageAccounts { get; set; }
            public string TenantId { get; set; }
            public string ClientId { get; set; }
        }
    }
}
