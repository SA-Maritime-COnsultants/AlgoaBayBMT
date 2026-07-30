using AlgoaBayBMT.Emergency.OilSpill.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoaBayBMT.Emergency.OilSpill
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOilSpillModule(this IServiceCollection services)
        {
            services.AddSingleton<ICoastlineService, CoastlineService>();
            services.AddScoped<IIncidentFormService, IncidentFormService>();
            services.AddScoped<IOilSpillService, OilSpillService>();
            services.AddScoped<IOilSpillModelService, OilSpillModelService>();
            services.AddScoped<IOilSpillResponseService, OilSpillResponseService>();
            services.AddScoped<IOilSpillExportService, OilSpillExportService>();
            return services;
        }
    }
}
