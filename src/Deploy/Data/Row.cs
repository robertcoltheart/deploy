namespace Deploy.Data
{
    public class Row
    {
        public Row(string table, object[] data)
        {
            Table = table;
            Data = data;
        }

        public string Table { get; }

        public object[] Data { get; }
    }
}
