namespace PP.Signicat.WebApi.Models.SignicatHandlers
{
    public class SigningInfo
    {
        public string customerOrg { get; set; }
        public bool notifyMe { get; set; }
        public string senderMail { get; set; }
        public int signMethod { get; set; }
        public int authMetod { get; set; }

        public int daysToLive { get; set; }
        public int LCID { get; set; }
        public string signingMetodText { get; set; }
        public bool SendSMS { get; set; }
        public string SMSText { get; set; }
        public bool isInk { get; set; }

    }
}