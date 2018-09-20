using System;
using System.Collections.Generic;
using System.Threading;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Registry for subscriptions. Provides thread safe methods for list of subscriptions.
    /// </summary>
    internal class Subscriptions : IDisposable
    {
        private readonly object _lock = new object();
        private List<IDisposable> _items = new List<IDisposable>();

        /// <summary>
        /// Add Disposable
        /// </summary>
        /// <param name="toAdd"></param>
        public void Add(IDisposable toAdd)
        {
            lock (_lock)
            {
                _items.Add(toAdd);
            }
        }

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public bool Remove(IDisposable toRemove)
        {
            bool flag;
            lock (_lock)
            {
                flag = _items.Remove(toRemove);
            }
            return flag;
        }

        /// <inheritdoc />
        /// <summary>
        /// Disposes all disposables registered in list.
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in Interlocked.Exchange(ref _items, new List<IDisposable>()))
            {
                disposable.Dispose();
            }
            lock (_lock)
            {
                foreach (var victim in _items.ToArray())
                {
                    victim.Dispose();
                }
                _items.Clear();
            }
        }

        /// <summary>
        /// Number of registered disposables.
        /// </summary>
        public int Count
        {
            get
            {
                int count;
                lock (_lock)
                {
                    count = _items.Count;
                }
                return count;
            }
        }
    }
}
