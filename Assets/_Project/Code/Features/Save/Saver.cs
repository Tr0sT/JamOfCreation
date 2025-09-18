#nullable enable
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace NuclearBand.Game
{
    [UsedImplicitly]
    public sealed class Saver : ISaver
    {
        public Saver()
        {
            var saveString = PlayerPrefs.GetString("Save");
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter {AllowIntegerValues = true}
                }
            };
            if (string.IsNullOrEmpty(saveString))
            {
                Save = new Save();
            }
            else
            {
                Save = JsonConvert.DeserializeObject<Save>(
                    saveString,
                    jsonSerializerSettings)!;
            }
        }
        
        
        public Save Save { get; private set; }
        public void AddUpgradeEntryId(UpgradeDataId upgradeDataId)
        {
            if (Save.UpgradeEntryIds.Contains(upgradeDataId))
            {
                return;
            }
            Save.UpgradeEntryIds.Add(upgradeDataId);
            var s= JsonConvert.SerializeObject(Save);
            PlayerPrefs.SetString("Save", s);
        }
    }
}