using CsvHelper.Configuration.Attributes;

namespace PasswordCsvMergerTool
{
    internal class PasswordRow
    {
        [Index(0)]
        public string? Name { get; set; }

        [Index(1)]
        public string? Url { get; set; }

        [Index(2)]
        public string? Username { get; set; }

        [Index(3)]
        public string? Password { get; set; }
    }
}