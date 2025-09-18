#nullable enable
using System;
using System.Collections.ObjectModel;
using R3;

namespace NuclearBand.Game
{
    public interface IActionsManager : IDisposable
    {
        event Action OnActionTypesChanged;
        ReadOnlyCollection<IPrompt> Actions { get; }
        ReadOnlyReactiveProperty<bool> AdvancedPrompting { get; }
        ReadOnlyReactiveProperty<PromptOption> PromptOption { get; }
        
        void Unlock(ActionType unlockActionType);
        IPrompt GetAction(ActionType actionType);

        bool CanUse(ActionType actionType);
        void Use(ActionType actionType);
        void UnlockAdvancedPrompting();
        void SetPromptOption(PromptOption promptOption);
        void SetCooldownCoeff(float f);
    }
}