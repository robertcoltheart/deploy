using System.Linq;

namespace Deploy.Console
{
    public class Row
    {
        public Row(object[] data)
        {
            Data = data;
        }

        public object[] Data { get; }

        public override string ToString()
        {
            return string.Join(", ", Data.Select(x => x?.ToString()));
        }
    }
}
