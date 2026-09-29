using System;
using System.Globalization;
using System.IO;

namespace Problem3_Bank
{
    /// <summary>
    /// Thrown when a withdrawal is larger than the account's current balance.
    /// </summary>
    public class InsufficientFundsException : Exception
    {
        /// <summary>
        /// Creates a new InsufficientFundsException.
        /// </summary>
        /// <param name="message">A description of the error.</param>
        public InsufficientFundsException(string message) : base(message) { }
    }

    /// <summary>
    /// Thrown when a transaction amount is invalid (zero or negative).
    /// </summary>
    public class InvalidAmountException : Exception
    {
        /// <summary>
        /// Creates a new InvalidAmountException.
        /// </summary>
        /// <param name="message">A description of the error.</param>
        public InvalidAmountException(string message) : base(message) { }
    }

    /// <summary>
    /// Defines deposit and withdraw operations for an account.
    /// </summary>
    public interface ITransactable
    {
        /// <summary>Deposits the given amount into the account.</summary>
        /// <param name="amount">The amount to deposit. Must be greater than zero.</param>
        void Deposit(double amount);

        /// <summary>Withdraws the given amount from the account.</summary>
        /// <param name="amount">The amount to withdraw. Must be greater than zero and no more than the balance.</param>
        void Withdraw(double amount);
    }

    /// <summary>
    /// Base class for bank accounts that support deposits, withdrawals, and interest calculation.
    /// </summary>
    public abstract class Account : ITransactable
    {
        /// <summary>The current balance of the account.</summary>
        public double Balance { get; protected set; }

        /// <summary>
        /// Creates an account with the given starting balance.
        /// </summary>
        /// <param name="startingBalance">The initial balance.</param>
        protected Account(double startingBalance)
        {
            Balance = startingBalance;
        }

        /// <summary>Calculates the interest projected for this account based on its current balance.</summary>
        public abstract double CalculateInterest();

        /// <inheritdoc/>
        /// <exception cref="InvalidAmountException">Thrown when amount is zero or negative.</exception>
        public void Deposit(double amount)
        {
            if (amount <= 0)
            {
                throw new InvalidAmountException($"Invalid amount '{amount}'");
            }
            Balance += amount;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidAmountException">Thrown when amount is zero or negative.</exception>
        /// <exception cref="InsufficientFundsException">Thrown when amount is greater than the balance.</exception>
        public void Withdraw(double amount)
        {
            if (amount <= 0)
            {
                throw new InvalidAmountException($"Invalid amount '{amount}'");
            }
            if (amount > Balance)
            {
                throw new InsufficientFundsException($"Insufficient funds (balance ${Balance:N2})");
            }
            Balance -= amount;
        }
    }

    /// <summary>A savings account that earns interest at a rate of 4%.</summary>
    public class SavingsAccount : Account
    {
        /// <summary>
        /// Creates a savings account with the given starting balance.
        /// </summary>
        /// <param name="startingBalance">The initial balance.</param>
        public SavingsAccount(double startingBalance) : base(startingBalance) { }

        /// <inheritdoc/>
        public override double CalculateInterest() => Balance * 0.04;
    }

    /// <summary>A checking account that earns interest at a rate of 1%.</summary>
    public class CheckingAccount : Account
    {
        /// <summary>
        /// Creates a checking account with the given starting balance.
        /// </summary>
        /// <param name="startingBalance">The initial balance.</param>
        public CheckingAccount(double startingBalance) : base(startingBalance) { }

        /// <inheritdoc/>
        public override double CalculateInterest() => Balance * 0.01;
    }

    /// <summary>
    /// Reads and applies a sequence of deposit/withdraw transactions to an account.
    /// </summary>
    public class BankAccountProcessor
    {
        /// <summary>
        /// The message from the most recent failed call to <see cref="TryProcessTransaction"/>,
        /// so Main can report exactly what went wrong without changing the method's signature.
        /// </summary>
        public static string? LastErrorMessage { get; private set; }

        /// <summary>
        /// Attempts to apply a single transaction to an account. Increments transactionCount
        /// on every attempt, whether it succeeds or fails.
        /// </summary>
        /// <param name="acct">The account to apply the transaction to.</param>
        /// <param name="type">'D' for deposit or 'W' for withdraw.</param>
        /// <param name="amount">The transaction amount.</param>
        /// <param name="newBalance">The account's balance after the attempt, whether it succeeded or not.</param>
        /// <param name="transactionCount">The running count of attempted transactions, incremented on every call.</param>
        /// <returns>True if the transaction succeeded; false otherwise.</returns>
        /// <exception cref="InvalidAmountException">Caught internally; reflected in the return value and LastErrorMessage.</exception>
        /// <exception cref="InsufficientFundsException">Caught internally; reflected in the return value and LastErrorMessage.</exception>
        public static bool TryProcessTransaction(Account acct, char type, double amount, out double newBalance, ref int transactionCount)
        {
            transactionCount++;
            try
            {
                if (type == 'D')
                {
                    acct.Deposit(amount);
                }
                else
                {
                    acct.Withdraw(amount);
                }
                newBalance = acct.Balance;
                return true;
            }
            catch (Exception ex) when (ex is InvalidAmountException || ex is InsufficientFundsException)
            {
                LastErrorMessage = ex.Message;
                newBalance = acct.Balance;
                return false;
            }
        }

        /// <summary>
        /// Entry point. Reads transactions.txt, applies each transaction to a SavingsAccount, and prints a report.
        /// Lines with an amount that can't be parsed are counted and reported without ever calling Deposit/Withdraw.
        /// </summary>
        public static void Main()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            Account account = new SavingsAccount(1000.00);
            Console.WriteLine($"=== Transaction Log (Starting Balance: ${account.Balance:N2}) ===");

            int transactionCount = 0;

            using (StreamReader reader = new StreamReader("transactions.txt"))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2)
                    {
                        transactionCount++;
                        Console.WriteLine($"[{transactionCount}] [ERROR] Invalid transaction line '{line}'");
                        continue;
                    }

                    char type = parts[0][0];
                    string amountText = parts[1];

                    if (!double.TryParse(amountText, out double amount))
                    {
                        transactionCount++;
                        Console.WriteLine($"[{transactionCount}] [ERROR] Invalid amount '{amountText}'");
                        continue;
                    }

                    string action = type == 'D' ? "Deposit" : "Withdraw";
                    bool success = TryProcessTransaction(account, type, amount, out double newBalance, ref transactionCount);

                    if (success)
                    {
                        Console.WriteLine($"[{transactionCount}] {action} ${amount:N2} -> Balance: ${newBalance:N2}");
                    }
                    else
                    {
                        Console.WriteLine($"[{transactionCount}] {action} ${amount:N2} -> [ERROR] {LastErrorMessage}");
                    }
                }
            }

            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine($"Transactions processed: {transactionCount}");
            Console.WriteLine($"Final Balance: ${account.Balance:N2}");
            Console.WriteLine($"Account Type: {account.GetType().Name} | Projected Interest: ${account.CalculateInterest():N2}");
        }
    }
}
