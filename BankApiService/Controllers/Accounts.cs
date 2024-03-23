using BankApiService.CsvHelperService;
using BankApiService.Enums;
using BankApiService.IdService;
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
            var nextId = IdHelper.GetNextId();

            account.Id = nextId;

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

        [HttpPost("{id}/deposit")]
        public ActionResult<Account> DepositToAccount(
            [FromRoute] int id,
            [FromBody] DepositRequest depositRequest)
        {
            var accountTodepoist = CsvService.GetAccountById(id);

            accountTodepoist.Balance += depositRequest.DepositAmount;

            accountTodepoist.Transactions.Add(
                new Transaction
            {
                Amount = depositRequest.DepositAmount,
                Date = DateTime.Now,
                TrasactionType = TransactionType.Deposit,
                Account = accountTodepoist.Number,
                OldBalance = accountTodepoist.Balance - depositRequest.DepositAmount,
                NewBalance = accountTodepoist.Balance

            });

            CsvService.UpdateAccountInformation(accountTodepoist);

            return Ok(accountTodepoist);
        }

        [HttpPost("{id}/withdraw")]
        public ActionResult<Account> DepositToAccount(
            [FromRoute] int id,
            [FromBody] WithdrawRequest depositRequest)
        {
            var accountTodepoist = CsvService.GetAccountById(id);

            accountTodepoist.Balance -= depositRequest.WithdrawAmount;

            accountTodepoist.Transactions.Add(
               new Transaction
               {
                   Amount = depositRequest.WithdrawAmount,
                   Date = DateTime.Now,
                   TrasactionType = TransactionType.Withdraw,
                   Account = accountTodepoist.Number,
                   OldBalance = accountTodepoist.Balance + depositRequest.WithdrawAmount,
                   NewBalance = accountTodepoist.Balance

               });

            CsvService.UpdateAccountInformation(accountTodepoist);

            return Ok(accountTodepoist);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteById([FromRoute] int id)
        {
            CsvService.DeleteAccount(id);
            return Ok();
        }
    }
}
