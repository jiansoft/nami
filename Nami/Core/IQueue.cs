using System;
using System.Collections.Generic;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Holds on to actions until the execution context can process them.
    /// </summary>
    public interface IQueue : IDisposable
    {
        ///<summary>
        /// Enqueue action for execution context to process.
        ///</summary>
        ///<param name="action"></param>
        void Enqueue(Action action);

        ///<summary>
        /// Dequeue action for execution context to process.
        ///</summary>
        List<Action> DequeueAll();

        int Count();

        /// <summary>
        /// Start consuming actions.
        /// </summary>
        void Run();

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        void Stop();
    }
}
