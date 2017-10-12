using System;
using jIAnSoft.Framework.Nami.Fibers;

namespace jIAnSoft.Framework.Nami.Channels
{
    /// <summary>
    /// Channel for synchronous and asynchronous requests.
    /// </summary>
    /// <typeparam name="TR"></typeparam>
    /// <typeparam name="TM"></typeparam>
    public class RequestReplyChannel<TR, TM>: IRequestReplyChannel<TR,TM>
    {
        private readonly Channel<IRequest<TR, TM>> _requestChannel = new Channel<IRequest<TR, TM>>();

        /// <summary>
        /// Subscribe to requests.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="onRequest"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IFiber fiber, Action<IRequest<TR, TM>> onRequest)
        {
            return _requestChannel.Subscribe(fiber, onRequest);
        }

        /// <summary>
        /// Send request to any and all subscribers.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>null if no subscribers registered for request.</returns>
        public IReply<TM> SendRequest(TR p)
        {
            var request = new ChannelRequest<TR, TM>(p);
            return _requestChannel.Publish(request) ? request : null;
        }
    }
}