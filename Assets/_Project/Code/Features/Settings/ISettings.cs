#nullable enable
namespace NuclearBand.Game
{
    public interface ISettings
    {
        UpgradeSettings UpgradeSettings { get; }
        EnergySettings EnergySettings { get; }
        PromptSettings PromptSettings { get; }
    }
}