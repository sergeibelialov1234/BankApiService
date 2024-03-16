using BankApiService.CsvHelperService;
using BankApiService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankApiService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Accounts : ControllerBase
    {
        [HttpGet]
        public List<Account> GetAccounts()
        {
            var account = new Account { Id = 1, Number = 123, Balance = 1000 };
            var account2 = new Account { Id = 2, Number = 456, Balance = 2000 };
            var list = new List<Account>();
            list.Add(account);
            list.Add(account2);

            return list;
        }

        [HttpPost]
        public string CreateAccout(Account account)
        {
            var random = new Random();
            account.Number = random.Next(100, 99999);

            var listAccounts = new List<Account>();
            listAccounts.Add(account);

            CsvService.WriteToCsv(listAccounts);

            return "Account created";
        }
    }
}
