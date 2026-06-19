namespace GamaEdtech.Common.Data
{
    using GamaEdtech.Common.Core;

    public static class DbProviderFactories
    {
        public static DbProviderFactory GetFactory
        {
            get
            {
                if (field is not null)
                {
                    return field;
                }

                field = Globals.ProviderType switch
                {
                    DbProviderType.SqlServer => new SqlServerProvider(),
                    _ => throw new NotSupportedException(Globals.ProviderType.ToString()),
                };
                return field;
            }
        }
    }
}
