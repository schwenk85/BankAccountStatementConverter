using System;

namespace BankAccountStatementConverter
{
    public class Transaction
    {
        public DateTime Date { get; set; }

        public string Title { get; set; }

        public double Amount { get; set; }

        public string SenderOrRecipient { get; set; }

        public string Purpose { get; set; }

        public string[] GetTransactionInfos()
        {
            return new [] { Date.ToString("yyyy-MM-dd"), $"{Amount:0.00}", Title, SenderOrRecipient, Purpose };
        }
    }
}
