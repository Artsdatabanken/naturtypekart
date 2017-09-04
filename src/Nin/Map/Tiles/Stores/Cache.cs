namespace Nin.Common.Map.Tiles.Stores
{
    public class Cache<T> : IPersistStuff<T>
    {
        readonly string[] keys;
        readonly T[] items;
        private int watermark;
        private readonly int cacheSize = 100;
        private readonly IPersistStuff<T> backend;

        public Cache(IPersistStuff<T> backend)
        {
            this.backend = backend;
            keys = new string[cacheSize];
            items = new T[cacheSize];
        }

        public void Save(string key, T tile)
        {
            backend.Save(key, tile);
            AddToCache(key, tile);
        }

        public T Load(string key)
        {
            int index = FindIndex(key);
            if (index >= 0)
                return items[index];

            var item = backend.Load(key);
            AddToCache(key, item);
            return item;
        }

        int FindIndex(string key)
        {
            for (int i = 0; i < cacheSize; i++)
                if (keys[i] == key)
                    return i;
            return -1;
        }

        private void AddToCache(string key, T item)
        {
            int index = FindIndex(key);
            if (index < 0)
            {
                index = watermark;
                watermark = (watermark+1) % cacheSize;
            }

            keys[index] = key;
            items[index] = item;
        }
    }
}