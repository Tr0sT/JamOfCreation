#nullable enable
using JetBrains.Annotations;
using Nuclear.Services;
using Nuclear.WindowsManager;
using VContainer;
using VContainer.Unity;

namespace NuclearBand.Game
{
    [UsedImplicitly]
    public sealed class WindowServiceInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(new WindowsManagerSettings("GUI/Canvas", "GUI/InputBlocker"));
            builder.Register<WindowsManager>(Lifetime.Transient).AsImplementedInterfaces();
            builder.Register<ObjectResolverProvider>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<GameObjectCreator>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ViewModelFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<WindowsService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterBuildCallback(container =>
            {
                var provider = container.Resolve<IObjectResolverProvider>() as ObjectResolverProvider;
                provider?.SetContainer(container);
            });
        }
    }
}