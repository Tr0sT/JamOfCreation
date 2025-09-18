#nullable enable
using System;
using System.Collections.ObjectModel;

namespace NuclearBand.Game
{
    public interface IUpgradesManager
    {
        event Action? OnPossibleUpgradesChanged;
        ReadOnlyCollection<UpgradeData> PossibleUpgrades { get; }
        
        void Unlock(UpgradeData upgradeData);
    }
}