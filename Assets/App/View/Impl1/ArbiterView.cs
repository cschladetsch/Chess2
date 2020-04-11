﻿namespace App.View.Impl1
{
    using UnityEngine;
    using UniRx;
    using Dekuple;
    using Dekuple.Agent;
    using Dekuple.View.Impl;
    using Agent;

    /// <summary>
    /// View of an Arbiter.
    /// </summary>
    public class ArbiterView
        : ViewBase<IArbiterAgent>
        , IArbiterView
    {
        public BoardView Board;
        public PlayerView WhitePlayer;
        public PlayerView BlackPlayer;
        public TMPro.TextMeshPro CurrentPlayerText;
        public TMPro.TextMeshPro StateText;
        public AudioClip[] MusicClips;
        public AudioClip[] EndTurnClips;
        
        public IPlayerView WhitePlayerView => WhitePlayer;
        public IPlayerView BlackPlayerView => BlackPlayer;
        //public IPlayerView CurrentPlayerView => CurrentPlayerColor == EColor.White ? WhitePlayerView : BlackPlayerView;
        public IBoardView BoardView => Board;

        public override void SetAgent(IAgent agent)
        {
            var arbiterAgent = agent as IArbiterAgent;
            Assert.IsNotNull(arbiterAgent);
            base.SetAgent(arbiterAgent);
            PlayMusic();

            WhitePlayerView.SetAgent(Agent.WhitePlayerAgent);
            BlackPlayerView.SetAgent(Agent.BlackPlayerAgent);

            var model = Agent.Model;
            model.GameState.DistinctUntilChanged().Subscribe(
                c => StateText.text = $"{c}").AddTo(this);
            model.CurrentPlayer.DistinctUntilChanged().Subscribe(
                c => CurrentPlayerText.text = $"{c.Color}").AddTo(this);
        }

        public bool CurrentPlayerOwns(IOwned owned)
        {
            Assert.IsTrue(IsValid);
            Assert.IsNotNull(owned);
            Assert.IsNotNull(owned.Owner);
            return Agent.CurrentPlayerAgent.Value.Model == owned.Owner.Value;
        }

        private void PlayMusic()
        {
            _AudioSource.clip = MusicClips[0];
            _AudioSource.loop = true;
            _AudioSource.volume = 0.5f;
            _AudioSource.Play();
        }

        protected override void Step()
        {
            base.Step();
            Agent?.Step();
        }
    }
}


