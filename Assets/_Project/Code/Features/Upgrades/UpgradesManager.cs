#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NuclearBand.Game
{
    public sealed class UpgradesManager : IUpgradesManager
    {
        private readonly ISettings _settings;
        private readonly ISaver _saver;
        private readonly ICurrenciesManager _currenciesManager;
        private readonly IActionsManager _actionsManager;

        private readonly List<UpgradeData> _possibleUpgrades = new();

        public UpgradesManager(ISettings settings, ISaver saver, 
            ICurrenciesManager currenciesManager,
            IActionsManager actionsManager)
        {
            _settings = settings;
            _saver = saver;
            _currenciesManager = currenciesManager;
            _actionsManager = actionsManager;
            foreach (var saveUpgradeEntryId in _saver.Save.UpgradeEntryIds)
            {
                Unlock(_settings.UpgradeSettings.Upgrades.First(u => u.Id == saveUpgradeEntryId));
            }

            UpdatePossibleUpgrades();
        }

        public event Action? OnPossibleUpgradesChanged;
        public ReadOnlyCollection<UpgradeData> PossibleUpgrades => _possibleUpgrades.AsReadOnly();
        public void Unlock(UpgradeData upgradeData)
        {
            _saver.AddUpgradeEntryId(upgradeData.Id);
            
            foreach (var unlock in upgradeData.Unlocks)
            {
                if (unlock.CurrencyBonus != null)
                {
                    _currenciesManager.AddCurrencyBonus(unlock.CurrencyBonus, false);
                }

                if (unlock.ActionTypeUnlock != null)
                {
                    _actionsManager.Unlock(unlock.ActionTypeUnlock.Value);
                }

                if (unlock.CurrencyTypeUnlock != null)
                {
                    _currenciesManager.Unlock(unlock.CurrencyTypeUnlock.Value);
                }

                if (unlock.UnlockType == UnlockType.AdvancedPrompting)
                {
                    _actionsManager.UnlockAdvancedPrompting();
                }
            }

            UpdatePossibleUpgrades();
        }

        private void UpdatePossibleUpgrades()
        {
            _possibleUpgrades.Clear();
            _possibleUpgrades.AddRange(
                _settings.UpgradeSettings.Upgrades.Where(u =>
                    !_saver.Save.UpgradeEntryIds.Contains(u.Id) && 
                    u.Restrictions.All(r => _saver.Save.UpgradeEntryIds.Contains(r))
                )
            );
            OnPossibleUpgradesChanged?.Invoke();
        }
    }
}