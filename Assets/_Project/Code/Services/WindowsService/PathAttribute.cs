#nullable enable
using System;

namespace Nuclear.Services
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PathAttribute : Attribute
    {
        public string Path { get; }

        public PathAttribute(string path)
        {
            Path = path;
        }
    }
}