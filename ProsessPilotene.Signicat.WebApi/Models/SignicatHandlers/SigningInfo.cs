namespace PP.Signicat.WebApi.Models.SignicatHandlers
{
    public class SigningInfo
    {
        public string notifyMe { get; set; }
        public string senderMail { get; set; }
        public string authMetod { get; set; }

        public int daysToLive { get; set; }
        public string signingMetodText { get; set; }
    }
}