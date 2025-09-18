#nullable enable
using Noesis;

namespace NuclearBand.Game
{
    public partial class UpgradesPanel : UserControl
    {
        public UpgradesPanel()
        {
            NoesisUnity.LoadComponent(this, "Assets/_Project/Resources/GUI/MainInterfaceWindow/UpgradesPanel.xaml");
        }
    }
}