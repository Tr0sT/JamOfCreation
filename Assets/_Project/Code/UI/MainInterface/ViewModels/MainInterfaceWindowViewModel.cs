#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace NuclearBand.Game
{
    [UsedImplicitly]
    public sealed class MainInterfaceWindowViewModel : IMainInterfaceWindowViewModel
    {
        private readonly IDisposable _d;
        public ReadOnlyCollection<ICurrencyViewModel> CurrencyViewModels { get; }
        public IUpgradesPanelViewModel UpgradesPanelViewModel { get; }
        public IActionsPanelViewModel ActionsPanelViewModel { get; }
        public IPromptOptionsViewModel PromptOptionsViewModel { get; }
        public IActionsPanelViewModel PromptsPanelViewModel { get; }

        public MainInterfaceWindowViewModel(ICurrenciesManager currenciesManager, 
            IUpgradesManager upgradesManager, 
            IActionsManager actionsManager,
            ITimeManager timeManager)
        {
            CurrencyViewModels = currenciesManager.GetCurrencies()
                .Select(c => (ICurrencyViewModel)new CurrencyViewModel(currenciesManager, c.CurrencyType))
                .ToList().AsReadOnly();
            UpgradesPanelViewModel = new UpgradesPanelViewModel(upgradesManager, currenciesManager);
            ActionsPanelViewModel = new ActionsPanelViewModel(actionsManager, currenciesManager, ActionPanelType.Actions);
            PromptOptionsViewModel = new PromptOptionsViewModel(actionsManager);
            PromptsPanelViewModel = new ActionsPanelViewModel(actionsManager, currenciesManager, ActionPanelType.Prompts);
            _d = timeManager.SecondsFromStart.Subscribe(_ => Tick());
        }

        public void Dispose()
        {
            UpgradesPanelViewModel.Dispose();
            ActionsPanelViewModel.Dispose();
            PromptsPanelViewModel.Dispose();
            _d.Dispose();
        }

        private void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Z) && PromptsPanelViewModel.ActionViewModels.Count() > 0) 
                ((IActionViewModel)(((System.Collections.IList)(PromptsPanelViewModel.ActionViewModels))[0])).OnClick.Execute(null!);
            else if (Input.GetKeyDown(KeyCode.X) && PromptsPanelViewModel.ActionViewModels.Count() > 1) 
                ((IActionViewModel)(((System.Collections.IList)(PromptsPanelViewModel.ActionViewModels))[1])).OnClick.Execute(null!);
            else if (Input.GetKeyDown(KeyCode.C) && PromptsPanelViewModel.ActionViewModels.Count() > 2) 
                ((IActionViewModel)(((System.Collections.IList)(PromptsPanelViewModel.ActionViewModels))[2])).OnClick.Execute(null!);
            
            else if (Input.GetKeyDown(KeyCode.A) && PromptOptionsViewModel.Visible.Value) 
                PromptOptionsViewModel.SelectedOption.Value = PromptOption.Default;
            else if (Input.GetKeyDown(KeyCode.S) && PromptOptionsViewModel.Visible.Value) 
                PromptOptionsViewModel.SelectedOption.Value = PromptOption.Reasoning;
            else if (Input.GetKeyDown(KeyCode.D) && PromptOptionsViewModel.Visible.Value) 
                PromptOptionsViewModel.SelectedOption.Value = PromptOption.WebSearch;
            
            else if (Input.GetKeyDown(KeyCode.Q) && ActionsPanelViewModel.ActionViewModels.Count() > 0)
                ((IActionViewModel)(((System.Collections.IList)(ActionsPanelViewModel.ActionViewModels))[0])).OnClick.Execute(null!);
            else if (Input.GetKeyDown(KeyCode.W) && ActionsPanelViewModel.ActionViewModels.Count() > 1) 
                ((IActionViewModel)(((System.Collections.IList)(ActionsPanelViewModel.ActionViewModels))[1])).OnClick.Execute(null!);
            else if (Input.GetKeyDown(KeyCode.E) && ActionsPanelViewModel.ActionViewModels.Count() > 2) 
                ((IActionViewModel)(((System.Collections.IList)(ActionsPanelViewModel.ActionViewModels))[2])).OnClick.Execute(null!);
        }
    }
}