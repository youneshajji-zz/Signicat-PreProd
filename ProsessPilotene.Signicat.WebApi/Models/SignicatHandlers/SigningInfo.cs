namespace PP.Signicat.WebApi.Models.SignicatHandlers
{
    public class SigningInfo
    {
        public string customerOrg { get; set; }
        public int notifyMe { get; set; }
        public string senderMail { get; set; }
        public string signMethod { get; set; }
        public string authMetod { get; set; }

        public int daysToLive { get; set; }
        public int LCID { get; set; }
        public string signingMetodText { get; set; }
        public int SendSMS { get; set; }
        public string SMSText { get; set; }
        public bool isInk { get; set; }

    }
}