using Microsoft.WindowsAzure.Storage.Table;

namespace PP.Signicat.WebApi.Models.CRMCredentials
{
    public class CustomerCredentialsModel : TableEntity
    {
        public CustomerCredentialsModel(string category, string orgname)
        : base(category, orgname) { }

        public CustomerCredentialsModel() { }
        public string orgname { get; set; }
        public string orgurl { get; set; }
        public string discoveryurl { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string domain { get; set; }
    }
}