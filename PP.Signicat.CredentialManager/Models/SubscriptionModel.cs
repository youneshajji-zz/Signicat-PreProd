using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.Table;

namespace PP.Signicat.CredentialManager.Models
{
    public class SubscriptionModel : TableEntity
    {
        //subscription = Unique organization name
        public SubscriptionModel(string customer, string subscription)
        : base(customer, subscription) { }

        public SubscriptionModel() { }

        [Display(Name = "Category")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please add category name")]
        public string category { get; set; } //PartitionKey

        [Display(Name = "Subscription")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please add Subscription name")]
        public string subscription { get; set; } //RowKey

        [Display(Name = "Customer")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please add customer name")]
        public string customer { get; set; } //Customer

        [Display(Name = "Partner")]
        public string partner { get; set; }

        [Display(Name = "Organization Url")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please add Organization Url")]
        public string orgurl { get; set; }

        [Display(Name = "Discovery Url")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please add Discovery Url")]
        public string discoveryurl { get; set; }

        [Display(Name = "Username")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please add Username")]
        public string username { get; set; }

        [Display(Name = "Password")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please add Password")]
        public string password { get; set; }

        [Display(Name = "Domain")]
        public string domain { get; set; }

        //[DataType(DataType.Date)]
        [Display(Name = "Service started")]
        public DateTime? servicestarted { get; set; }

        //[DataType(DataType.Date)]
        [Display(Name = "Service ended")]
        public DateTime? serviceended { get; set; }

        [Display(Name = "Status")]
        public Status status { get; set; }
        public int subscriptionstatus { get; set; }

        [Display(Name = "User count")]
        public int usercount { get; set; }

        [Display(Name = "Volume License")]
        public int volumelicense { get; set; }
        public List<TransactionModel> transactionList { get; set; }
    }

    public enum Status
    {
        [Display(Name = "Active")]
        Active = 1,

        [Display(Name = "Suspended")]
        Suspended = 2,

        [Display(Name = "Inactive")]
        Inactive = 3
    }
}