#nullable enable
using Noesis;

namespace NuclearBand.Game
{
    public partial class PromptOptions : UserControl
    {
        public PromptOptions()
        {
            NoesisUnity.LoadComponent(this, "Assets/_Project/Resources/GUI/MainInterfaceWindow/PromptOptions.xaml");
        }
    }
}

