using System;
using System.Collections.Generic;
using System.Linq;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace BankAccountStatementConverter
{
    public class RaibaAccountStatement : IAccountStatement
    {
        private int _accountNumber;
        private bool _accountNumberSuccess;

        private int _statementNumber;
        private bool _statementNumberSuccess;

        private int _statementYear;
        private bool _statementYearSuccess;

        private bool _tableStartSuccess;

        private DateTime _oldAccountBalanceDate;
        private bool _oldAccountBalanceDateSuccess;
        
        private double _oldAccountBalance;
        private bool _oldAccountBalanceSuccess;

        private DateTime _newAccountBalanceDate;
        private bool _newAccountBalanceDateSuccess;

        private double _newAccountBalance;
        private bool _newAccountBalanceSuccess;

        private int _transactionCount;
        private bool _transactionCountSuccess;

        private readonly List<ITransaction> _transactions = new List<ITransaction>();
        private bool _transactionsSuccess;

        public void ParseFromPdf(string fullFileName)
        {
            // Read PDF file
            var pdfDocument = PDDocument.load(fullFileName);
            var textStripper = new PDFTextStripper();
            var text = textStripper.getText(pdfDocument);
            pdfDocument.close();

            // Split text in lines
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Account number
            _accountNumberSuccess = 
                int.TryParse(lines[8].Trim(), out _accountNumber) && _accountNumber == 4111;

            // Statement number and year
            if (lines[9].Contains("/"))
            {
                var statementNumber = 
                    lines[9].Substring(0, lines[9].IndexOf("/", StringComparison.Ordinal)).Trim();
                _statementNumberSuccess = int.TryParse(statementNumber, out _statementNumber);

                var statementYear =
                    lines[9].Substring(lines[9].IndexOf("/", StringComparison.Ordinal) + 1).Trim();
                _statementYearSuccess = int.TryParse(statementYear, out _statementYear); 
            }

            // Table start
            _tableStartSuccess = lines[25] == "---------------------------------------------------";

            // Old balance date
            _oldAccountBalanceDateSuccess = lines[26].Contains("alter Kontostand vom") &&
                DateTime.TryParse(
                    lines[26].Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries).Last(), 
                    out _oldAccountBalanceDate);

            // Old balance value
            var oldAccountBalanceLineSplit = 
                lines[28].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            _oldAccountBalanceSuccess = 
                double.TryParse(oldAccountBalanceLineSplit.First(), out var oldAccountBalance);

            _oldAccountBalanceSuccess = TrySetSign(
                oldAccountBalanceLineSplit.Last(),
                out double sign);

            if (_oldAccountBalanceSuccess)
            {
                _oldAccountBalance = oldAccountBalance * sign;
            }
            
            // Transactions
            var transactions = new List<List<string>>();
            List<string> transaction = null;
            var newAccountBalanceLineNumber = -1;

            for (var index = 29; index < lines.Length; index++)
            {
                var line = lines[index];

                // Transaction start
                if (LineStartsWithTwoDates(line))
                {
                    if (transaction != null)
                    {
                        transactions.Add(transaction);
                    }
                    transaction = new List<string> { line };
                }
                // Transaction line
                else if (line.Length > 14 &&
                    line.StartsWith("              ") && 
                    line.Substring(14, 1) != " " && 
                    transaction != null)
                {
                    transaction.Add(line);
                }
                // End of transactions
                else if (line.Contains("neuer Kontostand vom"))
                {
                    newAccountBalanceLineNumber = index;
                    break;
                }
            }

            if (transaction != null)
            {
                transactions.Add(transaction);
            }

            // Add transactions
            foreach (var t in transactions)
            {
                switch (t[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last())
                {
                    case "H":
                        var deposit = new RaibaDeposit();
                        deposit.Parse(t, _statementYear);
                        _transactions.Add(deposit);
                        break;
                    case "S":
                        var withdrawal = new RaibaWithdrawal();
                        withdrawal.Parse(t, _statementYear);
                        _transactions.Add(withdrawal);
                        break;
                    default:
                        _transactionsSuccess = false;
                        break;
                }
            }

            // Verify transaction count
            _transactionCount = 0;
            foreach (var line in lines)
            {
                if (LineStartsWithTwoDates(line))
                {
                    _transactionCount++;
                }
            }
            _transactionCountSuccess = _transactions.Count == _transactionCount;

            // New balance
            var newAccountBalanceLineParts = lines[newAccountBalanceLineNumber]
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // New balance date
            _newAccountBalanceDateSuccess = DateTime.TryParse(
                newAccountBalanceLineParts[newAccountBalanceLineParts.Length -3], 
                out _newAccountBalanceDate);

            // New balance value
            _newAccountBalanceSuccess = double.TryParse(
                newAccountBalanceLineParts[newAccountBalanceLineParts.Length - 2], 
                out double newAccountBalance);

            _newAccountBalanceSuccess = TrySetSign(
                newAccountBalanceLineParts[newAccountBalanceLineParts.Length - 1],
                out sign);

            if (_newAccountBalanceSuccess)
            {
                _newAccountBalance = newAccountBalance * sign;
            }

            // Add balance value to each transaction
            var balance = _oldAccountBalance;
            foreach (var t in _transactions)
            {
                balance = t.AddBalance(balance);
            }
            if (_transactions.Last().Balance.Equals(_newAccountBalance) == false)
            {
                _transactionsSuccess = false;
            }
        }

        public IEnumerable<string[]> GetTransactionInfos()
        {
            return _transactions.Select(transaction => transaction.GetTransactionInfos());
        }

        public IEnumerable<string[]> GetGnuCashTransactionInfos()
        {
            return _transactions.Select(
                transaction => transaction.GetGnuCashTransactionInfos(
                    _statementNumber.ToString() + "-" + _statementYear.ToString()));
        }

        public string[] GetAccountStatementInfos()
        {
            return new []
            {
                _accountNumberSuccess.ToString(),
                _statementNumber.ToString(),
                _statementNumberSuccess.ToString(),
                _statementYear.ToString(),
                _statementYearSuccess.ToString(),
                _tableStartSuccess.ToString(),
                _oldAccountBalanceDate.ToString("yyyy-MM-dd"),
                _oldAccountBalanceDateSuccess.ToString(),
                $"{_oldAccountBalance:0.00}",
                _oldAccountBalanceSuccess.ToString(),
                _newAccountBalanceDate.ToString("yyyy-MM-dd"),
                _newAccountBalanceDateSuccess.ToString(),
                $"{_newAccountBalance:0.00}",
                _newAccountBalanceSuccess.ToString(),
                _transactionCount.ToString(),
                _transactionCountSuccess.ToString(),
                _transactionsSuccess.ToString()
            };
        }
        
        private static bool TrySetSign(string s, out double sign)
        {
            switch (s)
            {
                case "H":
                    sign = 1;
                    return true;
                case "S":
                    sign = -1;
                    return true;
                default:
                    sign = 0;
                    return false;
            }
        }

        private static bool LineStartsWithTwoDates(string line)
        {
            return line.Length > 14 &&
                line.Substring(2, 1) == "." && line.Substring(5, 2) == ". " &&
                line.Substring(9, 1) == "." && line.Substring(12, 2) == ". ";
        }
    }
}