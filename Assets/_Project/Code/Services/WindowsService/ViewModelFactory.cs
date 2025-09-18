#nullable enable
using System;
using JetBrains.Annotations;
using VContainer;

namespace Nuclear.Services
{
    [UsedImplicitly]
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IObjectResolverProvider _objectResolverProvider;

        public ViewModelFactory(IObjectResolverProvider objectResolverProvider)
        {
            _objectResolverProvider = objectResolverProvider;
        }

        public TViewModel Create<TViewModel>() where TViewModel : IViewModel
        {
            return _objectResolverProvider.Container.Resolve<TViewModel>();
        }

        public TViewModel Create<TViewModel, TViewModelData>(TViewModelData data) 
            where TViewModel : IViewModel
        {
            var viewModelCreator = _objectResolverProvider.Container.Resolve<Func<TViewModelData, TViewModel>>();
            return viewModelCreator.Invoke(data);
        }
    }
}