using System.Diagnostics;

namespace Deploy.Console
{
    [DebuggerDisplay("{Value}")]
    public class StringValue
    {
        public StringValue(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}
