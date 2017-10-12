using System;
using jIAnSoft.Framework.Nami.Fibers;

namespace jIAnSoft.Framework.Nami.Channels
{
    /// <summary>
    /// Methods for working with a replyChannel
    /// </summary>
    /// <typeparam name="TR"></typeparam>
    /// <typeparam name="TM"></typeparam>
    public interface IReplySubscriber<out TR, in TM>
    {
        /// <summary>
        /// Subscribe to a request on the channel.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="onRequest"></param>
        /// <returns></returns>
        IDisposable Subscribe(IFiber fiber, Action<IRequest<TR, TM>> onRequest);
    }
}
