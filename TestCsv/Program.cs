using CsvHelper;
using JsonFormatterPlus;
using System.Globalization;
using System.Text.Json;

namespace TestCsv
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var records = new List<Foo>
            {
                new Foo { Id = 1, Name = "one" },
                new Foo { Id = 2, Name = "two" },
                new Foo { Id = 3, Name = "three"}
            };

            var jsonSting = JsonSerializer.Serialize(records);


            string formattedJson = JsonFormatter.Format(jsonSting);
            Console.WriteLine(formattedJson);
        }
    }

    public class Foo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}