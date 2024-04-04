using BankApiService.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace BankApiService.CsvHelperService
{
    public class CsvService<T>  where T : EntityBase, new()
    {
        public void WriteToCsv(List<T> listToWrite, string fileName)
        {
            // Append to the file.
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = !File.Exists(fileName),
            };

            using (var stream = File.Open(fileName, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(listToWrite);
            }
        }

        public List<T> ReadFromCsv(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new List<T>();
            }

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Read all records and return as a list of Person objects
                return csv.GetRecords<T>().ToList();
            }
        }

        public T GetEntityById(int id, string fileName)
        {
            var list = ReadFromCsv(fileName);

            foreach (var account in list)
            {
                if (account.Id == id)
                {
                    return account;
                }
            }
            
            return new T() { Id = -1 };
        }

        public void DeleteEntity(int id, string fileName)
        {
            // Получить все аккаунты
            var list = ReadFromCsv(fileName);

            // Удалить аккаунт с указанным id из списка всех аккаунтов

            var entityToDelete = list.FirstOrDefault(acc => acc.Id == id);

            list.Remove(entityToDelete);

            // Записать обновленный список аккаунтов в файл
            OverwriteAccountsToCsv(list, fileName);
        }

        public void OverwriteAccountsToCsv(List<T> entites, string fileName)
        {
            // Append to the file.
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = true,
            };

            using (var stream = File.Open(fileName, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(entites);
            }
        }

        public void UpdateEntityInformation(T entityToUpdate, string fileName)
        {
            // Получить все аккаунты
            var list = ReadFromCsv(fileName);

            // Найти аккаунт с указанным id
            var account = list.FirstOrDefault(acc => acc.Id == entityToUpdate.Id);

            // Удалить аккаунт с указанным id из списка всех аккаунтов

            list.Remove(account);

            // Добавить обновленный аккаунт в список всех аккаунтов
            list.Add(entityToUpdate);

            // Записать обновленный список аккаунтов в файл
            OverwriteAccountsToCsv(list, fileName);
        }
    }
}
