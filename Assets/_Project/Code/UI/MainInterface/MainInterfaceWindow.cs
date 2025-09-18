#nullable enable
using Nuclear.Services;
using Nuclear.Services.GUI;
using UnityEngine;

namespace NuclearBand.Game
{
    [Path("GUI/MainInterfaceWindow/MainInterfaceWindow")]
    public sealed class MainInterfaceWindow : MWindow<IMainInterfaceWindowViewModel>
    {
        [SerializeField] private NoesisView _view = null!;

        public override void Init()
        {
            base.Init();
            _view.Content.DataContext = _viewModel;
        }
    }
}
