using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Service.Models;
using Microsoft.Extensions.Logging;
using Service.Workers;
using Service.Services.TaskQueue;
using System.IO;

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
        private readonly Random random = new Random(); //рандрмный генератор. для метода  DoWork()
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

                //с помощью цикла запускаем 20 задач одновременно
                for (int i = 0; i < 20; i++) DoWork();

                logger.LogInformation($"{DateTime.Now} Процесс задач остановился");
              //  throw new Exception("Длина строки больше 6 символов");//Искуcтвенная ошибка для тестировани
                Monitor.Exit(syncRoot); 
            }
            else //Лог ошибки
            {
                string ResultPathERRor = "C:\\ErrorResult.txt";
                logger.LogInformation($"{DateTime.Now}  В настоящее время идет процесс обработки. Skipped");
                //Запись результата в файл на жестком диске
                using (StreamWriter writer = new StreamWriter(ResultPathERRor, true, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(DateTime.Now.ToString() + $"Что то пошло не так....{logger.ToString()}");// + string.Join(" ")); ;
                }
            }
               

        }

        //Задача для выволнения в очереди сервиса
        private void DoWork()
        {
            var number = random.Next(20); //имитация работы с помощю рандома

            var processor = services.GetRequiredService<TaskProcessor>(); // подключаем нужный класс с логикой работы
            var queue = services.GetRequiredService<IBackgroundTaskQueue>(); // очередь для задачь. Нужная задача будет запускатся только в одном экземпляре

            queue.QueueBackgroundWorkItem(token =>
            {
                return processor.RunAsync(number, token); // запускаем наш рабочий класс с логикой на выполнение. Получаем готовый результат
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
