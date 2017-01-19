using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace PP.Signicat.CredentialManager.Models
{
    public class SubscriptionHandler
    {
        private const string PassPhrase = "SignicatPassPhrase";
        public SubscriptionModel CreateSubscription(SubscriptionModel credentials)
        {
            CloudTable table = ConnectToAzureTableStorage();

            try
            {
                var password = "";
                if (credentials.password.Length < 30)
                    password = new StringCipher().Encrypt(credentials.password, PassPhrase);
                else
                    password = credentials.password;

                var status = Status.Active;
                if (credentials.status != 0)
                    status = credentials.status;

                SubscriptionModel entity = new SubscriptionModel(credentials.customer, credentials.subscription)
                {
                    orgurl = credentials.orgurl,
                    discoveryurl = credentials.discoveryurl,
                    username = credentials.username,
                    password = password,
                    domain = credentials.domain,
                    partner = credentials.partner,
                    servicestarted = credentials.servicestarted,
                    serviceended = credentials.serviceended,
                    subscriptionstatus = (int)status,
                    usercount = credentials.usercount,
                    volumelicense = credentials.volumelicense
                };

                TableOperation insertOperation = TableOperation.Insert(entity);
                table.Execute(insertOperation);

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public SubscriptionModel UpdateSubscription(SubscriptionModel credentials)
        {
            CloudTable table = ConnectToAzureTableStorage();

            try
            {
                var password = "";
                if (credentials.password.Length < 30)
                    password = new StringCipher().Encrypt(credentials.password, PassPhrase);
                else
                    password = credentials.password;

                var status = Status.Active;
                if (credentials.status != 0)
                    status = credentials.status;

                SubscriptionModel entity = new SubscriptionModel(credentials.customer, credentials.subscription)
                {
                    ETag = "*",
                    orgurl = credentials.orgurl,
                    discoveryurl = credentials.discoveryurl,
                    username = credentials.username,
                    password = password,
                    domain = credentials.domain,
                    partner = credentials.partner,
                    servicestarted = credentials.servicestarted,
                    serviceended = credentials.serviceended,
                    subscriptionstatus = (int)status,
                    usercount = credentials.usercount,
                    volumelicense = credentials.volumelicense
                };

                TableOperation insertOperation = TableOperation.Merge(entity);
                table.Execute(insertOperation);

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public SubscriptionModel GetSubscription(string customer, string subscription)
        {
            try
            {
                CloudTable table = ConnectToAzureTableStorage();
                TableOperation retrieveOperation = TableOperation.Retrieve<SubscriptionModel>(customer, subscription);
                TableResult query = table.Execute(retrieveOperation);

                if (query.Result == null)
                    return null;

                var custCred = (SubscriptionModel)query.Result;
                //var password = new StringCipher().Decrypt(custCred.password, PassPhrase);
                //custCred.password = password;
                custCred.status = (Status)custCred.subscriptionstatus;
                custCred.customer = custCred.PartitionKey;
                custCred.subscription = custCred.RowKey;

                return custCred;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<SubscriptionModel> GetAllSubscriptions()
        {
            try
            {
                CloudTable table = ConnectToAzureTableStorage();
                var CustomerCredentialsInventoryQuery = (from entry in table.CreateQuery<SubscriptionModel>()
                                                         select entry);
                var CustomerCredentialsInventory = CustomerCredentialsInventoryQuery.ToList();
                var decryptedInventory = new List<SubscriptionModel>();

                foreach (var item in CustomerCredentialsInventory)
                {
                    //var password = new StringCipher().Decrypt(item.password, PassPhrase);
                    //var transactionList = new TransactionHandler().GetAllTransactions(item.RowKey);

                    var customerCred = new SubscriptionModel();
                    customerCred.PartitionKey = item.PartitionKey;
                    customerCred.RowKey = item.RowKey;
                    customerCred.ETag = item.ETag;
                    customerCred.password = item.password;
                    customerCred.discoveryurl = item.discoveryurl;
                    customerCred.domain = item.domain;
                    customerCred.subscription = item.RowKey;
                    customerCred.customer = item.PartitionKey;
                    customerCred.partner = item.partner;
                    customerCred.orgurl = item.orgurl;
                    customerCred.username = item.username;
                    customerCred.servicestarted = item.servicestarted;
                    customerCred.serviceended = item.serviceended;
                    customerCred.subscriptionstatus = item.subscriptionstatus;
                    customerCred.status = (Status)item.subscriptionstatus;
                    customerCred.usercount = item.usercount;
                    customerCred.volumelicense = item.volumelicense;
                    customerCred.Timestamp = item.Timestamp;

                    decryptedInventory.Add(customerCred);
                }

                return decryptedInventory;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool DeleteSubscription(string customer, string subscription)
        {
            try
            {
                CloudTable table = ConnectToAzureTableStorage();

                SubscriptionModel entity = new SubscriptionModel(customer, subscription) { ETag = "*" };

                TableOperation retrieveOperation = TableOperation.Delete(entity);
                TableResult query = table.Execute(retrieveOperation);
                return true;

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

                CloudTable table = client.GetTableReference("CustomerCRMCredentialsPreProd");
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