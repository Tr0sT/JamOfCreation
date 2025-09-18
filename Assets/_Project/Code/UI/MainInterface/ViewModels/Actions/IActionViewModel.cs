#nullable enable
using System.Windows.Input;
using R3;

namespace NuclearBand.Game
{
    public interface IActionViewModel
    {
        string Title { get; }
        IReadOnlyBindableReactiveProperty<string> Description { get; }
        IReadOnlyBindableReactiveProperty<string> RemainingCooldown { get; }
        IReadOnlyBindableReactiveProperty<string> Cooldown { get; }
        IReadOnlyBindableReactiveProperty<bool> ShowCooldown { get; }
        IReadOnlyBindableReactiveProperty<string> Cost { get; }
        ICommand OnClick { get; }
    }
}