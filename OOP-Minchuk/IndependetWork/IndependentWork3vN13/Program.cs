using System;

namespace sw3v13
{
    public class ThreadPool : IDisposable
    {
        private int _poolSize;
        private bool _isActive;        
        private bool _disposed = false;  

        public int PoolSize => _poolSize;
        public bool IsActive => _isActive;

        public ThreadPool(int poolSize)
        {
            _poolSize = poolSize;
            _isActive = true;
            Console.WriteLine($"[ThreadPool] Пул потоків створено. Розмір пулу: {_poolSize}. Ресурс виділено.");
        }

        public void ExecuteTask(string taskName)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ThreadPool));

            if (_isActive)
            {
                Console.WriteLine($"[ThreadPool] Виконання завдання \"{taskName}\" у пулі з {_poolSize} потоків.");
            }
            else
            {
                Console.WriteLine("[ThreadPool] Неможливо виконати завдання: пул неактивний.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); 
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Console.WriteLine("[ThreadPool] Звільнення керованих ресурсів.");
                }

                if (_isActive)
                {
                    Console.WriteLine("[ThreadPool] Завершення всіх потоків пулу...");
                    _isActive = false;
                }

                _disposed = true;
            }
        }
        ~ThreadPool()
        {
            Console.WriteLine("[ThreadPool] Виклик деструктора (~ThreadPool). Ресурс не було звільнено вчасно!");
            Dispose(false);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===== Сценарій 1: об'єкт створено через using =====");
            using (var pool1 = new ThreadPool(4))
            {
                pool1.ExecuteTask("Обробка запиту #1");
            }
            Console.WriteLine("Блок using завершено, Dispose() викликано автоматично.\n");

            Console.WriteLine("===== Сценарій 2: об'єкт без using, явний виклик Dispose() =====");
            var pool2 = new ThreadPool(8);
            pool2.ExecuteTask("Обробка запиту #2");
            pool2.Dispose();
            pool2.Dispose();
            Console.WriteLine("Dispose() викликано явно (двічі — повторний виклик безпечний).\n");

            Console.WriteLine("===== Сценарій 3: об'єкт без виклику Dispose(), звільнення через GC =====");
            CreateOrphanObject();

            Console.WriteLine("Виклик GC.Collect() ще не відбувся — ресурс поки НЕ звільнено.");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("Після GC.Collect() та GC.WaitForPendingFinalizers() — деструктор відпрацював.\n");

            Console.WriteLine("Роботу програми завершено.");
        }

        // гарантовано вийшло з області видимості до виклику GC.Collect()
        static void CreateOrphanObject()
        {
            var pool3 = new ThreadPool(2);
            pool3.ExecuteTask("Обробка запиту #3 (без Dispose)");
        }
    }
}