#nullable enable
using System.Collections.Immutable;

namespace NuclearBand.Game
{
    public sealed record EnergySettings(int InitialCapacity, float InitialRegenerationRate);
}