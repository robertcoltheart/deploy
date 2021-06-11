namespace Deploy.Data
{
    internal class TableBuilder
    {
        private readonly Table _table;

        public TableBuilder(string name)
        {
            _table = new Table(name);
        }

        public static TableBuilder Create(string name)
        {
            return new TableBuilder(name);
        }

        public TableBuilder PrimaryColumn(string name, DataType type = DataType.Char, Constraint constraint = Constraint.NotNull, int length = 255)
        {
            _table.Columns.Add(new Column(name, type, constraint, true, length));

            return this;
        }

        public TableBuilder Column(string name, DataType type = DataType.Char, Constraint constraint = Constraint.Null, int length = 255)
        {
            _table.Columns.Add(new Column(name, type, constraint, false, length));

            return this;
        }

        public Table Build()
        {
            return _table;
        }
    }
}
