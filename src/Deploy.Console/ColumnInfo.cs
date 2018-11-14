namespace Deploy.Console
{
    /*
Unk: 1000_0000_0000_0000 Always present
Tmp: 0100_0000_0000_0000 Never present
PrK: 0010_0000_0000_0000 PRIMARY KEY
Nul: 0001_0000_0000_0000 NULL, NOT NULL

Str: 0000_1000_0000_0000 CHAR, OBJECT
???: 0000_0100_0000_0000 CHAR, SHORT
Loc: 0000_0010_0000_0000 LOCALIZABLE
Vld: 0000_0001_0000_0000 Always present
     */

    /// <summary>
    /// Char length in last 8
    /// Int size in last 8
    /// Object 0 in last 8
    /// </summary>
    public class ColumnInfo
    {
        public ColumnInfo(int id, string name, uint type)
        {
            Id = id;
            Name = name;
            Type = type;
        }

        public int Id { get; }

        public string Name { get; }

        public uint Type { get; }
    }
}
