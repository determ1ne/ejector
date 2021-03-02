using System.Collections.Generic;

namespace Ejector.Services
{
    public class NaiveCache
    {
        private readonly Dictionary<string, object> _contents;

        public NaiveCache()
        {
            _contents = new Dictionary<string, object>();
        }

        public void Set(string key, object value) => _contents[key] = value;

        public object Get(string key) => _contents[key];

        public void Remove(string key) => _contents.Remove(key);
    }
}