namespace dotMailer.ImportContacts
{
    internal enum ApiContactImportStatuses
    {
        NotAvailableInThisVersion = -1,
        Finished = 1,
        NotFinished = 2,
        RejectedByWatchdog = 3,
        InvalidFileFormat = 4,
        ExceedsAllowedContactLimit = 7,
        Unknown = 5,
        Failed = 6
    }
}
