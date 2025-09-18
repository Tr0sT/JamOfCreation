#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MoreLinq;

namespace NuclearBand.Game
{
    internal sealed class UpgradeViewModel : IUpgradeViewModel
    {
        private readonly UpgradeData _upgradeData;
        private readonly IUpgradesManager _upgradesManager;
        private readonly ICurrenciesManager _currenciesManager;
        private readonly RelayCommand _onClick;

        private static List<string> _successSounds = new()
        {
            "o-pozabavimsja",
            "sorceresswhat4",
            "peasantwhat3",
            "peasantwhat1",
            "peasantbuildingcomplete1",
            "orcbarrackswhat1",
            "heroblademasterwhat1",
            "gruntready1",
            "goblinsapperpissed4",
            "foresttrollyes1",
            "foresttrollready1",
            "buildingconstruction",
            "buildingplacement",
            "acolyteyes2"
        };

        public static List<string> FailSounds = new() {"gryphonriderwhat5", "gruntnogold1", "error", "necromancernogold1"};

        public UpgradeViewModel(UpgradeData upgradeData, 
            IUpgradesManager upgradesManager,
            ICurrenciesManager currenciesManager)
        {
            _upgradeData = upgradeData;
            _upgradesManager = upgradesManager;
            _currenciesManager = currenciesManager;
            Title = upgradeData.Title;
            Description = upgradeData.Description;
            Cost = upgradeData.Price.ToString(true);
            _onClick = new RelayCommand(_ => ClickHandler());
        }
        
        public string Title { get; }
        public string Description { get; }
        public string Cost { get; }
        public ICommand OnClick => _onClick;
        

        private void ClickHandler()
        {
            if (_currenciesManager.CanPay(_upgradeData.Price))
            {
                _currenciesManager.Pay(_upgradeData.Price);
                _upgradesManager.Unlock(_upgradeData);
                if (string.IsNullOrEmpty(_upgradeData.Sound))
                {
                    AudioService.Instance.PlaySound($"Upgrades/{_successSounds.RandomSubset(1).Single()}", 1f, true, true);
                }
                else
                {
                    AudioService.Instance.PlaySound($"Upgrades/{_upgradeData.Sound}", 1f, true, true);
                }
            }
            else
            {
                AudioService.Instance.PlaySound($"SpellFail/{FailSounds.RandomSubset(1).Single()}");
            }
        }
    }
}