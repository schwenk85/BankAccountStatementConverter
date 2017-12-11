namespace BankAccountStatementConverter
{
    /// <inheritdoc />
    /// <summary>
    /// A drawing / Abhebung
    /// </summary>
    public interface IWithdrawal : ITransaction
    {
        /// <summary>
        /// The recipient of the withdrawal
        /// </summary>
        string Recipient { get; set; }
    }
}
