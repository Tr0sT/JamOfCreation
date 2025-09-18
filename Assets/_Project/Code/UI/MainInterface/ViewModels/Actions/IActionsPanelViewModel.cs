using System;
using ObservableCollections;
using R3;

namespace NuclearBand.Game
{
    public interface IActionsPanelViewModel : IDisposable
    {
        string Title { get; }
        INotifyCollectionChangedSynchronizedViewList<IActionViewModel> ActionViewModels { get; }
        IReadOnlyBindableReactiveProperty<bool> Visible { get; }
    }
}