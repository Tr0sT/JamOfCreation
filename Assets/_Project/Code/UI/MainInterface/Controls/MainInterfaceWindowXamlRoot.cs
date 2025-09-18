#nullable enable
using Noesis;

namespace NuclearBand.Game
{
    public partial class MainInterfaceWindowXamlRoot : UserControl
    {
        public MainInterfaceWindowXamlRoot()
        {
            NoesisUnity.LoadComponent(this, "Assets/_Project/Resources/GUI/MainInterfaceWindow/MainInterfaceWindow.xaml");
        }
    }
}
