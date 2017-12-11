using System.Collections.Generic;

namespace BankAccountStatementConverter
{
    /// <summary>
    /// Represents a bank account Statement
    /// </summary>
    public interface IAccountStatement
    {
        /// <summary>
        /// Reads a PDF File, converts its content to string and parses it
        /// </summary>
        /// <param name="fullFileName"></param>
        void ParseFromPdf(string fullFileName);

        /// <summary>
        /// Returns an enumerable of string arrays to create a CSV
        /// </summary>
        /// <returns></returns>
        IEnumerable<string[]> GetTransactionInfos();

        /// <summary>
        /// Returns an enumerable of string arrays to create a CSV for GnuCash
        /// </summary>
        /// <returns></returns>
        IEnumerable<string[]> GetGnuCashTransactionInfos();

        /// <summary>
        /// Returns Informations and about the account Statement as well as
        /// validations if everything was parsed correctly
        /// </summary>
        /// <returns></returns>
        string[] GetAccountStatementInfos();
    }
}
