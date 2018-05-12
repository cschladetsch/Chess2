﻿using System.Collections.Generic;
using Flow;
using UnityEngine.EventSystems;

namespace App.Model
{
    /// <summary>
    /// A Player in the game.
    /// Hopefully, these could be bots, or remote players as well
    /// as simple hotseat players at the same local device.
    /// </summary>
    public interface IPlayer : ICreated<EColor, IDeck>, IOwner
    {
        EColor Color { get; }
        int MaxMana { get; }
        int Mana { get; }
        int Health { get; }
        IHand Hand { get; }
        IDeck Deck { get; }
        ICardInstance King { get; }
        IEnumerable<ICardInstance> CardsOnBoard { get; }
        IEnumerable<ICardInstance> CardsInGraveyard { get; }

        void NewGame();
        void ChangeMaxMana(int mana);
    }
}