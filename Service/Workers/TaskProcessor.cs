using Microsoft.Extensions.Logging;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly ILogger<TaskProcessor> logger; // обект логер
        private readonly Settings settings; // настройки работы 
        private static int countProcesse = 0;  //количество запущеных процессов

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

            await Task.Run(async () =>
            {
                await Task.Delay(1000);
                getCompProcesse(); // получение списка запущеных процеесов
            });


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
            //ывод в лог
              logger.LogInformation($"Задача заверщилась. Резульат: {string.Join(" ", result)}");
        }


        /// <summary>
        /// Получение списка запущенных процессов
        /// </summary>
        /// <returns></returns>
       // public List<Transport> getCompProcesse()
        public void getCompProcesse()
        {
            List<Transport> transList = new List<Transport>();
            Transport transport; // = new Transport((transList.Count + 1).ToString(), "fndfg", "fhjmfhj", "gk,ghj", 3.14);

            Process[] processes = Process.GetProcesses();
            countProcesse = processes.Count();

            string result = "";

            foreach (var instance in processes)
            {
                //записывае в обьекты
                //transport = new Transport(instance.Id.ToString(), instance.ProcessName.ToString(), instance.MainWindowTitle.ToString());

                result += instance.ToString();

                // transList.Add(instance.ProcessName);
                // listBox1.Items.Add(instance.ProcessName);
                // transList.Add(transport); запист в лист
            }
            // countProcesse = transList.Count;

            //Запись результата в файл на жестком диске
            using (var writer = new StreamWriter(settings.ResultPath, true, Encoding.UTF8))
            {
                // writer.WriteLine(DateTime.Now.ToString() + $"Всего запузеных процессов {countProcesse} \t\n: " + string.Join(" ", result));
           
            }

            logger.LogInformation($"Запись результпата. Резульат: {string.Join(" ", result)}");

            // transList.Add(transport);
            // return transList;
        }
    }
//Тестовая структура
    public struct Transport
    {
        public string ID { set; get; }
        public string ProcessName { set; get; }
        public string OpisanieProgressa { set; get; }
        //public string DateDestination { set; get; }
        //public double Price { set; get; }

        // public Transport(string cityDeparture, string cityDestination, string dateDeparture, string dateDestination, double price)
        public Transport(string _id, string _processName, string _opisanieProgressa)
        {
            this.ID = _id;
            this.ProcessName = _processName;

            OpisanieProgressa = _opisanieProgressa;
            //this.DateDeparture = dateDeparture;
            //this.DateDestination = dateDestination;
            //this.Price = price;
        }
    }
}
