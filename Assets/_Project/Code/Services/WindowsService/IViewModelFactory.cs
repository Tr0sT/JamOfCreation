#nullable enable
namespace Nuclear.Services
{
    public interface IViewModelFactory
    {
        TViewModel Create<TViewModel>() where TViewModel : IViewModel;
        TViewModel Create<TViewModel, TViewModelData>(TViewModelData data) where TViewModel : IViewModel;
    }
}