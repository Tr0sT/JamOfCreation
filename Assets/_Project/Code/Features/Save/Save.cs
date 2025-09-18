#nullable enable

using System.Collections.Generic;

namespace NuclearBand.Game
{
    public sealed class Save
    {
        public List<UpgradeDataId> UpgradeEntryIds { get; set; } = new();
    }
}