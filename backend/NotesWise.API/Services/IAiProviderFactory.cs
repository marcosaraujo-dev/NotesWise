namespace NotesWise.API.Services
{
    public interface IAiProviderFactory
    {
        IAiProvider CreateProvider(string providerName);
        IAiProvider CreateDefaultProvider();
        IEnumerable<string> GetAvailableProviders();
    }
}
