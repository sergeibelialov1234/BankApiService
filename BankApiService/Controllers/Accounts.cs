using BankApiService.Models;
using BankApiService.Requests;
using BankApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Transaction = BankApiService.Models.Transaction;
using BankApiService.Enums;


namespace BankApiService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class Accounts : ControllerBase
    {
        private readonly ILogger<Accounts> _logger;

        // DB Sqlite
        private readonly IAccountsService _accountsService;
        private readonly ITransactionService _transactionService;

        public Accounts(
            ILogger<Accounts> logger,
            IAccountsService accountsService,
            ITransactionService transactionService)
        {
            _logger = logger;
            _accountsService = accountsService;
            _transactionService = transactionService;
        }

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
            try
            {
                var accountTodepoist = _accountsService.GetAccountById(id);

                if (accountTodepoist == null)
                {
                    return BadRequest($"Account with ID: {id} not found.");
                }

                accountTodepoist.Balance += depositRequest.Amount;

                var transaction = new Transaction
                {
                    Amount = depositRequest.Amount,
                    Date = DateTime.Now,
                    TrasactionType = TransactionType.Deposit,
                    AccountId = accountTodepoist.Id,
                    OldBalance = accountTodepoist.Balance - depositRequest.Amount,
                    NewBalance = accountTodepoist.Balance
                };

                accountTodepoist.Transactions.Add(transaction);

                _accountsService.UpdateAccount(accountTodepoist);
                //_transactionService.AddTransaction(transaction);

                return Ok(accountTodepoist);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost("{id}/withdraw")]
        public ActionResult<Account> DepositToAccount(
            [FromRoute] int id,
            [FromBody] WithdrawRequest depositRequest)
        {

            var accountWithdrawFrom = _accountsService.GetAccountById(id);

            if (accountWithdrawFrom == null)
            {
                return BadRequest($"Account with ID: {id} not found.");
            }

            accountWithdrawFrom.Balance -= depositRequest.Amount;

            var transaction = new Transaction
            {
                Amount = depositRequest.Amount,
                Date = DateTime.Now,
                TrasactionType = TransactionType.Withdraw,
                AccountId = accountWithdrawFrom.Id,
                OldBalance = accountWithdrawFrom.Balance + depositRequest.Amount,
                NewBalance = accountWithdrawFrom.Balance

            };

            _accountsService.UpdateAccount(accountWithdrawFrom);
            _transactionService.AddTransaction(transaction);

            return Ok(accountWithdrawFrom);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteById([FromRoute] int id)
        {
            
            var operationResult = _accountsService.DeleteAccount(id);

            if (operationResult == OperationResult.Failure)
            {
                return BadRequest($"Account with ID: {id} not found.");
            }

            return Ok();
        }

        [HttpPut("{id}")]
        public ActionResult UpdateOwnerName([FromRoute] int id, [FromBody] UpdateOwnerNameRequest updateRequest)
        {
            var result = _accountsService.UpdateOwnerName(id, updateRequest.Owner);

            if (result == OperationResult.Failure)
            {
                return BadRequest($"Account with ID: {id} not found.");
            }

            return Accepted();
        }

        [HttpPost("transfer")]
        public ActionResult Transfer(TransferRequest request)
        {
            var fromAccount = _accountsService.GetAccountById(request.FromId);
            var toAccount = _accountsService.GetAccountById(request.ToId);

            if(fromAccount == null || toAccount == null)
            {
                return BadRequest("One of the accounts not found.");
            }

            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;

            var transationFrom = new Transaction
            {
                Amount = request.Amount,
                Date = DateTime.Now,
                TrasactionType = TransactionType.Transfer,
                AccountId = fromAccount.Id,
                OldBalance = fromAccount.Balance + request.Amount,
                NewBalance = fromAccount.Balance
            };

            var transationTo = new Transaction
            {
                Amount = request.Amount,
                Date = DateTime.Now,
                TrasactionType = TransactionType.Transfer,
                AccountId = toAccount.Id,
                OldBalance = toAccount.Balance - request.Amount,
                NewBalance = toAccount.Balance
            };

           _accountsService.UpdateAccount(fromAccount);
            _accountsService.UpdateAccount(toAccount);

            _transactionService.AddTransaction(transationFrom);
            _transactionService.AddTransaction(transationTo);

            return Ok(fromAccount);
        }

       
    }
}
