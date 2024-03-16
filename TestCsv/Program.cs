using CsvHelper;
using System.Globalization;

namespace TestCsv
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var records = new List<Foo>
            {
                new Foo { Id = 1, Name = "one" },
            };

            using (var writer = new StreamWriter("file.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }
    }

    public class Foo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}