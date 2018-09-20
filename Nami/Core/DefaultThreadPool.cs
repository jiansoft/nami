using System;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Default implementation that uses the .NET thread pool.
    /// </summary>
    public class DefaultThreadPool : IThreadPool
    {
        /// <inheritdoc />
        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Queue(Action action)
        {
            Task.Run(action);
        }
    }
}