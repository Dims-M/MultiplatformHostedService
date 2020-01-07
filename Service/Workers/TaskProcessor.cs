using Microsoft.Extensions.Logging;
using Service.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Workers
{
    /// <summary>
    /// Бизнец логика работы сервиса
    /// </summary>
    public class TaskProcessor
    {
        private readonly ILogger<TaskProcessor> logger;
        private readonly Settings settings;

        //конструктор для получения логера и настроек работы
        public TaskProcessor(ILogger<TaskProcessor> logger, Settings settings)
        {
            this.logger = logger;
            this.settings = settings;
        }


        public async Task RunAsync(int number, CancellationToken token) //Распространяет уведомление о том, что операции должны быть отменены.
        {
            token.ThrowIfCancellationRequested(); // отмена операции

           
            Func<int, int> fibonacci = null;
            //Функция для расчета фибоначи
            fibonacci = (num) =>
            {
                if (num < 2) return 1;
                else return fibonacci(num - 1) + fibonacci(num - 2);
            };

            var result = await Task.Run(async () => //запуск в  Task.Run для абоы в отдельном потоке
            {
                await Task.Delay(1000);
                return Enumerable.Range(0, number).Select(n => fibonacci(n));
            }, token);

            //Запись результата в файл на жестком диске
            using (var writer = new StreamWriter(settings.ResultPath, true, Encoding.UTF8))
            {
                writer.WriteLine(DateTime.Now.ToString() + " : " + string.Join(" ", result));
            }

            logger.LogInformation($"Задача заверщилась. Резульат: {string.Join(" ", result)}");
        }
    }
}
