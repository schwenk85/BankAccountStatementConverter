using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BankAccountStatementConverter
{
    public class RaibaDeposit : IDeposit
    {
        public DateTime Date { get; set; }

        public string Title { get; set; }

        public double Amount { get; set; }

        public double Balance { get; set; }

        public string Purpose { get; set; }

        public string Sender { get; set; }

        public void Parse(List<string> content, int year)
        {
            if (content.Count > 0)
            {
                var header = content[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Date
                if (DateTime.TryParse(header[0] + year, out var date))
                {
                    Date = date;
                }

                // Title
                string title = null;
                for (var i = 2; i < header.Length - 2; i++)
                {
                    title += header[i] + " ";
                }
                Title = title?.Trim();

                // Amount
                if (double.TryParse(header[header.Length - 2], out var amount))
                {
                    Amount = amount;
                }
            }

            if (content.Count > 1)
            {
                // Sender
                Sender = content[1].Trim();
            }

            if (content.Count > 2)
            {
                // Purpose
                string purpose = null;
                for (var i = 2; i < content.Count; i++)
                {
                    purpose += content[i];
                }
                if (purpose != null)
                {
                    // Replace multiple whitespaces with one space, 
                    // then trim whitespaces at the beginning and end
                    Purpose = Regex.Replace(purpose, @"\s+", " ").Trim();
                }
            }
        }

        public double AddBalance(double oldBalance)
        {
            Balance = oldBalance + Amount;
            return Balance;
        }

        public string[] GetTransactionInfos()
        {
            return new[]
            {
                Date.ToString("yyyy-MM-dd"),
                $"{Amount:0.00}",
                $"{Balance:0.00}",
                Title,
                Sender,
                Purpose
            };
        }

        public string[] GetGnuCashTransactionInfos(string number)
        {
            // Date, Number, Description, Remark, Account, Deposit, Withdrawal, Balance
            return new[]
            {
                Date.ToString("yyyy-MM-dd"),
                number,
                Sender + " - " + Title,
                Purpose,
                $"{Amount:0.00}",
                $"{0:0.00}",
                $"{Balance:0.00}"
            };
        }

        public string[] GetHomeBankTransactionInfos()
        {
            // date, payment, info, payee, memo, amount, category, tags
            return new[]
            {
                Date.ToString("dd-MM-yy"),
                string.Empty, //TODO
                Title,
                Sender,
                Purpose,
                $"{Amount:0.00}",
                string.Empty, //TODO
                $"Kontrolle:{Balance:0.00}€"
            };
        }
    }
}
