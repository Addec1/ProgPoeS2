using System;

namespace CMCS.Prototype.Models
{
   
    public class DocumentVm
    {
        public Guid DocumentId { get; set; }
        public Guid ClaimId { get; set; }

        // Backing fields so we can expose both FileName and OriginalName
        private string _fileName = string.Empty;
        public string FileName { get => _fileName; set => _fileName = value ?? string.Empty; }
        public string OriginalName { get => _fileName; set => _fileName = value ?? string.Empty; }

        public string ContentType { get; set; } = "application/octet-stream";

        // Backing field so we can expose both SizeKb and SizeKB
        private long _sizeKb;
        /// <summary> Size in KB (lowercase b variant expected by some views) </summary>
        public long SizeKb { get => _sizeKb; set => _sizeKb = value; }
        /// <summary> Alias for SizeKb (uppercase B variant) </summary>
        public long SizeKB { get => _sizeKb; set => _sizeKb = value; }

        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
    }
}