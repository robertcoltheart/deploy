namespace Deploy.Data
{
    internal class TableBuilder
    {
        private readonly Table _table;

        private TableBuilder(string name)
        {
            _table = new Table(name);
        }

        public static TableBuilder Create(string name)
        {
            return new TableBuilder(name);
        }

        public TableBuilder PrimaryColumn(string name, DataType type = DataType.Char, Constraint constraint = Constraint.NotNull)
        {
            _table.Columns.Add(new Column(name, type, constraint, true));

            return this;
        }

        public TableBuilder Column(string name, DataType type = DataType.Char, Constraint constraint = Constraint.Null)
        {
            _table.Columns.Add(new Column(name, type, constraint, false));

            return this;
        }

        public Table Build()
        {
            return _table;
        }
    }
}