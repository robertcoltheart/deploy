namespace Deploy.Serialization
{
    public class SummaryInformation
    {
        public string Title { get; set; } = "Installation Database";

        public string Subject { get; set; }

        public string Author { get; set; }

        public string Keywords { get; set; } = "Installer";

        public string Template { get; set; }

        public string RevisionNumber { get; set; }

        public int PageCount { get; set; } = 500;

        public int WordCount { get; set; } = 2;
    }
}
