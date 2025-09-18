#nullable enable
using System;
using R3;

namespace NuclearBand.Game
{
    public interface IPrompt : IDisposable
    {
        ActionType ActionType { get; }
        string Title { get; }
        ReadOnlyReactiveProperty<string> Description { get; }
        ActionPanelType ActionPanelType { get; }
        ReadOnlyReactiveProperty<float> RemainingCooldown { get; }
        ReadOnlyReactiveProperty<float> Cooldown { get; }
        ReadOnlyReactiveProperty<Price> Price { get; }
    }
}