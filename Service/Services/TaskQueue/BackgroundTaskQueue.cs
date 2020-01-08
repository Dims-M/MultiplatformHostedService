using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services.TaskQueue
{
    /// <summary>
    /// Очередь для приема задач. Принимает делегат. Который потом будет вызыватся.
    /// </summary>
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        /// <summary>
        /// ConcurrentQueue обеспечивает многопоточную(паралельных потоках.) работу. 
        /// Для избежания захвата одной задачи, нескольками потоками
        /// </summary>
        private ConcurrentQueue<Func<CancellationToken, Task>> workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
        private SemaphoreSlim signal = new SemaphoreSlim(0); //семофор  который ограничивает число потоков, которые могут одновременно обращаться к ресурсу или пулу ресурсов.

        public int Size => workItems.Count; //размер очереди

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await signal.WaitAsync(cancellationToken); //ожидания освобождения семофор
            workItems.TryDequeue(out var workItem); //Пытается удалить и вернуть объект в начале параллельной очереди.

            return workItem;
        }

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            //проверяем 
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            //добавляем в очередь
            workItems.Enqueue(workItem);
            signal.Release(); //запускаем семафор котроля. Что бы не захватили задачу в момент ее добавления
        }
    }
}
