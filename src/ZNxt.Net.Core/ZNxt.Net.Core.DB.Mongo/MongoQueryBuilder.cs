using System.Text;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.DB.Mongo
{
    public class MongoQueryBuilder : IDBQueryBuilder
    {
        private readonly FilterQuery _filter;
        public MongoQueryBuilder(FilterQuery filter)
        {
            _filter = filter;
        }
        public string GetQuery()
        {

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("{");
            foreach (var filter in _filter)
            {
                switch (filter.Condition)
                {
                    case FilterCondition.AND:
                        sbQuery.Append("$and : ");
                        break;
                    case FilterCondition.OR:
                        sbQuery.Append("$or : ");
                        break;
                }

                sbQuery.Append("[");
                sbQuery.Append("{");
                if (filter.Field.Value.GetType() == typeof(string))
                {
                    sbQuery.Append($"{filter.Field.Name}:'{filter.Field.Value}'");
                }
                else if (filter.Field.Value.GetType() == typeof(bool))
                {
                    sbQuery.Append($"{filter.Field.Name}:{filter.Field.Value.ToString().ToLower()}");
                }

                else
                {
                    sbQuery.Append($"{filter.Field.Name}:{filter.Field.Value}");
                }
                sbQuery.Append("}");
                sbQuery.Append("]");
            }
            sbQuery.Append("}");
            return sbQuery.ToString();
        }
    }
}
