namespace GamaEdtech.Data.Dto.Provider.CurrencyConverter
{
    public sealed class ConvertRequestDto
    {
        public required decimal Amount { get; set; }
        public string? Mint { get; set; }
    }
}
