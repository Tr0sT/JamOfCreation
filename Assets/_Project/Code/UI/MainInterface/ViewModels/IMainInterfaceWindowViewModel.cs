#nullable enable
using System.Collections.ObjectModel;
using Nuclear.Services;

namespace NuclearBand.Game
{
    public interface IMainInterfaceWindowViewModel : IDisposableViewModel
    {
        ReadOnlyCollection<ICurrencyViewModel> CurrencyViewModels { get; }
        IUpgradesPanelViewModel UpgradesPanelViewModel { get; }
        IActionsPanelViewModel ActionsPanelViewModel { get; }
        IPromptOptionsViewModel PromptOptionsViewModel { get; }
        IActionsPanelViewModel PromptsPanelViewModel { get; }
    }
}
