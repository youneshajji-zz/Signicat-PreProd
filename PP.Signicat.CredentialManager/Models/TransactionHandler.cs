using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace PP.Signicat.CredentialManager.Models
{
    public class TransactionHandler
    {
        internal TransactionModel UpsertTransactions(TransactionModel transaction)
        {
            CloudTable table = ConnectToAzureTableStorage();

            try
            {
                TransactionModel entity = new TransactionModel(transaction.subscription, transaction.period)
                {
                    customer = transaction.customer,
                    countertotal = transaction.countertotal,
                    counteruniqueusers = transaction.counteruniqueusers,
                    counterbankid = transaction.counterbankid,
                    countersocial = transaction.countersocial,
                    counternpid = transaction.countersocial
                };

                TableOperation insertOperation = TableOperation.InsertOrMerge(entity);
                table.Execute(insertOperation);

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        internal TransactionModel GetTransaction(string subscription, string period)
        {
            try
            {
                CloudTable table = ConnectToAzureTableStorage();
                TableOperation retrieveOperation = TableOperation.Retrieve<TransactionModel>(subscription, period);
                TableResult query = table.Execute(retrieveOperation);
                var transaction = (TransactionModel)query.Result;
                transaction.period = period;
                transaction.subscription = subscription;

                return transaction;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<TransactionModel> GetAllTransactions(string subscription)
        {
            try
            {
                if (subscription == null)
                    return new List<TransactionModel>();

                CloudTable table = ConnectToAzureTableStorage();
                var transactionsInventoryQuery = (from entry in table.CreateQuery<TransactionModel>()
                                                  where entry.PartitionKey == subscription
                                                  select entry);
                var transactionsInventory = transactionsInventoryQuery.ToList();
                var decryptedInventory = new List<TransactionModel>();

                foreach (var item in transactionsInventory)
                {
                    var transaction = new TransactionModel();
                    transaction.PartitionKey = item.PartitionKey;
                    transaction.RowKey = item.RowKey;
                    transaction.ETag = item.ETag;
                    transaction.period = item.RowKey;
                    transaction.subscription = item.PartitionKey;
                    transaction.customer = item.customer;
                    transaction.countertotal = item.countertotal;
                    transaction.counteruniqueusers = item.counteruniqueusers;
                    transaction.counterbankid = item.counterbankid;
                    transaction.countersocial = item.countersocial;
                    transaction.counternpid = item.counternpid;

                    decryptedInventory.Add(transaction);
                }

                return decryptedInventory;

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

                CloudTable table = client.GetTableReference("CustomerTransactions");
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