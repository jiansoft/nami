namespace jIAnSoft.Nami.Channels
{
    /// <summary>
    /// Channel publishing interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPublisher<in T>
    {
        /// <summary>
        /// Publish a message to all subscribers. Returns true if any subscribers are registered.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool Publish(T msg);
    }
}
