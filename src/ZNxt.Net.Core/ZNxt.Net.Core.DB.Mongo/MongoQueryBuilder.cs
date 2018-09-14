using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.DB.Mongo
{
    public class MongoQueryBuilder : IDBQueryBuilder
    {
        private FilterQuery _filter;
        public MongoQueryBuilder(FilterQuery filter)
        {
            _filter = filter;
        }
        public string GetQuery()
        {
            throw new NotImplementedException();
        }
    }
}
