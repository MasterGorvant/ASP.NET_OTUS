using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PromoCodeFactory.DataAccess.Repositories.EF;

namespace PromoCodeFactory.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DbContext>();


                //����������������� ��� ������
                //db.Database.EnsureDeletedAsync();
                //Console.WriteLine("Database deleted");
                //db.Database.EnsureCreatedAsync();
                //Console.WriteLine("Database created");

                //���������������� ��� ������
                //������ ��� �������� ���� ������: https://www.learnentityframeworkcore.com/migrations/add-migration
                db.Database.Migrate();
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}