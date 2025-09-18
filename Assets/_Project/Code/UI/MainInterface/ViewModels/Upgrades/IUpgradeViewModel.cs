#nullable enable

using System.Windows.Input;

namespace NuclearBand.Game
{
    public interface IUpgradeViewModel
    {
        string Title { get; }
        string Description { get; }
        string Cost { get; }

        ICommand OnClick { get; }
    }
}