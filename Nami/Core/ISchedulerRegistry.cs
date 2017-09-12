using System;

namespace jIAnSoft.Framework.Nami.Core
{
    /// <summary>
    /// Enqueues actions and 
    /// </summary>
    public interface ISchedulerRegistry
    {
        /// <summary>
        /// Enqueue action to target fiber.
        /// </summary>
        /// <param name="action"></param>
        void Enqueue(Action action);

        /// <summary>
        /// Remove timer
        /// </summary>
        /// <param name="timer"></param>
        void Remove(IDisposable timer);
    }
}
