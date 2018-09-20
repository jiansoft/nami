using System;

namespace jIAnSoft.Nami.Core
{
    /// <summary>
    /// Context of execution.
    /// </summary>
    public interface IExecutionContext
    {
        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        void Enqueue(Action action);
    }
}