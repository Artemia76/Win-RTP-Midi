using System;
using System.Collections.Generic;
using Spring.Net.Interop;

namespace Spring.Net
{
    public sealed class ServiceProvider : IRegisterService
    {
        private static readonly ServiceProvider instance_ = new ServiceProvider();
        private readonly IDictionary<Type, Object> services_ = new Dictionary<Type, Object>();

        private ServiceProvider()
        {
        }

        public static IRegisterService GetProvider()
        {
            return instance_;
        }

        #region IServiceProvider Implementation

        public void RegisterService<T>(T t) where T : class
        {
            services_.Add(typeof (T), t);
        }

        #endregion

        #region IServiceProviderEx Implementation

        public T GetService<T>() where T : class
        {
            return (T) GetService(typeof (T));
        }

        #endregion

        #region IServiceProvider Implementation

        public object GetService(Type serviceType)
        {
            if (services_.ContainsKey(serviceType))
                return services_[serviceType];
            throw new NotSupportedException();
        }

        #endregion
    }
}