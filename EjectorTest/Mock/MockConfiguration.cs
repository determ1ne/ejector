using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace EjectorTest.Mock
{
    internal class MockConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string> contents = new Dictionary<string, string>();
        
        public IConfigurationSection GetSection(string key)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new System.NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new System.NotImplementedException();
        }

        public string this[string key]
        {
            get => contents[key];
            set => contents[key] = value;
        }
    }
}