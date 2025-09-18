#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using R3;

namespace NuclearBand.Game
{
    [UsedImplicitly]
    public sealed class ActionsManager : IActionsManager
    {
        private readonly ISettings _settings;
        private readonly ISaver _saver;
        private readonly ITimeManager _timeManager;
        private readonly ICurrenciesManager _currenciesManager;
        private readonly Dictionary<ActionType, Prompt> _actions = new();
        private readonly ReactiveProperty<bool> _advancedPrompting = new();
        private readonly ReactiveProperty<PromptOption> _promptOption = new();

        public ActionsManager(ISettings settings, ISaver saver, ITimeManager timeManager,
            ICurrenciesManager currenciesManager)
        {
            _settings = settings;
            _saver = saver;
            _timeManager = timeManager;
            _currenciesManager = currenciesManager;
        }

        public void Dispose()
        {
            foreach (var action in _actions.Values)
            {
                action.Dispose();
            }
        }

        public event Action? OnActionTypesChanged;

        public ReadOnlyCollection<IPrompt> Actions => _actions.Values.Select(a => (IPrompt)a)
            .ToList().AsReadOnly();

        public ReadOnlyReactiveProperty<bool> AdvancedPrompting => _advancedPrompting;
        public ReadOnlyReactiveProperty<PromptOption> PromptOption => _promptOption; 

        public void Unlock(ActionType unlockActionType)
        {
            var promptData = _settings.PromptSettings.Prompts.First(p => p.ActionType == unlockActionType);

            _actions.Add(unlockActionType, new Prompt(promptData, _timeManager, _currenciesManager, this));

            OnActionTypesChanged?.Invoke();
        }

        public IPrompt GetAction(ActionType actionType) => _actions[actionType];
        public bool CanUse(ActionType actionType)
        {
            var action = GetAction(actionType);
            var actionPrice = action.Price.CurrentValue;
            if (action.RemainingCooldown.CurrentValue <= 0 && _currenciesManager.CanPay(actionPrice))
            {
                return true;
            }

            return false;
        }

        public void Use(ActionType actionType)
        {
            var promptOption = _promptOption;
            var oldPromptOption = _promptOption.Value;
            var action = GetAction(actionType);
            if (action.ActionPanelType == ActionPanelType.Actions)
            {
                promptOption.Value = NuclearBand.Game.PromptOption.Default;
            }
            var actionPrice = action.Price.CurrentValue;
            _currenciesManager.Pay(actionPrice);
            
            _actions[actionType].Use();
            _promptOption.Value = oldPromptOption;
        }

        public void UnlockAdvancedPrompting()
        {
            _advancedPrompting.Value = true;
        }

        public void SetPromptOption(PromptOption promptOption)
        {
            _promptOption.Value = promptOption;
        }

        public void SetCooldownCoeff(float f)
        {
            foreach (var prompt in _actions.Values)
            {
                prompt.SetCooldownCoeff(f);
            }
        }
    }
}