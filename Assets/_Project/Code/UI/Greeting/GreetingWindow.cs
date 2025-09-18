#nullable enable
using Nuclear.Services;
using Nuclear.Services.GUI;
using UnityEngine;

namespace NuclearBand.Game
{
    [Path("GUI/GreetingWindow/GreetingWindow")]
    public sealed class GreetingWindow : MWindow<IGreetingWindowViewModel>
    {
        [SerializeField] private NoesisView _view = null!;

        public override void Init()
        {
            base.Init();
            _view.Content.DataContext = _viewModel;
        }
    }
}