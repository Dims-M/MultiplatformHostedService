using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Service.Models;
using Microsoft.Extensions.Logging;
using Service.Workers;
using Service.Services.TaskQueue;

namespace Service.Services
{
    /// <summary>
    /// Класс служба планировщика заданий 
    /// </summary>
    public class TaskSchedulerService : IHostedService, IDisposable // IHostedService Определяет методы для объектов, управляемых узлом, IDisposable - Предоставляет механизм для освобождения неуправляемых ресурсов. 
    {
        private Timer timer;
        private readonly IServiceProvider services;
        private readonly Settings settings;
        private readonly ILogger logger;
        private readonly Random random = new Random();
        private readonly object syncRoot = new object();

        /// <summary>
        /// Метод Срабатывает, когда хост приложения готов запустить службу.
        /// </summary>
        /// <param name="services">Указываем какой сервис нужно запустить</param>
        public TaskSchedulerService(IServiceProvider services)
        {
            this.services = services;
            this.settings = services.GetRequiredService<Settings>();
            this.logger = services.GetRequiredService<ILogger<TaskSchedulerService>>();
        }
        
        /// <summary>
        /// Запуск службы
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var interval = settings?.RunInterval ?? 0;

            if (interval == 0)
            {
                logger.LogWarning("checkInterval is not defined in settings. Set to default: 60 sec.");
                interval = 60;
            }

            timer = new Timer(
                (e) => ProcessTask(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(interval));

            return Task.CompletedTask; //Получает задачу, которая уже успешно завершена.
        }

        private void ProcessTask()
        {
            if (Monitor.TryEnter(syncRoot))
            {
                logger.LogInformation($"Process task started");

                for (int i = 0; i < 20; i++) DoWork();

                logger.LogInformation($"Process task finished");
                Monitor.Exit(syncRoot);
            }
            else
                logger.LogInformation($"Processing is currently in progress. Skipped");

        }

        private void DoWork()
        {
            var number = random.Next(20);

            var processor = services.GetRequiredService<TaskProcessor>();
            var queue = services.GetRequiredService<IBackgroundTaskQueue>();

            queue.QueueBackgroundWorkItem(token =>
            {
                return processor.RunAsync(number, token);
            });
        }

        /// <summary>
        /// Завершение работы сервиса
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Освобождает все ресурсы, используемые текущим экземпляром  System.Threading.Timer.
        /// </summary>
        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
