namespace GamaEdtech.Presentation.ViewModel.Payment
{
    public sealed class PaymentsSummaryResponseViewModel
    {
        public DateOnly Date { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public long FailedCount { get; set; }
        public long PaidCount { get; set; }
        public long PendingCount { get; set; }
    }
}
