#nullable enable
using System.Linq;
using ObservableCollections;

namespace NuclearBand.Game
{
    internal sealed class UpgradesPanelViewModel : IUpgradesPanelViewModel
    {
        private readonly IUpgradesManager _upgradesManager;
        private readonly ICurrenciesManager _currenciesManager;
        private readonly ObservableList<IUpgradeViewModel> _availableUpgrades = new ObservableList<IUpgradeViewModel>();

        public UpgradesPanelViewModel(IUpgradesManager upgradesManager, ICurrenciesManager currenciesManager)
        {
            _upgradesManager = upgradesManager;
            _currenciesManager = currenciesManager;
            _upgradesManager.OnPossibleUpgradesChanged += UpdateAvailableUpgrades;
            UpdateAvailableUpgrades();
        }

        public void Dispose()
        {
            _upgradesManager.OnPossibleUpgradesChanged -= UpdateAvailableUpgrades;
        }

        private void UpdateAvailableUpgrades()
        {
            _availableUpgrades.Clear();
            var upgrades = _upgradesManager.PossibleUpgrades
                .Select(p => new UpgradeViewModel(p, _upgradesManager, _currenciesManager));
            foreach (var upgrade in upgrades)
            {
                _availableUpgrades.Add(upgrade);
            }
        }

        public INotifyCollectionChangedSynchronizedViewList<IUpgradeViewModel> AvailableUpgrades => _availableUpgrades.ToNotifyCollectionChangedSlim();
    }
}