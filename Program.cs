using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BankAccountStatementConverter
{
    public class Program
    {
        private static List<RaibaAccountStatement> _accountStatements;

        static void Main(string[] args)
        {
            _accountStatements = new List<RaibaAccountStatement>();

            ReadPdfs();
            CreateAccountStatementsCsv();
            CreateTransactionsCsv();
            CreateGnuCashTransactionsCsv();
            CreateHomeBankTransactionsCsv();
        }

        private static void ReadPdfs()
        {
            var pdfFullFileNames = Directory.GetFiles(
                Directory.GetCurrentDirectory(), "*.pdf", SearchOption.AllDirectories);

            foreach (var pdfFullFileName in pdfFullFileNames)
            {
                var accountStatement = new RaibaAccountStatement();
                accountStatement.ParseFromPdf(pdfFullFileName);
                _accountStatements.Add(accountStatement);
            }
        }

        private static void CreateAccountStatementsCsv()
        {
            var csvAccountStatementsFullFileName =
                Directory.GetCurrentDirectory() + @"\AccountStatements.csv";

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Join(
                ";",
                "accountNrOK",
                "statem.Nr",
                "statem.NrOK",
                "statem.Year",
                "statem.YearOK",
                "tableStartOK",
                "oldBal.Date",
                "oldBal.DateOK",
                "oldBal.",
                "oldBal.OK",
                "newBal.Date",
                "newBal.DateOK",
                "newBal.",
                "newBal.OK",
                "transactions",
                "trans.Count",
                "transactions.OK"
            ));

            foreach (var accountStatement in _accountStatements)
            {
                stringBuilder.AppendLine(string.Join(";", accountStatement.GetAccountStatementInfos()));
            }

            File.WriteAllText(csvAccountStatementsFullFileName, stringBuilder.ToString());
        }

        private static void CreateTransactionsCsv()
        {
            var csvTransactionsFullFileName = Directory.GetCurrentDirectory() + @"\Transactions.csv";

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Join(
                ";",
                "Date",
                "Amount",
                "Balance",
                "Title",
                "SenderOrRecipient",
                "Purpose"
            ));

            foreach (var accountStatement in _accountStatements)
            {
                foreach (var transactionInfo in accountStatement.GetTransactionInfos())
                {
                    stringBuilder.AppendLine(string.Join(";", transactionInfo));
                }
            }

            File.WriteAllText(csvTransactionsFullFileName, stringBuilder.ToString());
        }

        private static void CreateGnuCashTransactionsCsv()
        {
            var csvTransactionsFullFileName = 
                Directory.GetCurrentDirectory() + @"\TransactionsGnuCash.csv";

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Join(
                ";",
                "Datum",
                "Nr",
                "Beschreibung",
                "Bemerkung",
                "Einzahlung",
                "Abhebung",
                "Saldo"
            ));

            foreach (var accountStatement in _accountStatements)
            {
                foreach (var transactionInfo in accountStatement.GetGnuCashTransactionInfos())
                {
                    stringBuilder.AppendLine(string.Join(";", transactionInfo));
                }
            }

            File.WriteAllText(csvTransactionsFullFileName, stringBuilder.ToString());
        }

        private static void CreateHomeBankTransactionsCsv()
        {
            // file:///C:/Program%20Files%20(x86)/HomeBank/share/homebank/help/index.html

            var csvTransactionsFullFileName =
                Directory.GetCurrentDirectory() + @"\TransactionsHomeBank.csv";

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Join(
                ";",
                "date", // format must be DD-MM-YY
                "payment", // from 0=none to 10=FI fee
                "info", // a string
                "payee", // a payee name
                "memo", // a string
                "amount", // a number with a '.' or ',' as decimal separator, ex: -24.12 or 36,75
                "category", // a full category name (category, or category:subcategory)
                "tags" // tags separated by space; tag is mandatory since v4.5
            ));

            foreach (var accountStatement in _accountStatements)
            {
                foreach (var transactionInfo in accountStatement.GetHomeBankTransactionInfos())
                {
                    stringBuilder.AppendLine(string.Join(";", transactionInfo));
                }
            }

            File.WriteAllText(csvTransactionsFullFileName, stringBuilder.ToString());
        }
    }
}