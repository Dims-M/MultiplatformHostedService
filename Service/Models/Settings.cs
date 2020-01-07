using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Models
{
    /// <summary>
    /// Класс обертка над обычными настройками
    /// </summary>
    public class Settings
    {
        //Представляет набор свойств конфигурации приложения ключ/значение.
        private readonly IConfiguration configuration;

        //конструктор.В качестве параметра передаем интерфейс экемпляр IConfiguration 
        public Settings(IConfiguration configuration)
        {
            this.configuration = configuration; 
        }

        public int WorkersCount => configuration.GetValue<int>("WorkersCount"); // время
        public int RunInterval => configuration.GetValue<int>("RunInterval"); //Получение интервала из файла настроек config.json 
        public string InstanceName => configuration.GetValue<string>("name"); //параметр для командной строки. ОЯтправлять настроики через нее
        public string ResultPath => configuration.GetValue<string>("ResultPath"); //получить путь в файлу
    }
}
