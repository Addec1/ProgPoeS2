using System;
using System.Collections.Generic;
using System.Linq;

namespace CMCS.Prototype.Models
{
    public class ClaimVm
    {
        public Guid ClaimId { get; set; }

        // Lecturer submitting this claim
        public string LecturerName { get; set; } = string.Empty;

        // Period
        public int Month { get; set; }
        public int Year { get; set; }

        // When the claim was created (used in views / sorting)
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Optional notes / comments (some views reference this)
        public string? Notes { get; set; }

        // Rate & totals
        public decimal HourlyRate { get; set; }
        public decimal TotalHours { get; set; }

        // Computed
        public decimal TotalAmount => Math.Round(TotalHours * HourlyRate, 2);

        // Workflow
        public ClaimStatus Status { get; set; } = ClaimStatus.Draft;

        // Line items
        public List<ClaimEntryVm> Entries { get; set; } = new();
    }
}

// Reference: Microsoft Learn (2023) C# Properties and expression-bodied members.
// Available at: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties
// Assisted in defining calculated properties such as TotalAmount.
