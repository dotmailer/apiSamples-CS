using System;

namespace dotMailer.SendCampaignWithAttachment
{
    internal class ApiCampaignSend
    {
        public Guid Id
        {
            get;
            set;
        }

        public int CampaignId
        {
            get;
            set;
        }

        public int[] AddressBookIds
        {
            get;
            set;
        }

        public ApiCampaignSendStatuses Status { get; set; }
    }
}
