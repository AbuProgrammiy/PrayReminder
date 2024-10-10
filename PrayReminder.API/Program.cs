
using PrayReminder.Application.Services.BackgroundServices;
using PrayReminder.Infrastructure;
using PrayReminder.Application;

namespace PrayReminder.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHostedService<MainBackgroundService>();

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();

            var app = builder.Build();
            
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
