using System;

namespace jIAnSoft.Nami.Channels
{
    /// <inheritdoc />
    /// <summary>
    /// Used to receive one or more replies.
    /// </summary>
    /// <typeparam name="TM"></typeparam>
    public interface IReply<TM> : IDisposable
    {
        /// <summary>
        /// Receive a single response. Can be called repeatedly for multiple replies.
        /// </summary>
        /// <param name="timeoutInMs"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool Receive(int timeoutInMs, out TM result);
    }
}
