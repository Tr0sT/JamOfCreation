#nullable enable
using System.Collections.Immutable;

namespace NuclearBand.Game
{
    public sealed record PromptSettings(ImmutableArray<PromptData> Prompts);

    public enum ActionType
    {
        ChatGPT,
        NanoBanana,
        Claude,
        
        BrainStorm,
        Optimization,
        Coffee
    }

    public enum ActionPanelType
    {
        Actions,
        Prompts
    }

    public sealed record PromptData(
        string Title,
        string Description,
        ActionPanelType ActionPanelType,
        ActionType ActionType,
        int Cooldown,
        Price Price,
        CurrencyBonus? CurrencyBonus);
}