using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace PP.Signicat.CredentialManager.Models
{
    public class TransactionModel : TableEntity
    {
        //subscription = Unique organization name
        public TransactionModel(string subscription, string monthyear)
        : base(subscription, monthyear) { }

        public TransactionModel() { }

        [Display(Name = "Period")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please add Period. mm-yyyy Eks: 11-2016")]
        public string period { get; set; } //RowKey

        [Display(Name = "Subscription")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please add subscription name")]
        public string subscription { get; set; } //PartitionKey
        public string category { get; set; }

        [Display(Name = "Total Signings")]
        public int countertotal { get; set; }

        [Display(Name = "Unique Users count")]
        public int counteruniqueusers { get; set; }

        [Display(Name = "BankID")]
        public int counterbankid { get; set; }

        [Display(Name = "Social")]
        public int countersocial { get; set; }

        [Display(Name = "Email/SMS")]
        public int counternpid { get; set; }

        [Display(Name = "Handwritten")]
        public int counterhandwritten { get; set; }

    }
}