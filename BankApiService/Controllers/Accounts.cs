using BankApiService.CsvHelperService;
using BankApiService.Dependcies;
using BankApiService.Dependcies.LifeCycle;
using BankApiService.Enums;
using BankApiService.IdService;
using BankApiService.Models;
using BankApiService.Requests;
using BankApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Transaction = BankApiService.Models.Transaction;

namespace BankApiService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Accounts : ControllerBase
    {
        private readonly CsvService<Account> csvAccountService;
        private readonly CsvService<Transaction> csvTransactionService;
        private readonly ILogger<Accounts> _logger;

        // DB Sqlite
        private readonly IAccountsService _accountsService;


        public Accounts(
            CsvService<Account> csvService,
            CsvService<Transaction> csvService1,
            ILogger<Accounts> logger,
            IAccountsService accountsService)
        {
            csvAccountService = csvService;
            csvTransactionService = csvService1;
            _logger = logger;
            _accountsService = accountsService;
        }

        private const string _accountFileName = "accounts.csv";
        private const string _transactionFileName = "transactions.csv";
        private const string _accountIdFileName = "id.txt";
        private const string _transactionIdFileName = "t_id.txt";

        [HttpGet("ping")]
        public ActionResult Ping()
        {

            return Ok();
        }   


        [HttpGet]
        public ActionResult<List<Account>> GetAccounts()
        {
            _logger.LogWarning("Getting all accounts");

            try
            {
                var accountsList = _accountsService.GetAccounts();

                return Ok(accountsList);
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Account> GetAccountById([FromRoute] int id)
        {
            var account = _accountsService.GetAccountById(id);
            if (account == null)
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
            account.Owner = accountRequest.Owner;

            try
            {
                _accountsService.AddAccount(account);
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
            var accountTodepoist = csvAccountService.GetEntityById(id, _accountFileName);

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

            csvAccountService.UpdateEntityInformation(accountTodepoist, _accountFileName);
            csvTransactionService.WriteToCsv(new List<Transaction>() { transaction }, _transactionFileName);

            return Ok(accountTodepoist);
        }

        [HttpPost("{id}/withdraw")]
        public ActionResult<Account> DepositToAccount(
            [FromRoute] int id,
            [FromBody] WithdrawRequest depositRequest)
        {
            var accountTodepoist = csvAccountService.GetEntityById(id, _accountFileName);

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

            csvAccountService.UpdateEntityInformation(accountTodepoist, _accountFileName);
           csvTransactionService.WriteToCsv(new List<Transaction>() { transaction }, _transactionFileName);

            return Ok(accountTodepoist);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteById([FromRoute] int id)
        {
            csvAccountService.DeleteEntity(id, _accountFileName);
            return Ok();
        }

        [HttpPut("{id}")]
        public ActionResult UpdateOwnerName([FromRoute] int id, [FromBody] UpdateOwnerNameRequest updateRequest)
        {
            var account = csvAccountService.GetEntityById(id, _accountFileName);

            if (account.Id == -1)
            {
                return BadRequest($"Account with ID: {id} not found.");
            }

            account.Owner = updateRequest.Owner;

            csvAccountService.UpdateEntityInformation(account, _accountFileName);

            return Accepted();
        }

        [HttpPost("transfer")]
        public ActionResult Transfer(TransferRequest request)
        {
            var fromAccount = csvAccountService.GetEntityById(request.FromId, _accountFileName);
            var toAccount = csvAccountService.GetEntityById(request.ToId, _accountFileName);

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

            csvAccountService.UpdateEntityInformation(toAccount, _accountFileName);
            csvTransactionService.WriteToCsv(new List<Transaction>() { transationTo }, _transactionFileName);

            csvAccountService.UpdateEntityInformation(fromAccount, _accountFileName);
            csvTransactionService.WriteToCsv(new List<Transaction>() { transationFrom }, _transactionFileName);

            return Ok(fromAccount);
        }

       
    }
}
