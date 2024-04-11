using BankApiService.Context;
using BankApiService.Enums;
using BankApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApiService.Services
{
    public interface IAccountsService
    {
        public Account CreateAccount(string accountOwner);
        public OperationResult UpdateOwnerName(int id, string ownerName);
        public OperationResult DeleteAccount(int id);
        public Account GetAccountById(int id);
        public List<Account> GetAccounts();
        void UpdateAccount(Account account);
    }

    public class AccountsService : IAccountsService
    {
        private readonly BankContext _context;
        private readonly Random random = new Random();

        public AccountsService(BankContext context)
        {
            _context = context;
        }

        public Account CreateAccount(string accountOwner)
        {
            var account = new Account();

            account.Number = random.Next(100, 99999);
            account.Owner = accountOwner;

            _context.Accounts.Add(account);
            _context.SaveChanges();

            return account;
        }

        public OperationResult DeleteAccount(int id)
        {
            var account = _context.Accounts.FirstOrDefault(x => x.Id == id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                _context.SaveChanges();
                return OperationResult.Success;
            }

           return OperationResult.Failure;
        }

        public void UpdateAccount(Account account)
        {
            _context.Accounts.Update(account);
            _context.SaveChanges();
        }

        public Account GetAccountById(int id)
        {
            var account = _context.Accounts
                .Include(x => x.Transactions)
                .FirstOrDefault(x => x.Id == id);
            return account;
        }

        public List<Account> GetAccounts()
        {
           var result = _context.Accounts
                .Include(x => x.Transactions)
                .ToList();
            return result;
        }

        public OperationResult UpdateOwnerName(int id, string ownerName)
        {
            var account = _context.Accounts.FirstOrDefault(x => x.Id == id);
            if (account == null)
            {
                return OperationResult.Failure;
            }

            account.Owner = ownerName;

            _context.Accounts.Update(account);
            _context.SaveChanges();

            return OperationResult.Success;
        }
    }
}
