using System.Collections.Generic;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Model
{
    public class DBQuery
    {
        public FilterQuery Filters { get; set; }
        public List<Field> Fields { get; set; }
        public List<SortField> SortBy { get; set; }
        public DBQuery()
        {
            Fields = new List<Field>();
            SortBy = new List<SortField>();
            Filters = new FilterQuery();

        }
    }

    public class FilterQuery : List<Filter>
    {

    } 

    public class Filter
    {
        public FilterField Field { get; set; }
        public FilterCondition Condition { get; set; }
        public Filter(string field, object value, FilterCondition condition = FilterCondition.AND)
        {
            this.Field = new FilterField(field, value);
            this.Condition = condition;
        }
    }

    public enum FilterCondition
    {
        AND,
        OR
    }
    
    public class Field
    {
        public string Name { get; set; }
        public FieldDataType DataType { get; set; }
        public Field(string name)
        {
            this.Name = name;
        }

    }
    public class FilterField : Field
    {
        public FilterField(string name, object value): base(name)
        {
            this.Value = value;
        }

        public object Value { get; set; }
    }

    public class SortField : Field
    {
        public SortField(string name, SortType sort = SortType.ASC) : base(name)
        {
            Sort = sort;
        }

        public SortType Sort { get; set; }
    }
    public enum FieldDataType
    {
        String,
        Numeric
    }
    public enum SortType
    {
        ASC = 1,
        DESC = -1
    }

    public class RawQuery : IDBQueryBuilder
    {
        private readonly string _filter;
        public RawQuery(string filter)
        {
            _filter = filter;
        }
        public string GetQuery()
        {
            return _filter;
        }
    }
}
