using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Extensions.HostExtensions;
using Service.Models;
using Service.Services;
using Service.Services.TaskQueue;
using Service.Workers;
using System;
using System.Threading.Tasks;

namespace Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder() //создание основного хоста. A program initialization utility.
                .ConfigureAppConfiguration(confBuilder => //конфигурация приложения Не хоста, не сервера
                {
                    confBuilder.AddJsonFile("config.json"); // Настройки конфигурации
                    confBuilder.AddCommandLine(args); // возможность обращение через командную строку
                })
                .ConfigureLogging((configLogging) => //Настройка логирования
                {
                    configLogging.AddConsole(); //Выводит логи  консоль
                    configLogging.AddDebug();  //Выводит логи в консоль выводв Visual st
                })
                .ConfigureServices((services) =>  //добавление доб возможностей. 
                //Внедрение зависимостей
                {
                    services.AddHostedService<TaskSchedulerService>();
                    services.AddHostedService<WorkerService>();

                    services.AddSingleton<Settings>(); //контейнер с оберткой. Для работы с файлом настроек config.json
                    services.AddSingleton<TaskProcessor>();
                    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
                });



            await builder.RunService(); //запуск сервиса
        }
    }
}
