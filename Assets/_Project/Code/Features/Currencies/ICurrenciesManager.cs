#nullable enable
using System;
using System.Collections.ObjectModel;

namespace NuclearBand.Game
{
    public interface ICurrenciesManager : IDisposable
    {
        ICurrency GetCurrency(CurrencyType currencyType);
        ReadOnlyCollection<ICurrency> GetCurrencies();
        void SetNewCurrentValue(CurrencyType currencyType, float newCurrent);
        void SetNewMaxValue(CurrencyType currencyType, int newCurrent);
        void SetNewRegenerationRate(CurrencyType currencyType, float newCurrent);

        bool CanPay(Price upgradeDataPrice);
        void Pay(Price upgradeDataPrice);
        void AddCurrencyBonus(CurrencyBonus currencyBonus, bool ignoreLimits);
        void Unlock(CurrencyType currencyType);
        void SetLimitCoeff(float coeff);
        void SetRegenCoeff(float coeff);
    }
}