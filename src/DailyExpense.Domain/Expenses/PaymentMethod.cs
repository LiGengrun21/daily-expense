namespace DailyExpense.Domain.Expenses;

public enum PaymentMethod
{
    Unknown = 0,
    Cash = 1,
    DebitCard = 2,
    CreditCard = 3,
    BankTransfer = 4,
    DigitalWallet = 5,
    WeChatPay = 6,
}
