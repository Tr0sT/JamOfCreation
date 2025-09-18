#nullable enable
using R3;

namespace NuclearBand.Game
{
    public enum PromptOption
    {
        Default,
        Reasoning,
        WebSearch
    }
    
    public interface IPromptOptionsViewModel
    {
        IReadOnlyBindableReactiveProperty<bool> Visible { get; }
        BindableReactiveProperty<PromptOption> SelectedOption { get; }
    }


}