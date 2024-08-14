using Microsoft.EntityFrameworkCore;
using System.Net;
using WebApplication1.Models;

namespace WebApplication1
{
    public class Startup
    {
        string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
       string ConnectionString = @"";
        public void ConfigureServices(IServiceCollection services)
        {
            // устанавливаем контекст данных
            //services.AddDbContext<UsersContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 11))));
            services.AddDbContext<UsersContext>(options => options.UseSqlServer(ConnectionString));
            services.AddDbContext<ScheduleContext>(options => options.UseSqlServer(ConnectionString));
            services.AddDbContext<ProgrammContext>(options => options.UseSqlServer(ConnectionString));
            services.AddDbContext<LabContext>(options => options.UseSqlServer(ConnectionString));
            services.AddControllers(); // используем контроллеры без представлений
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      policy.WithOrigins("http://web-project.somee.com/project",
                                                          "http://web-project.somee.com/project");
                                  });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // подключаем маршрутизацию на контроллеры
            });
        }
    }
}

