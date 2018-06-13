﻿using App.Common;
using Flow;
using UniRx;

namespace App.Agent
{
    using Model;

    public interface IArbiterAgent
        : IAgent<IArbiterModel>
    {
        IReadOnlyReactiveProperty<IPlayerAgent> CurrentPlayerAgent { get; }
        IBoardAgent BoardAgent { get; }
        IPlayerAgent WhitePlayerAgent { get; }
        IPlayerAgent BlackPlayerAgent { get; }

        ITransient PrepareGame(IPlayerAgent p0, IPlayerAgent p1);
        void Step();
    }
}
