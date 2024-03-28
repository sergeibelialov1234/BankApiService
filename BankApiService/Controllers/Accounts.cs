using BankApiService.CsvHelperService;
using BankApiService.Enums;
using BankApiService.IdService;
using BankApiService.Models;
using BankApiService.Requests;
using Microsoft.AspNetCore.Mvc;
using Transaction = BankApiService.Models.Transaction;

namespace BankApiService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Accounts : ControllerBase
    {
        IConfiguration _configuration;

        public Accounts(IConfiguration configuration) 
        {
            _configuration = configuration;
            _accountFileName = _configuration["_accountFileName"];
        }

        private readonly string _accountFileName;
        private const string _transactionFileName = "transactions.csv";
        private const string _accountIdFileName = "id.txt";
        private const string _transactionIdFileName = "t_id.txt";

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
            var nextId = IdHelper.GetNextId(_accountIdFileName);
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
                Id = IdHelper.GetNextId(_transactionIdFileName),
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
                Id = IdHelper.GetNextId(_transactionIdFileName),
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

        [HttpPost("transfer")]
        public ActionResult Transfer(TransferRequest request)
        {
            var fromAccount = CsvService<Account>.GetEntityById(request.FromId, _accountFileName);
            var toAccount = CsvService<Account>.GetEntityById(request.ToId, _accountFileName);

            if(fromAccount.Id == -1 || toAccount.Id == -1)
            {
                return BadRequest("One of the accounts not found.");
            }

            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;

            fromAccount.Transactions = TransactionService.GetTransactionsById(fromAccount.Id, _transactionFileName);
            toAccount.Transactions = TransactionService.GetTransactionsById(toAccount.Id, _transactionFileName);

            var transationFrom = new Transaction
            {
                Id = IdHelper.GetNextId(_transactionIdFileName),
                Amount = request.Amount,
                Date = DateTime.Now,
                TrasactionType = TransactionType.Transfer,
                AccountId = fromAccount.Id,
                OldBalance = fromAccount.Balance + request.Amount,
                NewBalance = fromAccount.Balance
            };

            var transationTo = new Transaction
            {
                Id = IdHelper.GetNextId(_transactionIdFileName),
                Amount = request.Amount,
                Date = DateTime.Now,
                TrasactionType = TransactionType.Transfer,
                AccountId = toAccount.Id,
                OldBalance = toAccount.Balance - request.Amount,
                NewBalance = toAccount.Balance
            };

            toAccount.Transactions.Add(transationTo);
            fromAccount.Transactions.Add(transationFrom);

            CsvService<Account>.UpdateEntityInformation(toAccount, _accountFileName);
            CsvService<Transaction>.WriteToCsv(new List<Transaction>() { transationTo }, _transactionFileName);

            CsvService<Account>.UpdateEntityInformation(fromAccount, _accountFileName);
            CsvService<Transaction>.WriteToCsv(new List<Transaction>() { transationFrom }, _transactionFileName);

            return Ok(fromAccount);
        }

        [HttpGet("ping")]
        public ActionResult Ping()
        {
            var punginfor = _configuration.GetSection("Version:Major:T3est:asdasd:").Get<PingInformation>();


            return Ok(punginfor);
        }
    }
}
