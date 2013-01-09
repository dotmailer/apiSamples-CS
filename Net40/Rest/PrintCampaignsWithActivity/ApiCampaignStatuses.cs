using System;

namespace dotMailer.PrintCampaignsWithActivity
{
    internal enum ApiCampaignStatuses
    {
        NotAvailableInThisVersion = -1,
        Unsent = 0,
        Sending = 2,
        Sent = 3,
        Paused = 4,
        Cancelled = 5,
        RequiresSystemApproval = 6,
        RequiresSMSApproval = 7,
        RequiresWorkflowApproval = 8,
        Triggered = Int32.MaxValue
    }
}
