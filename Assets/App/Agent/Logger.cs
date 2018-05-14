﻿using Flow;

namespace App.Agent
{
    /// <inheritdoc />
    /// <summary>
    /// AgentBase for all agents. Provides a custom logger and an ITransient implementation
    /// to be used with Flow library.
    /// </summary>
    public class Logger : ITransient
    {
        public event TransientHandler Completed;
        public bool Active { get; private set; }
        public IKernel Kernel { get; set; }
        public string Name { get; set; }

        public Logger()
        {
            Active = true;
        }

        public ITransient SetName(string name)
        {
            Name = name;
            return this;
        }

        public void Complete()
        {
            if (!Active)
                return;
            Completed?.Invoke(this);
            Active = false;
        }
    }
}