using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class ISGOTTTemplateService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IISGOTTTemplateService
    {
        public async Task<List<ISGOTTStageTemplate>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.ISGOTTStageTemplates
                .AsNoTracking()
                .Include(x => x.Items.Where(i => i.IsActive))
                .Where(x => x.IsActive)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
