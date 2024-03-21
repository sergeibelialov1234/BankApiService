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
        public ActionResult<List<Account>> GetAccounts()
        {
            try
            {
                var accountList = CsvService.ReadFromCsv();
                return Ok(accountList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Account> GetAccountById([FromRoute] int id)
        {
            var account = CsvService.GetAccountById(id);

            if (account.Id == -1)
            {
                return BadRequest($"Account with ID: {id} not found.");
            }

            return Ok(account);
        }

        [HttpPost]
        public ActionResult<Account> CreateAccout([FromBody] Account account)
        {
            var random = new Random();
            account.Number = random.Next(100, 99999);
            var Id = 0;

            var allAccounts = CsvService.ReadFromCsv();

            if (allAccounts.Count == 0)
            {
                Id = 1;
            }
            else
            {
                var lastAccount = allAccounts.LastOrDefault();
                Id = lastAccount.Id;
                Id++;
            }

            account.Id = Id;

            var listAccounts = new List<Account>();
            listAccounts.Add(account);

            try
            {
                CsvService.WriteToCsv(listAccounts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(account);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteById([FromRoute] int id)
        {
            CsvService.DeleteAccount(id);
            return Ok();
        }
    }
}
