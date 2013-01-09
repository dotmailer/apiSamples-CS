namespace dotMailer.SendCampaign
{
    internal enum ApiCampaignSendStatuses
    {
        NotAvailableInThisVersion = -1,

        NotSent = 0,

        Scheduled = 1,

        Sending = 2,

        Sent = 3,

        Cancelled = 4
    }
}
