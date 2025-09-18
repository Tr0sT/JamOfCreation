#nullable enable
using System;

namespace Nuclear.Services
{
    public interface IViewModel
    {
    }

    public interface IDisposableViewModel : IViewModel, IDisposable
    {
        
    }
}