// (C) 2012-2018 Christian Schladetsch. See http://www.schladetsch.net/flow/license.txt for Licensing information.

using System;

namespace Flow
{
    public delegate void TransientHandler(ITransient sender);
    public delegate void TransientHandlerReason(ITransient sender, ITransient reason);

    /// <summary>
    /// A Transient object notifies observers when it has been Completed. When a Transient is Completed,
    /// it has no more work to do and its internal state will not change without external influence.
    /// flow-control.
    /// </summary>
    public interface ITransient// : INamed
    {
        event TransientHandler Completed;

        bool Active { get; }
        IKernel Kernel { get; /*internal*/ set; }

        void Complete();

        //ITransient Named(string name);
    }
}
