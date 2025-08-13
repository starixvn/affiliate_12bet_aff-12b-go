namespace Fanex.Affiliate.Link.Models
{
    public class UnderMaintenanceTime
    {
        public bool IsUnderMaintenance { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}