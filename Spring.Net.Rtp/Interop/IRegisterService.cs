namespace Spring.Net.Interop
{
    public interface IRegisterService : IServiceProviderEx
    {
        void RegisterService<T>(T t) where T : class;
    }
}