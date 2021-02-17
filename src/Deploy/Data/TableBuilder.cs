namespace Deploy.Data
{
    internal class TableBuilder
    {
        private readonly Table table;

        private TableBuilder(string name)
        {
            table = new Table(name);
        }

        public static TableBuilder Create(string name)
        {
            return new TableBuilder(name);
        }

        public TableBuilder PrimaryColumn(string name, DataType type = DataType.Char, Constraint constraint = Constraint.NotNull)
        {
            table.Columns.Add(new Column(name, type, constraint, true));

            return this;
        }

        public TableBuilder Column(string name, DataType type = DataType.Char, Constraint constraint = Constraint.Null)
        {
            table.Columns.Add(new Column(name, type, constraint, false));

            return this;
        }

        public Table Build()
        {
            return table;
        }
    }
}
