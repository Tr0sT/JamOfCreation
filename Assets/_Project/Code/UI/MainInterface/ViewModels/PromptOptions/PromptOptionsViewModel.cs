#nullable enable
using R3;

namespace NuclearBand.Game
{
    public sealed class PromptOptionsViewModel : IPromptOptionsViewModel
    {
        private readonly IActionsManager _actionsManager;

        public PromptOptionsViewModel(IActionsManager actionsManager)
        {
            _actionsManager = actionsManager;
            SelectedOption.Subscribe(v =>
            {
                _actionsManager.SetPromptOption(v);
            });
        }

        public IReadOnlyBindableReactiveProperty<bool> Visible => _actionsManager.AdvancedPrompting.ToReadOnlyBindableReactiveProperty();
        public BindableReactiveProperty<PromptOption> SelectedOption { get; } = new ();
    }
}