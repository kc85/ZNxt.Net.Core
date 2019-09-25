using System;
using System.Collections.Generic;

namespace ZNxt.Net.Core.Model
{
    [Obsolete]
    public class ParamContainer
    {
        private readonly Dictionary<string, Func<object>> _keys = new Dictionary<string, Func<object>>();

        public Func<object> this[string key]
        {
            get
            {
                return _keys[key];
            }
            set => _keys[key] = value;
        }

        public void AddKey(string key, Func<object> val)
        {
            _keys[key] = val;
        }

        public object GetKey(string key)
        {
            if (_keys.ContainsKey(key))
            {
                return _keys[key]();
            }
            else
            {
                return null;
            }
        }

       
    }
}