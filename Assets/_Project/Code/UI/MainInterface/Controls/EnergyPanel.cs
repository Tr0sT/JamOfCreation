#nullable enable
using Noesis;

namespace NuclearBand.Game
{
    public partial class EnergyPanel : UserControl
    {
        public EnergyPanel()
        {
            NoesisUnity.LoadComponent(this, "Assets/_Project/Resources/GUI/MainInterfaceWindow/EnergyPanel.xaml");
        }
    }
}