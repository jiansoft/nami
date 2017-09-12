using System;
using System.Collections.Generic;

namespace jIAnSoft.Framework.Nami.Core
{
    /// <summary>
    /// Holds on to actions until the execution context can process them.
    /// </summary>
    public interface IQueue
    {
        ///<summary>
        /// Enqueues action for execution context to process.
        ///</summary>
        ///<param name="action"></param>
        void Enqueue(Action action);

        ///<summary>
        /// Dequeues action for execution context to process.
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
