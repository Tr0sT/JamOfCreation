#nullable enable
using System.Linq;
using System.Windows.Input;
using MoreLinq;
using R3;

namespace NuclearBand.Game
{
    internal sealed class ActionViewModel : IActionViewModel
    {
        private readonly IActionsManager _actionsManager;
        private readonly RelayCommand _onClick;
        private readonly IPrompt _action;

        public ActionViewModel(IPrompt action, IActionsManager actionsManager)
        {
            _action = action;
            _actionsManager = actionsManager;
            _onClick = new RelayCommand(_ => ClickHandler());
            RemainingCooldown = _action.RemainingCooldown.Select(v => v <= 0 ? "" : v.ToString("F1"))!.ToReadOnlyBindableReactiveProperty()!;
            ShowCooldown = _action.RemainingCooldown.Select(v => v > 0).ToReadOnlyBindableReactiveProperty();
            Cost = _action.Price.Select(p => p.ToString())!.ToReadOnlyBindableReactiveProperty()!;
            Cooldown = _action.Cooldown.Select(v => $"{((int)v).ToString()}сек")!.ToReadOnlyBindableReactiveProperty()!;
        }

        public string Title => _action.Title;
        public IReadOnlyBindableReactiveProperty<string> Description => _action.Description!.ToReadOnlyBindableReactiveProperty()!;
        public IReadOnlyBindableReactiveProperty<string> RemainingCooldown { get; }
        public IReadOnlyBindableReactiveProperty<string> Cooldown { get; }
        public IReadOnlyBindableReactiveProperty<bool> ShowCooldown { get; }
        public IReadOnlyBindableReactiveProperty<string> Cost { get; }
        public ICommand OnClick => _onClick;

        private void ClickHandler()
        {
            if (_actionsManager.CanUse(_action.ActionType))
            {
                _actionsManager.Use(_action.ActionType);
                AudioService.Instance.PlaySound("SpellOk/levelupcaster", 1f, false, false);
            }
            else
            {
                AudioService.Instance.PlaySound($"SpellFail/{UpgradeViewModel.FailSounds.RandomSubset(1).Single()}");
            }
        }
        
    }
}