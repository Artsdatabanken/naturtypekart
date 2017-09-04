namespace Nin.Common.Map.Tiles.Stores
{
    public interface IPersistStuff<T>
    {
        void Save(string key, T tile);
        T Load(string key);
    }
}