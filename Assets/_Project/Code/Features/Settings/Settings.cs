#nullable enable
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace NuclearBand.Game
{
    [UsedImplicitly]
    public sealed class Settings : ISettings
    {
        public Settings()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter {AllowIntegerValues = true}
                }
            };
            
            UpgradeSettings = JsonConvert.DeserializeObject<UpgradeSettings>(
                Resources.Load<TextAsset>("Settings/UpgradeSettings").text, 
                jsonSerializerSettings)!;
            
            EnergySettings = JsonConvert.DeserializeObject<EnergySettings>(
                Resources.Load<TextAsset>("Settings/EnergySettings").text, 
                jsonSerializerSettings)!;
            
            PromptSettings = JsonConvert.DeserializeObject<PromptSettings>(
                Resources.Load<TextAsset>("Settings/PromptSettings").text, 
                jsonSerializerSettings)!;
        }

        public UpgradeSettings UpgradeSettings { get; }
        public EnergySettings EnergySettings { get; }
        public PromptSettings PromptSettings { get; }
    }
}