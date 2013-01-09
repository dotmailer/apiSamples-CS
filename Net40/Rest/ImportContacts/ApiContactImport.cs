using System;

namespace dotMailer.ImportContacts
{
    internal class ApiContactImport
    {
        public Guid Id { get; set; }

        public ApiContactImportStatuses Status { get; set; }
    }
}
