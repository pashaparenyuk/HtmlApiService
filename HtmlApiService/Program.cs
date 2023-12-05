
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Http.Features;

namespace HtmlApiService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddHangfire(config => config.UseMemoryStorage());
            builder.Services.AddHangfireServer();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors();
            builder.Services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
            });
            app.UseCors("AllowAnyOrigin");
            app.UseHttpsRedirection();
            app.MapControllers();
            app.UseAuthorization();
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            app.Run();
        }
    }
}