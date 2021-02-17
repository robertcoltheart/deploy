using System.Collections.Generic;
using System.Linq;

namespace Deploy.Data
{
    internal class Table
    {
        public Table(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public List<Column> Columns { get; } = new List<Column>();

        public string Create()
        {
            var columns = Columns.Select(x => $"`{x.Name}` {x.Type.ToString().ToUpper()}{(x.Type == DataType.Char ? "(255)" : "")}{(x.Constraint == Constraint.NotNull ? " NOT NULL" : "")}")
                .Merge(", ");
            
            var keys = Columns.Where(x => x.PrimaryKey)
                .Select(x => $"`{x.Name}`")
                .Merge(", ");

            return $"CREATE TABLE `{Name}` ({columns} PRIMARY KEY {keys})";
        }

        public string Insert(params object[] values)
        {
            var columns = Columns.Select(x => $"`{x.Name}`")
                .Merge(", ");

            var queryValues = Columns.Zip(values, (x, y) => $"{(y == null ? "NULL" : x.Type == DataType.Char ? $"'{y}'" : y)}")
                .Merge(", ");

            return $"INSERT INTO `{Name}` ({columns}) VALUES ({queryValues})";
        }
    }
}
