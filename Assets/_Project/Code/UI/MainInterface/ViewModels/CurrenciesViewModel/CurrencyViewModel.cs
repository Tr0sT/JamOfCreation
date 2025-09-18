#nullable enable
using System;
using R3;

namespace NuclearBand.Game
{
    internal sealed class CurrencyViewModel : ICurrencyViewModel
    {
        private readonly ICurrenciesManager _currenciesManager;
        private readonly ICurrency _currency;

        public CurrencyViewModel(ICurrenciesManager currenciesManager, CurrencyType currencyType)
        {
            _currenciesManager = currenciesManager;

            Name = currencyType switch
            {
                CurrencyType.Energy => ":energy:Энергия",
                CurrencyType.Ideas => ":ideas:Идеи",
                CurrencyType.Art => ":art:Арт",
                CurrencyType.Code => ":code:Код",
                _ => throw new ArgumentOutOfRangeException(nameof(currencyType), currencyType, null)
            };

            _currency = _currenciesManager.GetCurrency(currencyType);
            RegenerationValue = _currency.RegenerationRate.Select(v => $"{(v >= 0 ? "+" : "-")}{v.ToString("F1")}/sec")!.ToReadOnlyBindableReactiveProperty()!;
            CurrentAndMaxValue = _currency.Current
                .CombineLatest(_currency.Max, (v, m) => $"{v.ToString("F1")}/{m}")!
                .ToReadOnlyBindableReactiveProperty()!;
            Progress = _currency.Current.CombineLatest(_currency.Max, (v, m) => m > 0 ? v / m : 0f)
                .ToReadOnlyBindableReactiveProperty();
            Visible = _currency.Visible.ToReadOnlyBindableReactiveProperty();
        }

        public string Name { get; }
        public IReadOnlyBindableReactiveProperty<string> CurrentAndMaxValue { get; }
        public IReadOnlyBindableReactiveProperty<string> RegenerationValue { get; }
        public IReadOnlyBindableReactiveProperty<float> Progress { get; }
        public IReadOnlyBindableReactiveProperty<bool> Visible { get; }
    }
}