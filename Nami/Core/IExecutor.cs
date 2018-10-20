using System;
using System.Collections.Generic;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Executes pending action(s).
    /// </summary>
    public interface IExecutor :IDisposable
    {
        /// <summary>
        /// Executes all actions by parallel.
        /// </summary>
        /// <param name="toExecute"></param>
        void ParallelExecute(IEnumerable<Action> toExecute);

        /// <summary>
        /// Executes all actions.
        /// </summary>
        /// <param name="toExecute"></param>
        void Execute(IEnumerable<Action> toExecute);
        
        ///<summary>
        /// Executes a single action. 
        ///</summary>
        ///<param name="toExecute"></param>
        void Execute(Action toExecute);
    }
}