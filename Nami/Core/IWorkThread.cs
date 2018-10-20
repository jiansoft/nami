using System;

namespace jIAnSoft.Nami.Core
{
    /// <summary>
    /// A thread pool for executing asynchronous actions.
    /// </summary>
    public interface IWorkThread
    {
        /// <summary>
        /// Enqueue action for execution.
        /// </summary>
        void Queue(Action action);

        void Start();
    }
}