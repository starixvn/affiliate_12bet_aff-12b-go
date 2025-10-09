namespace Fanex.Affiliate.Link.Services.Interface
{
    using Fanex.Affiliate.Link.Commands;
    using Fanex.Affiliate.Link.Core.Models;
    using Fanex.Affiliate.Link.DTOs;
    using Fanex.Affiliate.Link.Models;

    /// <summary>
    /// Tracking system statistics.
    /// </summary>
    public interface IClickTrackingEventService
    {
        /// <summary>
        /// Insert statistics into database.
        /// </summary>
        /// <param name="command">Input command.</param>
        /// <returns>Success or fail.</returns>
        Task<string> InsertStatistic(TrackingSystemCommand command);

        Task<ValidateResult> ValidateLinkFromDatabase(ClickTrackingEvent clickTrackingEvent);

        Task<MasterInfoValidation> GetMasterDataForValidateClicked();

        Task<MasterInfoValidation> GetMasterInfoFromDatabase();

        Task<long> GetClickId();

        Task<long> SyncMaxClickId(bool isSync);

        Task PublishMessage(ClickTrackingEvent clickTrackingEvent);
    }
}
