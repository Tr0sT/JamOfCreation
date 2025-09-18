#nullable enable
using System;
using R3;
using UnityEngine;

namespace NuclearBand.Game
{
    public sealed class MiningManager : IMiningManager
    {
        private readonly ISettings _settings;
        private readonly ISaver _saver;
        private readonly ICurrenciesManager _currenciesManager;

        private readonly IDisposable _timeSubscription;
        private float _lastSeconds;

        public MiningManager(ISettings settings, ISaver saver, 
            ITimeManager timeManager, 
            ICurrenciesManager currenciesManager)
        {
            _settings = settings;
            _saver = saver;
            _currenciesManager = currenciesManager;

            // подписываемся на обновление времени и обновляем энергию каждый тик
            _lastSeconds = timeManager.SecondsFromStart.CurrentValue;
            _timeSubscription = timeManager.SecondsFromStart.Subscribe(UpdateByTime);
        }
        
        public void Dispose()
        {
            _timeSubscription.Dispose();
        }

        private void UpdateByTime(float seconds)
        {
            var delta = seconds - _lastSeconds;
            _lastSeconds = seconds;
            if (delta <= 0f) return;

            foreach (var currency in _currenciesManager.GetCurrencies())
            {
                // Добавляем дробную энергию по скорости регенерации (единиц в секунду)
                var currencyDelta = currency.RegenerationRate.CurrentValue * delta;
                if (currencyDelta == 0)
                {
                    continue;
                }

                // Ограничиваем по максимуму
                var current = currency.Current.CurrentValue;
                var max = currency.Max.CurrentValue;
                if (current <= max && current + currencyDelta > max)
                {
                    current = max;
                }
                else
                {
                    current += currencyDelta;
                }

                _currenciesManager.SetNewCurrentValue(currency.CurrencyType, current);
            }
            
        }
    }
}