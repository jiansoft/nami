using System;

namespace jIAnSoft.Framework.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// A scheduled event.
    /// </summary>
    public interface IPendingEvent : IDisposable
    {
        /// <summary>
        /// Time of expiration for this event
        /// </summary>
        long Expiration { get; }

        /// <summary>
        /// Execute this event and optionally schedule another execution.
        /// </summary>
        /// <returns></returns>
        IPendingEvent Execute(long currentTime);
    }
}
