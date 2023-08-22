using System;

namespace Scrabby.Utilities
{
    [Serializable]
    public class StringKvp
    {
        public string Key;
        public string Value;
        
        public StringKvp(string key, string value)
        {
            Key = key;
            Value = value;
        }
        
        public override string ToString()
        {
            return Key + ": " + Value;
        }
        
        public static StringKvp Create(string key, string value)
        {
            return new StringKvp(key, value);
        }
    }
}