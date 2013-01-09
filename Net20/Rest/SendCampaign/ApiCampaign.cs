namespace dotMailer.SendCampaign
{
    internal class ApiCampaign
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Subject
        {
            get;
            set;
        }

        public string FromName
        {
            get;
            set;
        }

        public string HtmlContent
        {
            get;
            set;
        }

        public string PlainTextContent
        {
            get;
            set;
        }
    }    
}
