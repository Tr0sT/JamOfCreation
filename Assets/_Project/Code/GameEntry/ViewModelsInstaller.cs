#nullable enable
using JetBrains.Annotations;
using VContainer;
using VContainer.Unity;

namespace NuclearBand.Game
{
    [UsedImplicitly]
    public sealed class ViewModelsInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<GreetingWindowViewModel>(Lifetime.Transient)
                .As<IGreetingWindowViewModel>();

            builder.Register<MainInterfaceWindowViewModel>(Lifetime.Transient)
                .As<IMainInterfaceWindowViewModel>();
        }
    }
}
