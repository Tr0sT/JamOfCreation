#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NuclearBand.Game
{
    public sealed class CurrenciesManager : ICurrenciesManager
    {
        private readonly ISettings _settings;
        private readonly ISaver _saver;
        private readonly Dictionary<CurrencyType, Currency> _currencies = new();

        public CurrenciesManager(ISettings settings, ISaver saver)
        {
            _settings = settings;
            _saver = saver;

            var debugBonus = GameEntry.DebugBonus;
            _currencies.Add(CurrencyType.Energy, new Currency(CurrencyType.Energy,
                0,
                _settings.EnergySettings.InitialCapacity,
                _settings.EnergySettings.InitialRegenerationRate + debugBonus, 
                true));
            
            _currencies.Add(CurrencyType.Ideas, new Currency(CurrencyType.Ideas, 0, 5, debugBonus, false));
            _currencies.Add(CurrencyType.Art, new Currency(CurrencyType.Art, 0, 5, debugBonus, false));
            _currencies.Add(CurrencyType.Code, new Currency(CurrencyType.Code, 0, 5, debugBonus, false));
        }

        public void Dispose()
        {
        }

        public ICurrency GetCurrency(CurrencyType currencyType) => _currencies[currencyType];
        public ReadOnlyCollection<ICurrency> GetCurrencies() => _currencies.Values.Select(c => (ICurrency) c).ToList().AsReadOnly();

        public void SetNewCurrentValue(CurrencyType currencyType, float newCurrent) => _currencies[currencyType].SetNewCurrentValue(newCurrent);
        public void SetNewMaxValue(CurrencyType currencyType, int newCurrent) => _currencies[currencyType].SetNewMaxValue(newCurrent);
        public void SetNewRegenerationRate(CurrencyType currencyType, float newCurrent) => _currencies[currencyType].SetNewRegenerationRate(newCurrent);
        public bool CanPay(Price upgradeDataPrice)
        {
            foreach (var resource in upgradeDataPrice.Resources)
            {
                if (_currencies[resource.CurrencyType].Current.CurrentValue < resource.Count)
                {
                    return false;
                }
            }
            return true;
        }

        public void Pay(Price upgradeDataPrice)
        {
            foreach (var resource in upgradeDataPrice.Resources)
            {
                _currencies[resource.CurrencyType].SetNewCurrentValue(
                    _currencies[resource.CurrencyType].Current.CurrentValue - resource.Count);
            }
        }

        public void AddCurrencyBonus(CurrencyBonus currencyBonus, bool ignoreLimits)
        {
            var currency = GetCurrency(currencyBonus.CurrencyType);
            if (currencyBonus.CurrentBonus != null)
            {
                var newValue = currency.Current.CurrentValue + currencyBonus.CurrentBonus.Value;
                if (!ignoreLimits)
                {
                    newValue = Math.Min(newValue, Math.Max(currency.Current.CurrentValue, currency.Max.CurrentValue));
                }
                SetNewCurrentValue(currencyBonus.CurrencyType, newValue);
            }
            if (currencyBonus.MaxBonus != null)
            {
                SetNewMaxValue(currencyBonus.CurrencyType, 
                    currency.Max.CurrentValue + currencyBonus.MaxBonus.Value);
            }
            if (currencyBonus.RegenerationRateBonus != null)
            {
                SetNewRegenerationRate(currencyBonus.CurrencyType, 
                    currency.RegenerationRate.CurrentValue + currencyBonus.RegenerationRateBonus.Value);
            }
        }

        public void Unlock(CurrencyType currencyType)
        {
            _currencies[currencyType].Unlock();
        }

        public void SetLimitCoeff(float coeff)
        {
            foreach (var currency in _currencies.Values)
            {
                currency.SetLimitCoeff(coeff);
            }
        }

        public void SetRegenCoeff(float coeff)
        {
            foreach (var currency in _currencies.Values)
            {
                currency.SetRegenCoeff(coeff);
            }
        }
    }
}