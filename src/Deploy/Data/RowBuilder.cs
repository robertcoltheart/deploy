using System.Collections.Generic;

namespace Deploy.Data
{
    public class RowBuilder
    {
        private readonly string _table;
        private readonly List<object> _data = new List<object>();

        public RowBuilder(string table)
        {
            _table = table;
        }

        public RowBuilder Value(object value)
        {
            _data.Add(value);

            return this;
        }

        public Row Build()
        {
            return new Row(_table, _data.ToArray());
        }
    }
}
