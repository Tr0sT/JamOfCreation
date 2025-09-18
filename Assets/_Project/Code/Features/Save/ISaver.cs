#nullable enable
namespace NuclearBand.Game
{
    public interface ISaver
    {
        Save Save { get; }
        
        void AddUpgradeEntryId(UpgradeDataId upgradeDataId);
    }
}