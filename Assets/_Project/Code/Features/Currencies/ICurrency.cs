#nullable enable
using R3;

namespace NuclearBand.Game
{
    public interface ICurrency
    {
        CurrencyType CurrencyType { get; }
        ReadOnlyReactiveProperty<int> Max { get; }
        ReadOnlyReactiveProperty<float> Current { get; }
        ReadOnlyReactiveProperty<float> RegenerationRate { get; }
        ReadOnlyReactiveProperty<bool> Visible { get; }
        
    }
}