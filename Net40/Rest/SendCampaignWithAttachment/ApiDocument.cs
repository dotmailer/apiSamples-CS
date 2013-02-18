using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotMailer.SendCampaignWithAttachment
{
    internal class ApiDocument
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FileName { get; set; }

        public int FileSize { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }
    }
}
