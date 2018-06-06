﻿using App.Common.Message;
using UniRx;

namespace App.Model
{
    using Common;

    /// <summary>
    /// A Player in the game.
    /// Hopefully, these could be bots, or remote players as well
    /// as simple hotseat players at the same local device.
    /// </summary>
    public interface IPlayerModel
        : IModel
        , IOwner
        , IGameActor
    {
        IBoardModel Board { get; }
        IArbiterModel Arbiter { get; }
        IDeckModel Deck { get; }
        IHandModel Hand { get; }
        ICardModel King { get; }
        IPieceModel KingPiece { get; set; }
        IReactiveProperty<int> MaxMana { get; }
        IReactiveProperty<int> Mana { get; }
        IReactiveProperty<int> Health { get; }

        void StartGame();
        IRequest NextAction();
        Response DrawHand();
        Response CardDrawn(ICardModel card);
        void CardExhaustion();
        void StartTurn();
        void EndTurn();
        void Result(IRequest req, IResponse response);
        Response ChangeMana(int manaChange);
    }
}
