using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BankAccountStatementConverter
{
    public class Program
    {
        static void Main(string[] args)
        {
            // PDFs einlesen
            var pdfFullFileNames = Directory.GetFiles(
                Directory.GetCurrentDirectory(), "*.pdf", SearchOption.AllDirectories);

            var accountStatements = new List<RaibaAccountStatement>();

            foreach (var pdfFullFileName in pdfFullFileNames)
            {
                var accountStatement = new RaibaAccountStatement();
                accountStatement.TryParseFromPdf(pdfFullFileName);
                accountStatements.Add(accountStatement);
            }

            // Transactions CSV erzeugen
            var csvAccountStatementsFullFileName = Directory.GetCurrentDirectory() + @"\AccountStatements.csv";

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
                "trans.Count.OK"
            ));
            foreach (var accountStatement in accountStatements)
            {
                stringBuilder.AppendLine(string.Join(";", accountStatement.GetAccountStatementInfos()));
            }

            File.WriteAllText(csvAccountStatementsFullFileName, stringBuilder.ToString());

            // Transactions CSV erzeugen
            var csvTransactionsFullFileName = Directory.GetCurrentDirectory() + @"\Transactions.csv";
            
            stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Join(";", "Date", "Amount", "Title", "SenderOrRecipient", "Purpose"));
            foreach (var accountStatement in accountStatements)
            {
                foreach (var transactionInfo in accountStatement.GetTransactionInfos())
                {
                    stringBuilder.AppendLine(string.Join(";", transactionInfo));
                }
            }
            
            File.WriteAllText(csvTransactionsFullFileName, stringBuilder.ToString());
        }
    }
}
