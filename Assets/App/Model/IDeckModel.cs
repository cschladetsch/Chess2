﻿using System;
using System.Collections.Generic;

namespace App.Model
{
    using Common;

    /// <summary>
    /// A deckModel in play in a game
    /// </summary>
    public interface IDeckModel :
        ICardCollection<ICardModel>,
        IConstructWith<ITemplateDeck>,
        IModel
    {
        void NewGame();
        void Shuffle();
        ICardModel Draw();
        IEnumerable<ICardModel> Draw(int count);
        bool AddToBottom(ICardModel cardModel);

        /// <summary>
        /// Adds a number of cards to random locations in the deckModel.
        /// </summary>
        /// <param name="cardModel"></param>
        /// <returns>the number of cards added</returns>
        int ShuffleIn(params ICardModel[] cardModel);
    }
}
