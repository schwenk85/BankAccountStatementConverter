using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace BankAccountStatementConverter
{
    public class RaibaAccountStatement
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

        public RaibaAccountStatement()
        {
            Transactions = new List<Transaction>();
        }

        private List<Transaction> Transactions { get; }

        public bool TryParseFromPdf(string fullFileName)
        {
            var pdfDocument = PDDocument.load(fullFileName);
            var textStripper = new PDFTextStripper();
            var text = textStripper.getText(pdfDocument);
            pdfDocument.close();

            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Kontonummer
            _accountNumberSuccess = int.TryParse(lines[8].Trim(), out _accountNumber) && _accountNumber == 4111;

            // Kontoauszug Nummer und Jahr
            if (lines[9].Contains("/"))
            {
                var statementNumber = lines[9].Substring(0, lines[9].IndexOf("/", StringComparison.Ordinal));
                _statementNumberSuccess = int.TryParse(statementNumber.Trim(), out _statementNumber);

                var statementYear =
                    lines[9].Substring(lines[9].IndexOf("/", StringComparison.Ordinal) + 1);
                _statementYearSuccess = int.TryParse(statementYear.Trim(), out _statementYear); 
            }

            // Start der Tabelle
            _tableStartSuccess = lines[25] == "---------------------------------------------------";

            // Alter Kontostand Datum
            _oldAccountBalanceDateSuccess = DateTime.TryParse(
                lines[26].Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries).Last(), 
                out _oldAccountBalanceDate);

            // Alter Kontostand
            var oldAccountBalanceLineSplit = lines[28].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            _oldAccountBalanceSuccess = double.TryParse(oldAccountBalanceLineSplit.First(), out double oldAccountBalance);

            _oldAccountBalanceSuccess = TrySetSign(
                oldAccountBalanceLineSplit.Last(),
                out double sign);

            if (_oldAccountBalanceSuccess)
            {
                _oldAccountBalance = oldAccountBalance * sign;
            }
            
            // Transaktionen
            Transaction transaction = null;
            var newAccountBalanceLineNumber = -1;
            
            for (var index = 29; index < lines.Length; index++)
            {
                var line = lines[index];

                // Transaktion Start
                if (LineStartsWithTwoDates(line))
                {
                    if (transaction != null)
                    {
                        if (transaction.Purpose != null)
                        {
                            transaction.Purpose = Regex.Replace(transaction.Purpose, @"\s+", " ");
                        }
                        Transactions.Add(transaction);
                    }
                    transaction = new Transaction();

                    var lineParts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // Datum
                    if (DateTime.TryParse(lineParts[0] + _statementYear, out DateTime date))
                    {
                        transaction.Date = date;
                    }

                    // Betrag
                    if (double.TryParse(lineParts[lineParts.Length - 2], out double amount) &&
                        TrySetSign(lineParts.Last(), out sign))
                    {
                        transaction.Amount = amount * sign;
                    }

                    // Titel
                    string title = null;
                    for (var i = 2; i < lineParts.Length - 2; i++)
                    {
                        title += lineParts[i] + " ";
                    }
                    transaction.Title = title?.Trim();
                }
                else if (line.Length > 14 &&
                    line.StartsWith("              ") && 
                    line.Substring(14, 1) != " " && 
                    transaction != null)
                {
                    if (transaction.SenderOrRecipient == null)
                    {
                        transaction.SenderOrRecipient = line.Trim();
                    }
                    else
                    {
                        transaction.Purpose += line.Trim();
                    }
                }
                else if (line.Contains("neuer Kontostand vom"))
                {
                    newAccountBalanceLineNumber = index;

                    if (transaction != null)
                    {
                        Transactions.Add(transaction);
                    }

                    break;
                }
            }

            _transactionCount = 0;
            foreach (var line in lines)
            {
                if (LineStartsWithTwoDates(line))
                {
                    _transactionCount++;
                }
            }
            _transactionCountSuccess = Transactions.Count == _transactionCount;

            var newAccountBalanceLineParts = lines[newAccountBalanceLineNumber]
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Neuer Kontostand Datum
            _newAccountBalanceDateSuccess = DateTime.TryParse(
                newAccountBalanceLineParts[newAccountBalanceLineParts.Length -3], 
                out _newAccountBalanceDate);

            // Neuer Kontostand
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
            
            return true;
        }

        public IEnumerable<string[]> GetTransactionInfos()
        {
            return Transactions.Select(transaction => transaction.GetTransactionInfos());
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
                _transactionCountSuccess.ToString()
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
