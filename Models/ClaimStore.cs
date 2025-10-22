using System;
using System.Collections.Generic;
using System.Linq;

namespace CMCS.Prototype.Models
{
    public static class ClaimStore
    {
        public const string CurrentLecturer = "Adrian Chetty";

        public static readonly List<ClaimVm> Claims =
        [
            new ClaimVm
            {
                ClaimId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000011"),
                LecturerName = CurrentLecturer,
                Month = DateTime.Now.Month, Year = DateTime.Now.Year,
                HourlyRate = 420, TotalHours = 8m, Status = ClaimStatus.Submitted,
                Entries =
                {
                    new ClaimEntryVm{ Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-4)), Hours = 3m, Description = "Lectures" },
                    new ClaimEntryVm{ Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), Hours = 5m, Description = "Consultations + prep" },
                }
            },
            new ClaimVm
            {
                ClaimId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000012"),
                LecturerName = "Ayesha Khan",
                Month = DateTime.Now.Month, Year = DateTime.Now.Year,
                HourlyRate = 400, TotalHours = 12m, Status = ClaimStatus.Verified,
                Entries =
                {
                    new ClaimEntryVm{ Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-6)), Hours = 6m, Description = "Marking" },
                    new ClaimEntryVm{ Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-5)), Hours = 6m, Description = "Moderation" },
                }
            },
            new ClaimVm
            {
                ClaimId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000013"),
                LecturerName = "Daniel Peters",
                Month = DateTime.Now.Month, Year = DateTime.Now.Year,
                HourlyRate = 380, TotalHours = 4m, Status = ClaimStatus.Submitted,
                Entries =
                {
                    new ClaimEntryVm{ Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)), Hours = 4m, Description = "Tutorial support" },
                }
            }
        ];

        public static ClaimVm? Find(Guid id) => Claims.FirstOrDefault(c => c.ClaimId == id);
    }
}
//