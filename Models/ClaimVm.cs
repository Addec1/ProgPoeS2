using System;
using System.Collections.Generic;

namespace CMCS.Prototype.Models
{
    public sealed class ClaimVm
    {
        public Guid ClaimId { get; set; }
        public string LecturerName { get; set; } = "";
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalAmount => Math.Round(TotalHours * HourlyRate, 2);
        public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
        public List<ClaimEntryVm> Entries { get; set; } = new();
    }

  

    public sealed class DocumentVm
    {
        public string FileName { get; set; } = "";
        public int SizeKb { get; set; }
        public DateTime UploadedOn { get; set; }
    }

    public sealed class ApprovalVm
    {
        public Guid ClaimId { get; set; }
        public string LecturerName { get; set; } = "";
        public ApprovalStage Stage { get; set; }
        public ClaimStatus Status { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string? Comments { get; set; }
    }
}