using System;

namespace Spring.Net.Interop
{
    public interface IServiceProviderEx : IServiceProvider
    {
        T GetService<T>() where T : class;
    }
}