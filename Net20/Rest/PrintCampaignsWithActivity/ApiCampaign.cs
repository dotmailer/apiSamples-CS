namespace dotMailer.PrintCampaignsWithActivity
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

        public ApiCampaignFromAddress FromAddress
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

        public ApiCampaignReplyActions ReplyAction
        {
            get;
            set;
        }

        public string ReplyToAddress
        {
            get;
            set;
        }

        public bool IsSplitTest
        {
            get;
            set;
        }

        public ApiCampaignStatuses Status
        {
            get;
            set;
        }
    }    
}
