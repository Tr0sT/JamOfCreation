#nullable enable
using VContainer;
using VContainer.Unity;

namespace NuclearBand.Game
{
    public sealed class FeatureInstallers : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<TimeManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<Settings>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<Saver>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<UpgradesManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ActionsManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<MiningManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<CurrenciesManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}