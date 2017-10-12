namespace jIAnSoft.Framework.Nami.Channels
{
    /// <summary>
    /// Typed channel for request/reply
    /// </summary>
    /// <typeparam name="TR"></typeparam>
    /// <typeparam name="TM"></typeparam>
    public interface IRequestReplyChannel<TR, TM> : IRequestPublisher<TR, TM>, IReplySubscriber<TR, TM>
    {
    }
}
