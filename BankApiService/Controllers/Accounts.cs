using BankApiService.CsvHelperService;
using BankApiService.Enums;
using BankApiService.IdService;
using BankApiService.Models;
using BankApiService.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;

namespace BankApiService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Accounts : ControllerBase
    {

        private const string _accountFileName = "accounts.csv";
        private const string _transactionFileName = "transactions.csv";

        [HttpGet]
        public ActionResult<List<Account>> GetAccounts()
        {
            try
            {
                var accountList = CsvService<Account>.ReadFromCsv(_accountFileName);
       
                foreach (var account in accountList)
                {
                    account.Transactions = TransactionService.GetTransactionsById(account.Id, _transactionFileName);
                }

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
            var account = CsvService<Account>.GetEntityById(id, _accountFileName);

            if (account.Id == -1)
            {
                return BadRequest($"Account with ID: {id} not found.");
            }

            account.Transactions = TransactionService.GetTransactionsById(account.Id, _transactionFileName);

            return Ok(account);
        }

        [HttpPost]
        public ActionResult<Account> CreateAccout([FromBody] CreateAccountRequest accountRequest)
        {
            var random = new Random();

            var account = new Account();

            account.Number = random.Next(100, 99999);
            var nextId = IdHelper.GetNextId();
            account.Owner = accountRequest.Owner;
            account.Id = nextId;

            var listAccounts = new List<Account>();
            listAccounts.Add(account);

            try
            {
                CsvService<Account>.WriteToCsv(listAccounts, _accountFileName);
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
            var accountTodepoist = CsvService<Account>.GetEntityById(id, _accountFileName);

            accountTodepoist.Balance += depositRequest.Amount;

            if (accountTodepoist.Id == -1)
            {
                return BadRequest($"Account with ID: {id} not found.");
            }

            accountTodepoist.Transactions = TransactionService.GetTransactionsById(accountTodepoist.Id, _transactionFileName);

            var transaction = new Transaction
            {
                Id = IdHelper.GetNextTransactionId(),
                Amount = depositRequest.Amount,
                Date = DateTime.Now,
                TrasactionType = TransactionType.Deposit,
                AccountId = accountTodepoist.Id,
                OldBalance = accountTodepoist.Balance - depositRequest.Amount,
                NewBalance = accountTodepoist.Balance
            };

            accountTodepoist.Transactions.Add(transaction);

            CsvService<Account>.UpdateEntityInformation(accountTodepoist, _accountFileName);
            CsvService<Transaction>.WriteToCsv(new List<Transaction>() { transaction }, _transactionFileName);

            return Ok(accountTodepoist);
        }

        [HttpPost("{id}/withdraw")]
        public ActionResult<Account> DepositToAccount(
            [FromRoute] int id,
            [FromBody] WithdrawRequest depositRequest)
        {
            var accountTodepoist = CsvService<Account>.GetEntityById(id, _accountFileName);

            accountTodepoist.Balance -= depositRequest.Amount;

            if (accountTodepoist.Id == -1)
            {
                return BadRequest($"Account with ID: {id} not found.");
            }

            accountTodepoist.Transactions = TransactionService.GetTransactionsById(accountTodepoist.Id, _transactionFileName);

            var transaction = new Transaction
            {
                Id = IdHelper.GetNextTransactionId(),
                Amount = depositRequest.Amount,
                Date = DateTime.Now,
                TrasactionType = TransactionType.Withdraw,
                AccountId = accountTodepoist.Id,
                OldBalance = accountTodepoist.Balance + depositRequest.Amount,
                NewBalance = accountTodepoist.Balance

            };

            accountTodepoist.Transactions.Add(transaction);

            CsvService<Account>.UpdateEntityInformation(accountTodepoist, _accountFileName);
            CsvService<Transaction>.WriteToCsv(new List<Transaction>() { transaction }, _transactionFileName);

            return Ok(accountTodepoist);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteById([FromRoute] int id)
        {
            CsvService<Account>.DeleteEntity(id, _accountFileName);
            return Ok();
        }

        [HttpPut("{id}")]
        public ActionResult UpdateOwnerName([FromRoute] int id, [FromBody] UpdateOwnerNameRequest updateRequest)
        {
            var account = CsvService<Account>.GetEntityById(id, _accountFileName);

            if (account.Id == -1)
            {
                return BadRequest($"Account with ID: {id} not found.");
            }

            account.Owner = updateRequest.Owner;

            CsvService<Account>.UpdateEntityInformation(account, _accountFileName);

            return Accepted();
        }
    }
}
