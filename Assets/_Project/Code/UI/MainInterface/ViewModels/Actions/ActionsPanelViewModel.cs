#nullable enable
using System.Linq;
using ObservableCollections;
using R3;

namespace NuclearBand.Game
{
    internal sealed class ActionsPanelViewModel : IActionsPanelViewModel
    {
        private readonly IActionsManager _actionsManager;
        private readonly ICurrenciesManager _currenciesManager;
        private readonly ActionPanelType _actionPanelType;
        private readonly ObservableList<IActionViewModel> _actionViewModels = new ObservableList<IActionViewModel>();

        public ActionsPanelViewModel(IActionsManager actionsManager, ICurrenciesManager currenciesManager,
            ActionPanelType actionPanelType)
        {
            _actionsManager = actionsManager;
            _currenciesManager = currenciesManager;
            _actionPanelType = actionPanelType;
            _actionsManager.OnActionTypesChanged += OnActionTypesChanged;
            OnActionTypesChanged();
            Title = _actionPanelType == ActionPanelType.Actions ? "Действия:" : "Промпты:";
            Visible = _actionViewModels
                .ObserveCountChanged(true)
                .Select(v => v > 0)
                .ToReadOnlyBindableReactiveProperty();
        }

        public void Dispose()
        {
            _actionsManager.OnActionTypesChanged -= OnActionTypesChanged;
        }
        
        public string Title { get; }
        public INotifyCollectionChangedSynchronizedViewList<IActionViewModel> ActionViewModels => 
            _actionViewModels.ToNotifyCollectionChanged();
        public IReadOnlyBindableReactiveProperty<bool> Visible { get; }

        private void OnActionTypesChanged()
        {
            _actionViewModels.Clear();
            var actions = _actionsManager.Actions
                .Where(a => a.ActionPanelType == _actionPanelType)
                .Select(a => new ActionViewModel(a, _actionsManager));
            foreach (var actionViewModel in actions)
            {
                _actionViewModels.Add(actionViewModel);
            }
        }
    }
}