#nullable enable
using System.Windows.Input;
using JetBrains.Annotations;
using Nuclear.Services;

namespace NuclearBand.Game
{
    [UsedImplicitly]
    internal sealed class GreetingWindowViewModel : IGreetingWindowViewModel
    {
        private readonly IWindowsService _windowsService;
        public ICommand StartCommand { get; }

        public GreetingWindowViewModel(IWindowsService windowsService)
        {
            _windowsService = windowsService;
            StartCommand = new RelayCommand(_ => Start());
        }

        private void Start()
        {
            AudioService.Instance.PlaySound("Upgrades/buildingplacement");
            _windowsService.CreateWindow<MainInterfaceWindow, IMainInterfaceWindowViewModel>();
        }
    }
}