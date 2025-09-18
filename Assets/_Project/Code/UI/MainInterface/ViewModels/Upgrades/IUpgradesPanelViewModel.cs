#nullable enable
using System;
using ObservableCollections;

namespace NuclearBand.Game
{
    public interface IUpgradesPanelViewModel : IDisposable
    {
        INotifyCollectionChangedSynchronizedViewList<IUpgradeViewModel> AvailableUpgrades { get; }
    }
}