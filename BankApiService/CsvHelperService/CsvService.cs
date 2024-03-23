using BankApiService.Controllers;
using BankApiService.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;

namespace BankApiService.CsvHelperService
{
    public static class CsvService
    {
        public static void WriteToCsv(List<Account> listToWrite)
        {
            // Append to the file.
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = !File.Exists("accounts.csv"),
            };

            using (var stream = File.Open("accounts.csv", FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(listToWrite);
            }
        }

        public static List<Account> ReadFromCsv()
        {
            if (!File.Exists("accounts.csv"))
            {
                return new List<Account>();
            }

            using (var reader = new StreamReader("accounts.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Read all records and return as a list of Person objects
                return csv.GetRecords<Account>().ToList();
            }
        }

        public static Account GetAccountById(int id)
        {
            var allAccounts = ReadFromCsv();

            foreach (var account in allAccounts)
            {
                if (account.Id == id)
                {
                    return account;
                }
            }
            
            return new Account() { Id = -1 };
        }

        public static void DeleteAccount(int id)
        {
            // Получить все аккаунты
            var allAccounts = ReadFromCsv();

            // Удалить аккаунт с указанным id из списка всех аккаунтов

            var accountToDelete = allAccounts.FirstOrDefault(acc => acc.Id == id);

            allAccounts.Remove(accountToDelete);

            // Записать обновленный список аккаунтов в файл
            OverwriteAccountsToCsv(allAccounts);
        }

        public static void OverwriteAccountsToCsv(List<Account> accounts)
        {
            // Append to the file.
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = true,
            };

            using (var stream = File.Open("accounts.csv", FileMode.Create))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(accounts);
            }
        }

        public static void UpdateAccountInformation(Account accountToUpdate)
        {
            // Получить все аккаунты
            var allAccounts = ReadFromCsv();

            // Найти аккаунт с указанным id
            var account = allAccounts.FirstOrDefault(acc => acc.Id == accountToUpdate.Id);

            // Удалить аккаунт с указанным id из списка всех аккаунтов

            allAccounts.Remove(account);

            // Добавить обновленный аккаунт в список всех аккаунтов
            allAccounts.Add(accountToUpdate);

            // Записать обновленный список аккаунтов в файл
            OverwriteAccountsToCsv(allAccounts);
        }
    }
}
