namespace CyberNexus.Api.Models;

public class Achievement
{
    public int Id { get; set; }

    /// <summary>Stable machine key the server checks conditions against, e.g. "first_lesson".</summary>
    public string Key { get; set; } = string.Empty;

    public string TitleEn { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;

    public string Icon { get; set; } = "military_tech";
    public int XpReward { get; set; } = 100;

    /// <summary>Level the user must reach before this can unlock.</summary>
    public int RequiredLevel { get; set; } = 1;

    public List<UserAchievement> Owners { get; set; } = new();
}
