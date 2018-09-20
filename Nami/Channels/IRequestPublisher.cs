namespace jIAnSoft.Nami.Channels
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TR"></typeparam>
    /// <typeparam name="TM"></typeparam>
    public interface IRequestPublisher<in TR, TM>
    {
        /// <summary>
        /// Send request on the channel.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        IReply<TM> SendRequest(TR request);
    }
}
