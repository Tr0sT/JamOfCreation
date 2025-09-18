#nullable enable
using System;
using R3;

namespace NuclearBand.Game
{
    public interface ITimeManager : IDisposable
    {
        ReadOnlyReactiveProperty<float> SecondsFromStart { get; }
        IDisposable ScheduleAction(float delay, Action action);
    }
}