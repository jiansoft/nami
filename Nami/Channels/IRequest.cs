namespace jIAnSoft.Nami.Channels
{
    /// <summary>
    /// A request object that can be used to send 1 or many responses to the initial request.
    /// </summary>
    /// <typeparam name="TR"></typeparam>
    /// <typeparam name="TM"></typeparam>
    public interface IRequest<out TR, in TM>
    {
        /// <summary>
        /// Request Message
        /// </summary>
        TR Request { get; }

        /// <summary>
        /// Send one or more responses.
        /// </summary>
        /// <param name="replyMsg"></param>
        /// <returns></returns>
        bool SendReply(TM replyMsg);
    }
}
