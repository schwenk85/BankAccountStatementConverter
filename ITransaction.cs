using System;
using System.Collections.Generic;

namespace BankAccountStatementConverter
{
    /// <summary>
    /// Representation of a bank transaction
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// The transaction date
        /// </summary>
        DateTime Date { get; set; }

        /// <summary>
        /// The transaction title
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The amount of money which is received or withdrawn
        /// </summary>
        double Amount { get; set; }
        
        /// <summary>
        /// The actual balance of the bank accout after the amount is added or subtracted / Saldo
        /// </summary>
        double Balance { get; set; }

        /// <summary>
        /// The reason of the money transfer
        /// </summary>
        string Purpose { get; set; }

        /// <summary>
        /// Parses a list of strings to transaction content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="year"></param>
        void Parse(List<string> content, int year);

        /// <summary>
        /// Adds/Substracts the transaction amount to the old balance. 
        /// Saves and returns the new balance.
        /// </summary>
        /// <param name="oldBalance">old balance</param>
        /// <returns>new balance</returns>
        double AddBalance(double oldBalance);

        /// <summary>
        /// Returns a string array to create a CSV
        /// </summary>
        /// <returns></returns>
        string[] GetTransactionInfos();

        /// <summary>
        /// Returns a string array to create a CSV for GnuCash
        /// </summary>
        /// <returns></returns>
        string[] GetGnuCashTransactionInfos(string number);

        /// <summary>
        /// Returns a string array to create a CSV for HomeBank
        /// "date"     format must be DD-MM-YY
        /// "payment"  from 0=none to 10=FI fee
        /// "info"     a string
        /// "payee"    a payee name
        /// "memo"     a string
        /// "amount"   a number with a '.' or ',' as decimal separator, ex: -24.12 or 36,75
        /// "category" a full category name (category, or category:subcategory)
        /// "tags"     tags separated by space; tag is mandatory since v4.5
        /// </summary>
        /// <returns>
        /// example:
        /// 15-02-04;0;;;Some cash;-40,00;Bill:Withdrawal of cash;tag1
        /// 15-02-04;1;;;Internet DSL;-45,00;Inline service/Internet;tag2
        /// </returns>
        string[] GetHomeBankTransactionInfos();
    }
}