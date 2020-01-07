using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Models;
using Service.Services.TaskQueue;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services
{
    //Класскоторый будет брать задачу из очереди и выполнять ее. Будет выполнятся пока работает приложение
    public class WorkerService : BackgroundService //  Базовый класс для реализации  Microsoft.Extensions.Hosting.IHostedService.
    {
        private readonly IBackgroundTaskQueue taskQueue; //делегат 
        private readonly ILogger<WorkerService> logger; //обьект для работы с логерром
        private readonly Settings settings;             // обьект настроек

        //конструктор. При вызове класса инициализируется
        public WorkerService(IBackgroundTaskQueue taskQueue, ILogger<WorkerService> logger, Settings settings)
        {
            this.taskQueue = taskQueue;
            this.logger = logger;
            this.settings = settings;
        }


        /// <summary>
        /// Метод выполняет основную работу
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken token) //token присылает хост при создании класса. завершение работы сервиса.()напр конт С
        {
            var workersCount = settings.WorkersCount; //чтение из файла загрузок
            var workers = Enumerable.Range(0, workersCount).Select(num => RunInstance(num, token)); // генерация workers и метод RunInstance с параметрами 

            await Task.WhenAll(workers); //запуск всех worker в отдельных потоках

        }

        private async Task RunInstance(int num, CancellationToken token)
        {
            logger.LogInformation($"#{num} is starting.");

            while(!token.IsCancellationRequested)
            {
                var workItem = await taskQueue.DequeueAsync(token);

                try
                {
                    logger.LogInformation($"#{num}: Processing task. Queue size: {taskQueue.Size}.");
                    await workItem(token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"#{num}: Error occurred executing task.");
                }
            }

            logger.LogInformation($"#{num} is stopping.");
        }
    }
}
