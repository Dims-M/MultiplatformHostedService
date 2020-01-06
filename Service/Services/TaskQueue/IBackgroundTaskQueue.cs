
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services.TaskQueue
{
    /// <summary>
    /// Интерфейс Очередь для приема задачь. Принимает делегат. Который потом будет вызыватся.
    /// </summary>
    public interface IBackgroundTaskQueue
    {
        /// <summary>
        /// Узнать размер очереди
        /// </summary>
        int Size { get; }

        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
