using System;

namespace CMCS.Prototype.Models
{
    
    public class ApprovalVm
    {
        public Guid ApprovalId { get; set; } = Guid.NewGuid();
        public Guid ClaimId { get; set; }

        // Expected by views
        public string LecturerName { get; set; } = string.Empty;

        // Workflow state after this action (e.g., Submitted, Verified, Approved, Rejected)
        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;

        // Human label for who performed/at what step (e.g., "Coordinator", "Manager")
        public string Stage { get; set; } = string.Empty;

        // Free-form action name if a view binds it (e.g., "Verified", "Approved")
        public string Action { get; set; } = string.Empty;

        // Optional: who performed the action
        public string? ActorName { get; set; }

        // Notes/reason captured at the time of action
        public string? Notes { get; set; }

        public DateTime PerformedOn { get; set; } = DateTime.UtcNow;
    }
}