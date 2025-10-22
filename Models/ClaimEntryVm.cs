namespace CMCS.Prototype.Models
{
    public class ClaimEntryVm
    {
        public DateOnly Date { get; set; }
        public decimal Hours { get; set; }
        public string Description { get; set; } = "";
    }
}
