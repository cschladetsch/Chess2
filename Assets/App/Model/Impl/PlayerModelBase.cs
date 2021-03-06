﻿using System;
using Dekuple;
using Dekuple.Model;
using UnityEngine;

using UniRx;

// ReSharper disable PublicConstructorInAbstractClass

// event not used
#pragma warning disable 67

namespace App.Model
{
    using Common;
    using Common.Message;

    /// <summary>
    /// Common for all PlayerModels.
    /// </summary>
    public abstract class PlayerModelBase
        : ModelBase
        , IPlayerModel
    {
        [Inject] public IBoardModel Board { get; set; }
        [Inject] public IArbiterModel Arbiter { get; set; }
        [Inject] public Service.ICardTemplateService _CardtemplateService;

        public event Action<IPieceModel, IItemModel> OnEquipped;
        public event Action<IPieceModel, IItemModel> OnUnequipped;
        public event Action<ISpellModel, IModel> OnCastSpell;

        public EColor Color { get; }
        public bool IsWhite => Color == EColor.White;
        public bool IsBlack => Color == EColor.Black;
        public bool AcceptedHand { get; private set; }

        public IEndTurnButtonModel EndTurnButton { get; private set; }
        public IReactiveProperty<int> MaxMana => _maxMana;
        public IReactiveProperty<int> Mana => _mana;
        public IReactiveProperty<int> Health => _health;
        public IHandModel Hand { get; private set; }
        public IDeckModel Deck { get; private set; }

        private readonly IntReactiveProperty _maxMana = new IntReactiveProperty(0);
        private readonly IntReactiveProperty _mana = new IntReactiveProperty(0);
        private readonly IntReactiveProperty _health = new IntReactiveProperty(0);

        public PlayerModelBase(EColor color)
            : base(null)
        {
            Color = color;
            SetOwner(this);

            AcceptedHand = false;
            _mana.Value = 0;
            _maxMana.Value = 0;
        }

        public override void PrepareModels()
        {
            // TODO: pass a deck template
            Deck = Registry.Get<IDeckModel>(null, this);
            Hand = Registry.Get<IHandModel>(this);
            EndTurnButton = Registry.Get<IEndTurnButtonModel>(this);

            Deck.PrepareModels();
            Hand.PrepareModels();
            EndTurnButton.PrepareModels();
        }

        public virtual void StartGame()
        {
            Deck.StartGame();
            Hand.StartGame();
            
            Mana.Value = 1;
            MaxMana.Value = 1;
        }

        public void EndGame()
        {
            Info($"{this} Ends Game");
        }

        public override string ToString()
        {
            return $"{Color}";
        }

        public void CardExhaustion()
        {
            // TODO
            //King.ChangeHealth(Parameters.CardExhaustionHealthLoss);
        }

        public virtual void StartTurn()
        {
            // only increase mana each other turn
            var turnNumber = Arbiter.TurnNumber.Value;
            var inc = turnNumber % 2 == 0 ? 1 : 0;
            
            MaxMana.Value = Math.Min(Parameters.MaxManaCap, MaxMana.Value + inc);
            Mana.Value = MaxMana.Value;
            if (turnNumber > 1)
                Hand.Add(Deck.Draw());

            Verbose(5, $"{this} starts turn with {Mana.Value} mana");
        }

        public void EndTurn()
        {
            Verbose(10, $"{this} ends turn with {Mana.Value} mana");
        }

        public Response CardDrawn(ICardModel card)
        {
            if (Hand.NumCards.Value == Hand.MaxCards)
                return Response.Fail;
            return Hand.Add(card) ? Response.Ok : Response.Fail;
        }

        public virtual void Result(IRequest req, IResponse response)
        {
            Verbose(5, $"{this}: {req} -> {response}");
        }

        public virtual IRequest Mulligan()
        {
            return null;
        }

        public virtual IGameRequest NextAction()
        {
            return null;
        }

        public Response ChangeMana(int change)
        {
            var result = Mana.Value + change;
            if (result < 0)
                return new Response(EResponse.Fail, EError.NotEnoughMana);
            Mana.Value = Math.Clamp(0, Parameters.MaxManaCap, result);
            return Common.Message.Response.Ok;
        }

        public ICardModel RandomCard()
        {
            return _CardtemplateService.NewCardModel(this, EPieceType.Peon);
        }

        public Response ChangeMaxMana(int change)
        {
            // TODO: set this via Rx
            MaxMana.Value = Mathf.Clamp(0, Parameters.MaxManaCap, MaxMana.Value + change);
            return Response.Ok;
        }
    }
}
