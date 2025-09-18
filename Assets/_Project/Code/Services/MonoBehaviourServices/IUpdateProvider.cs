#nullable enable
using System;

namespace Nuclear.Services
{
    public interface IUpdateProvider
    {
        event Action OnUpdate;
    }
}