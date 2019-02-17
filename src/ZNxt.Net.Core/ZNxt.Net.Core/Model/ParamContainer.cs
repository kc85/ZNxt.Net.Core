﻿using System;
using System.Collections.Generic;

namespace ZNxt.Net.Core.Model
{
    public class ParamContainer
    {
        private Dictionary<string, Func<dynamic>> _keys = new Dictionary<string, Func<dynamic>>();

        public dynamic this[string key]
        {
            get
            {
                return _keys[key];
            }
            set
            {
                _keys[key] = value;
            }
        }

        public void AddKey(string key, Func<dynamic> val)
        {
            _keys[key] = val;
        }

        public dynamic GetKey(string key)
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