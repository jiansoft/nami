using System;
using System.Collections.Generic;
using System.Threading;

namespace jIAnSoft.Framework.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Default implementation.
    /// </summary>
    public class DefaultQueue : IQueue
    {
        private readonly object _lock = new object();
        private readonly IExecutor _executor;

        private bool _running = true;

        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        ///<summary>
        /// Default queue with custom executor
        ///</summary>
        ///<param name="executor"></param>
        public DefaultQueue(IExecutor executor)
        {
            _executor = executor;
        }

        /// <inheritdoc />
        /// <summary>
        ///  Default queue with default executor
        /// </summary>
        public DefaultQueue() 
            : this(new DefaultExecutor())
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Add(action);
                Monitor.PulseAll(_lock);
            }
        }

        public List<Action> Dequeue()
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <summary>
        /// Execute actions until stopped.
        /// </summary>
        public void Run()
        {
            while (ExecuteNextBatch()) { }
        }

        /// <inheritdoc />
        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.PulseAll(_lock);
            }
        }

        public List<Action> DequeueAll()
        {
            lock (_lock)
            {
                if (!ReadyToDequeue()) return null;
                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                return _toPass;
            }
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0 && _running)
            {
                Monitor.Wait(_lock);
            }
            return _running;
        }

        /// <summary>
        /// Remove all actions and execute.
        /// </summary>
        /// <returns></returns>
        private bool ExecuteNextBatch()
        {
            var toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _executor.Execute(toExecute);
            return true;
        }
    }
}