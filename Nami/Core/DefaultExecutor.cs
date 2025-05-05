using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Default executor.
    /// </summary>
    public class DefaultExecutor : IExecutor
    {
        /// <summary>
        /// When disabled, actions will be ignored by executor. The executor is typically disabled at shutdown
        /// to prevent any pending actions from being executed. 
        /// </summary>
        private volatile bool _isEnabled = true;
        private int _disposed; // 0: false, 1: true

        /// <inheritdoc />
        /// <summary>
        /// Executes all actions.
        /// </summary>
        /// <param name="toExecute"></param>
        public void Execute(IEnumerable<Action> toExecute)
        {
            if (!_isEnabled)
            {
                return;
            }

            if (toExecute == null)
            {
                return;
            }

            foreach (var action in toExecute)
            {
                Execute(action);
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///  Executes a single action. 
        /// </summary>
        /// <param name="toExecute"></param>
        public void Execute(Action toExecute)
        {
            if (!_isEnabled)
            {
                return;
            }
           
            toExecute?.Invoke();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            if (!disposing)
            {
                return;
            }
            
            _isEnabled = false;
        }

        public void Dispose()
        {
            Dispose(true);
        }

    }
}