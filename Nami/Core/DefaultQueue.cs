using System;
using System.Collections.Generic;

namespace jIAnSoft.Framework.Nami.Core
{
    public class DefaultQueue : IQueue
    {
        private readonly object _lock = new object();
        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Add(action);
            }
        }

        public List<Action> DequeueAll()
        {
            lock (_lock)
            {
                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                return _toPass;
            }
        }

        public int Count()
        {
            lock (_lock)
            {
                return _actions.Count;
            }
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}