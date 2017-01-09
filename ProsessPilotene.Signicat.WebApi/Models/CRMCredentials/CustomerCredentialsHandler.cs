using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace PP.Signicat.WebApi.Models.CRMCredentials
{
    public class CustomerCredentialsHandler
    {
        private const string PassPhrase = "SignicatPassPhrase";

        internal CustomerCredentialsModel GetCredentials(string orgname)
        {
            try
            {
                CloudTable table = ConnectToAzureTableStorage();

                TableOperation retrieveOperation = TableOperation.Retrieve<CustomerCredentialsModel>("signicatcrm", orgname);

                TableResult query = table.Execute(retrieveOperation);

                if (query.Result == null)
                    return null;

                var custCred = (CustomerCredentialsModel)query.Result;
                var password = new StringCipher().Decrypt(custCred.password, PassPhrase);
                custCred.password = password;

                return custCred;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private CloudTable ConnectToAzureTableStorage()
        {
            string accountName = "ppsignicatresources";
            string accountKey = "LMbG3i6qcwDhDR01Z55U26aaxjkZ1NwoSkMHIG5RvVRuX9yYW51U9txcYDok6SzCufPp8Jisi4GuVuPrPJur3A==";

            try
            {
                StorageCredentials creds = new StorageCredentials(accountName, accountKey);
                CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);

                CloudTableClient client = account.CreateCloudTableClient();

                CloudTable table = client.GetTableReference("CustomerCRMCredentials");
                table.CreateIfNotExists();
                return table;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}