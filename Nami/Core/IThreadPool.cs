using System;

namespace jIAnSoft.Framework.Nami.Core
{
    /// <summary>
    /// A thread pool for executing asynchronous actions.
    /// </summary>
    public interface IThreadPool
    {
        /// <summary>
        /// Enqueue action for execution.
        /// </summary>
        void Queue(Action action);
    }
}