#nullable enable
using VContainer;

namespace Nuclear.Services
{
    public interface IObjectResolverProvider
    {
        IObjectResolver Container { get; }
    }
}