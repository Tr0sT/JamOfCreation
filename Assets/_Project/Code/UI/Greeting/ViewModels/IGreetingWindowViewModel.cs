#nullable enable
using System.Windows.Input;
using Nuclear.Services;

namespace NuclearBand.Game
{
    public interface IGreetingWindowViewModel : IViewModel
    {
        ICommand StartCommand { get; }
    }
}