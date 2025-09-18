#nullable enable
using System;
using System.Collections.Immutable;
using System.Linq;
using R3;

namespace NuclearBand.Game
{
    public sealed class Prompt : IPrompt
    {
        private readonly PromptData _promptData;
        private readonly ITimeManager _timeManager;
        private readonly ReactiveProperty<float> _remainingCooldown = new();
        private readonly ICurrenciesManager _currenciesManager;
        private readonly IActionsManager _actionsManager;
        private readonly ReadOnlyReactiveProperty<Price> _price;

        // Время (в секундах от старта), когда кулдаун должен закончиться
        private float _cooldownEndTime;
        // Подписка на SecondsFromStart для обновления обратного отсчёта
        private IDisposable? _timeSubscription;
        private readonly ReadOnlyReactiveProperty<string> _description;
        private float _cooldownReduction;
        private readonly ReactiveProperty<float> _cooldown = new();
        private readonly IDisposable _disposable;

        public Prompt(PromptData promptData,
            ITimeManager timeManager, 
            ICurrenciesManager currenciesManager,
            IActionsManager actionsManager)
        {
            _promptData = promptData;
            _timeManager = timeManager;
            _price = actionsManager.PromptOption.Select(p => 
                p == PromptOption.WebSearch && _promptData.ActionPanelType == ActionPanelType.Prompts
                    ? new Price(_promptData.Price.Resources
                        .Select(r => r with {Count = r.Count * 2})
                        .ToImmutableArray())
                    : _promptData.Price)!.ToReadOnlyReactiveProperty()!;
            _currenciesManager = currenciesManager;
            _actionsManager = actionsManager;
            if (_promptData.ActionPanelType == ActionPanelType.Actions)
            {
                _description = new ReactiveProperty<string>(_promptData.Description);
            }
            else
            {
                _description = _actionsManager.PromptOption.Select(p =>
                {
                    var coeff = p == PromptOption.Reasoning ? 2 : 1;
                    var resource = new ResourcesPair(_promptData.CurrencyBonus!.CurrencyType,
                        _promptData.CurrencyBonus.CurrentBonus!.Value * coeff);
                    var price = new Price(ImmutableArray.Create(resource));
                    return $"+{price.ToString()}";
                })!.ToReadOnlyReactiveProperty()!;
            }

            UpdateCooldown();
            _disposable = _actionsManager.PromptOption.Subscribe(p => UpdateCooldown());
        }

        public void Dispose()
        {
            // Отписываемся от подписки на время, если объект уничтожается раньше окончания кулдауна
            _timeSubscription?.Dispose();
            _timeSubscription = null;
            _disposable.Dispose();
        }

        private void UpdateCooldown()
        {
            if (_promptData.ActionPanelType == ActionPanelType.Actions)
            {
                _cooldown.Value = _promptData.Cooldown;
                return;
            }
            _cooldown.Value = (_actionsManager.PromptOption.CurrentValue == PromptOption.Reasoning ? _promptData.Cooldown * 3 : _promptData.Cooldown)
                              - _cooldownReduction;
        }

        public ActionType ActionType => _promptData.ActionType;
        public string Title => _promptData.Title;
        public ReadOnlyReactiveProperty<string> Description => _description;
        public ActionPanelType ActionPanelType => _promptData.ActionPanelType;
        public ReadOnlyReactiveProperty<float> RemainingCooldown => _remainingCooldown;
        public ReadOnlyReactiveProperty<float> Cooldown => _cooldown;
        public ReadOnlyReactiveProperty<Price> Price => _price;

        public void Use()
        {
            var promptOption = _actionsManager.PromptOption.CurrentValue;
            if (_promptData.CurrencyBonus != null)
            {
                _currenciesManager.AddCurrencyBonus(_promptData.CurrencyBonus, promptOption == PromptOption.WebSearch);
                if (promptOption == PromptOption.Reasoning)
                {
                    _currenciesManager.AddCurrencyBonus(_promptData.CurrencyBonus, false);
                }
            }

            if (_promptData.ActionType == ActionType.BrainStorm)
            {
                // удвоение лимитов на 10 секунд
                _currenciesManager.SetLimitCoeff(2.0f);
                _timeManager.ScheduleAction(10f, () => _currenciesManager.SetLimitCoeff(1.0f));
            }

            if (_promptData.ActionType == ActionType.Optimization)
            {
                _actionsManager.SetCooldownCoeff(4f);
                _timeManager.ScheduleAction(10f, () => _actionsManager.SetCooldownCoeff(0f));
            }
            
            if (_promptData.ActionType == ActionType.Coffee)
            {
                _currenciesManager.SetRegenCoeff(3.0f);
                _timeManager.ScheduleAction(10f, () => _currenciesManager.SetRegenCoeff(1.0f));
            }
            
            
            // Устанавливаем значение cooldown равным заданному числу секунд
            var cooldown =_cooldown.Value;
            _remainingCooldown.Value = cooldown;

            // Отписываемся от предыдущей подписки, если она есть
            _timeSubscription?.Dispose();

            // Запоминаем время окончания (относительно SecondsFromStart)
            var now = _timeManager.SecondsFromStart.CurrentValue;
            _cooldownEndTime = now + cooldown;

            // Подписываемся на обновления времени и обновляем оставшееся время
            _timeSubscription = _timeManager.SecondsFromStart.Subscribe(seconds =>
            {
                var remaining = _cooldownEndTime - seconds;
                if (remaining <= 0f)
                {
                    _remainingCooldown.Value = 0f;
                    // Отписываемся, когда кулдаун завершён
                    _timeSubscription?.Dispose();
                    _timeSubscription = null;
                }
                else
                {
                    _remainingCooldown.Value = remaining;
                }
            });
        }

        public void SetCooldownCoeff(float f)
        {
            _cooldownReduction = f;
            UpdateCooldown();
        }
    }
}