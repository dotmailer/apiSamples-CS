using System;

namespace dotMailer.SendCampaign
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
