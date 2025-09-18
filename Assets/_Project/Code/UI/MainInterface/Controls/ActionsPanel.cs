#nullable enable
using Noesis;

namespace NuclearBand.Game
{
    public partial class ActionsPanel : UserControl
    {
        public ActionsPanel()
        {
            NoesisUnity.LoadComponent(this, "Assets/_Project/Resources/GUI/MainInterfaceWindow/ActionsPanel.xaml");
        }
    }
}