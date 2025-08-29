namespace Fanex.Affiliate.Link.Services.Interface
{
    using Fanex.Affiliate.Link.Models;

    public interface IUnderMaintenanceService
    {
        UnderMaintenanceTime GetUnderMaintenanceTime();

        bool IsUnderMaintenance();
    }
}
