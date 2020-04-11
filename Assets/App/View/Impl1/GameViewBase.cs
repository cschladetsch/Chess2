﻿using Dekuple.Agent;

namespace App.View.Impl1
{
    using Dekuple;
    using Dekuple.View.Impl;
    using Model;

    /// <summary>
    /// Common for many game objects.
    /// </summary>
    public class GameViewBase
        : ViewBase
        , IGameViewBase
    {
        public IPlayerView PlayerView => OwnerView as IPlayerView;
        public IPlayerModel PlayerModel => Owner.Value as IPlayerModel;

        [Inject] public IArbiterView ArbiterView { get; set; }
        [Inject] public IBoardView BoardView { get; set; }

        protected bool IsCurrentPlayer() => ArbiterView.CurrentPlayerOwns(this);
    }

    public class GameViewBase<TAgent>
        : GameViewBase
        where TAgent
            : class
            , IAgent
    {
        public TAgent Agent => AgentBase as TAgent;
    }
}
