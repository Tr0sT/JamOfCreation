#nullable enable
using System.Collections.Immutable;
using System.Text;

namespace NuclearBand.Game
{
    public sealed record UpgradeSettings(ImmutableArray<UpgradeData> Upgrades);
    public sealed record UpgradeDataId(string Value);

    public enum CurrencyType
    {
        Energy,
        Ideas,
        Art,
        Code
    }
    
    public sealed record ResourcesPair(CurrencyType CurrencyType, float Count);

    public sealed record Price(ImmutableArray<ResourcesPair> Resources)
    {
        public override string ToString() => ToString(false);

        public string ToString(bool multiline)
        {
            var s = new StringBuilder();
            for (var index = 0; index < Resources.Length; index++)
            {
                var resourcesPair = Resources[index];
                switch (resourcesPair.CurrencyType)
                {
                    case CurrencyType.Energy:
                        s.Append(":energy:");
                        break;
                    case CurrencyType.Ideas:
                        s.Append(":ideas:");
                        break;
                    case CurrencyType.Art:
                        s.Append(":art:");
                        break;
                    case CurrencyType.Code:
                        s.Append(":code:");
                        break;
                }

                s.Append(resourcesPair.Count.ToString("F1"));
                if (multiline && index != Resources.Length - 1)
                {
                    s.Append("\n");
                }
            }

            return s.ToString();
        }
    }
    public sealed record Unlock(CurrencyBonus? CurrencyBonus,
        ActionType? ActionTypeUnlock,
        CurrencyType? CurrencyTypeUnlock,
        UnlockType? UnlockType
    );

    public enum UnlockType
    {
        AdvancedPrompting
    }

    public sealed record CurrencyBonus(
        CurrencyType CurrencyType,
        float? CurrentBonus,
        int? MaxBonus,
        float? RegenerationRateBonus);

    public sealed record UpgradeData(
        Price Price,
        UpgradeDataId Id,
        string Title,
        string Description,
        ImmutableArray<UpgradeDataId> Restrictions,
        ImmutableArray<Unlock> Unlocks,
        string? Sound);
}