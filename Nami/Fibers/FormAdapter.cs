﻿using System;
using System.ComponentModel;
using jIAnSoft.Nami.Core;

namespace jIAnSoft.Nami.Fibers
{
    internal class FormAdapter : IExecutionContext
    {
        private readonly ISynchronizeInvoke _invoker;

        public FormAdapter(ISynchronizeInvoke invoker)
        {
            _invoker = invoker;
        }

        public void Enqueue(Action action)
        {
            _invoker.BeginInvoke(action, null);
        }
    }
}