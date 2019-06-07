namespace jIAnSoft.Nami.Clockwork
{
    public struct BetweenTime
    {
        public readonly int Hour;
        public readonly int Minute;
        public readonly int Second;
        public readonly int Millisecond;
        private readonly int _totalMillisecond;

        public BetweenTime(int hour, int minute, int second, int millisecond)
        {
            Hour = hour % 24;
            Minute = minute % 60;
            Second = second % 60;
            Millisecond = millisecond % 1000;
            _totalMillisecond = Hour * (int) IntervalUnit.Hour +
                                Minute * (int) IntervalUnit.Minute +
                                Second * (int) IntervalUnit.Second +
                                Millisecond;
        }

        /// <summary>
        /// IsZero reports the time instant is zero.
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            return _totalMillisecond == 0;
        }

        /// <summary>
        /// After reports whether the time instant is after u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool After(BetweenTime u)
        {
            return _totalMillisecond > u._totalMillisecond;
        }

        /// <summary>
        ///  Before reports whether the time instant is before u.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool Before(BetweenTime u)
        {
            return _totalMillisecond < u._totalMillisecond;
        }

        /// <summary>
        /// Equal reports whether t and u represent the same time instant. 
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool Equal(BetweenTime u)
        {
            return _totalMillisecond == u._totalMillisecond;
        }

        public override string ToString()
        {
            return $"{Hour}:{Minute}:{Second}.{Millisecond}";
        }
    }
}