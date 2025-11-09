using System;
using System.Collections.Generic;
using System.Threading;

namespace Player.Services
{
    /// <summary>
    /// 可释放资源管理器 - 统一管理IDisposable资源
    /// </summary>
    public class DisposableManager : IDisposable
    {
        private readonly List<IDisposable> _disposables;
        private readonly ReaderWriterLockSlim _lock;
        private bool _disposed;

        public DisposableManager()
        {
            _disposables = new List<IDisposable>();
            _lock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// 注册可释放资源
        /// </summary>
        /// <param name="disposable">可释放资源</param>
        public void Register(IDisposable disposable)
        {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            
            _lock.EnterWriteLock();
            try
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(DisposableManager));
                
                _disposables.Add(disposable);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 取消注册可释放资源
        /// </summary>
        /// <param name="disposable">可释放资源</param>
        public void Unregister(IDisposable disposable)
        {
            if (disposable == null) return;
            
            _lock.EnterWriteLock();
            try
            {
                _disposables.Remove(disposable);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 注册弱引用资源（避免内存泄漏）
        /// </summary>
        /// <param name="disposable">可释放资源</param>
        public void RegisterWeak(IDisposable disposable)
        {
            if (disposable == null) throw new ArgumentNullException(nameof(disposable));
            
            Register(new WeakDisposableWrapper(disposable));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            
            if (disposing)
            {
                _lock.EnterWriteLock();
                try
                {
                    foreach (var disposable in _disposables)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            // 记录错误但不中断清理过程
                            System.Diagnostics.Debug.WriteLine($"释放资源时出错: {ex.Message}");
                        }
                    }
                    _disposables.Clear();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
                
                _lock.Dispose();
            }
            
            _disposed = true;
        }

        ~DisposableManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// 弱引用包装器
        /// </summary>
        private class WeakDisposableWrapper : IDisposable
        {
            private readonly WeakReference<IDisposable> _weakReference;

            public WeakDisposableWrapper(IDisposable disposable)
            {
                _weakReference = new WeakReference<IDisposable>(disposable);
            }

            public void Dispose()
            {
                if (_weakReference.TryGetTarget(out var disposable))
                {
                    disposable.Dispose();
                }
            }
        }
    }
}