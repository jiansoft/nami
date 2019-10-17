using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Default executor.
    /// </summary>
    public class DefaultExecutor : IExecutor
    {
        public DefaultExecutor()
        {
            IsEnabled = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Executes all actions.
        /// </summary>
        /// <param name="toExecute"></param>
        public void Execute(IEnumerable<Action> toExecute)
        {
            if (!IsEnabled)
            {
                return;
            }

            Parallel.ForEach(toExecute, Execute);
        }

        /// <inheritdoc />
        /// <summary>
        ///  Executes a single action. 
        /// </summary>
        /// <param name="toExecute"></param>
        public void Execute(Action toExecute)
        {
            if (!IsEnabled)
            {
                return;
            }
           
            toExecute?.Invoke();
        }
        
        /// <summary>
        /// When disabled, actions will be ignored by executor. The executor is typically disabled at shutdown
        /// to prevent any pending actions from being executed. 
        /// </summary>
        private bool IsEnabled { get; set; }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            IsEnabled = false;
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}