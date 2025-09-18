using R3;

namespace NuclearBand.Game
{
    public interface ICurrencyViewModel
    {
        string Name { get; }
        IReadOnlyBindableReactiveProperty<string> RegenerationValue { get; }
        IReadOnlyBindableReactiveProperty<string> CurrentAndMaxValue { get; }
        IReadOnlyBindableReactiveProperty<float> Progress { get; } // 0..1
        IReadOnlyBindableReactiveProperty<bool> Visible { get; } 
    }
}