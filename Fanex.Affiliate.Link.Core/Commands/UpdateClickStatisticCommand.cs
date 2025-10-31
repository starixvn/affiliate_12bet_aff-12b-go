using Fanex.Affiliate.Link.Core.Commands;

namespace Fanex.Affiliate.Link.Commands;

public class UpdateClickStatisticCommand : BaseNonQueryCommand
{
    public required string ClickIDs { get; set; }

    public bool IsExcluded { get; set; }

    public override string GetSettingKey() => "Aff_ClickStatisticsPatterns_Upd";

    public override bool IsValid() => !string.IsNullOrWhiteSpace(ClickIDs);
}