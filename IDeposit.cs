namespace BankAccountStatementConverter
{
    /// <inheritdoc />
    /// <summary>
    /// An incoming payment / Einzahlung
    /// </summary>
    public interface IDeposit : ITransaction
    {
        /// <summary>
        /// The sender of the deposit
        /// </summary>
        string Sender { get; set; }
    }
}
