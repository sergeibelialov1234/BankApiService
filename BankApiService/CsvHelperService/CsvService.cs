using BankApiService.Models;
using CsvHelper;
using System.Globalization;

namespace BankApiService.CsvHelperService
{
    public static class CsvService
    {
        public static void WriteToCsv(List<Account> listToWrite)
        {
            using (var writer = new StreamWriter("accounts.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(listToWrite);
            }
        }
    }
}
