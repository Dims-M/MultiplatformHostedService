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
        private readonly IServiceProvider services; //сервис для чтения конфигурации настроек
        private readonly Settings settings;         //получение  настроек через клас обертку Settings
        private readonly ILogger logger;            //обьект для работы с логом
        private readonly Random random = new Random();
        private readonly object syncRoot = new object(); //обьек для работы с калассом монитора в методе ProcessTask

        /// <summary>
        /// Метод чтения конфигурациин настроек config.json.
        /// </summary>
        /// <param name="services">Указываем какой сервис нужно запустить</param>
        public TaskSchedulerService(IServiceProvider services)
        {
            this.services = services; 
            this.settings = services.GetRequiredService<Settings>();  //получение  настроек через клас обертку Settings
            this.logger = services.GetRequiredService<ILogger<TaskSchedulerService>>(); //логирование
        }
        
        /// <summary>
        /// Запуск службы
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            
            var interval = settings?.RunInterval ?? 0; //чтение настроек конфиг config.json
           
            if (interval == 0)
            {
                logger.LogWarning("Интервал проверки в настройках не определен. Установлено значение по умолчанию: 60 сек.");
                interval = 60;
            }

            timer = new Timer( //обьект таймера
                (e) => ProcessTask(), // запуск метода( (e)) выполнения
                null, // состоянте обьекта
                TimeSpan.Zero, // ожидания запуск.  Zero = значит запускаем сразу
                TimeSpan.FromSeconds(interval)); // интервал запуска. Будет братся с конфига
            //заглушка
            return Task.CompletedTask; //Получает задачу, которая уже успешно завершена. 
        }

        /// <summary>
        /// Метод запуска
        /// </summary>
        private void ProcessTask()
        {
            //проверка. Должна выполнятся только одна копия метода(задачи)
            if (Monitor.TryEnter(syncRoot))  //Обеспечивает механизм синхронизации доступа к объектам.
            {
                logger.LogInformation($"{DateTime.Now} Начался процесс задач");

                for (int i = 0; i < 20; i++) DoWork();

                logger.LogInformation($"{DateTime.Now} Процесс задач остановился");
                Monitor.Exit(syncRoot); 
            }
            else
                logger.LogInformation($"{DateTime.Now}  Processing is currently in progress. Skipped");

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
            ///при нажатии кнтр С остановка работы таймера
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
