namespace GamaEdtech.Common.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using GamaEdtech.Common.Core;
    using GamaEdtech.Common.DataAccess.UnitOfWork;

    using Microsoft.EntityFrameworkCore;

    public class LanguageService(Lazy<IUnitOfWorkProvider> unitOfWorkProvider) : ILanguageService
    {
        public IReadOnlyList<string?> GetActiveLanguages()
        {
            var uow = unitOfWorkProvider.Value.CreateUnitOfWork();
            var lst = uow.GetRepository<Language, int>().GetManyQueryable(t => t.IsEnable).OrderByDescending(t => t.IsDefault).Select(t => t.Code).ToList();

            return lst?.Count > 0 ? lst : [Constants.DefaultLanguageCode];
        }

        public async Task<IReadOnlyList<string?>> GetActiveLanguagesAsync()
        {
            var uow = unitOfWorkProvider.Value.CreateUnitOfWork();
            var lst = await uow.GetRepository<Language, int>().GetManyQueryable(t => t.IsEnable).OrderByDescending(t => t.IsDefault).Select(t => t.Code).ToListAsync();

            return lst?.Count > 0 ? lst : [Constants.DefaultLanguageCode];
        }
    }
}
