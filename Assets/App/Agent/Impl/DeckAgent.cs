using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flow;

namespace App.Agent
{
	using App.Model;
	using App.Registry;

	public class DeckAgent
        //: CardCollection
        : AgentBaseCoro<IDeckModel>
        , IDeckAgent
    {
        public int MaxCards => Parameters.MinCardsInDeck;
        public IEnumerable<ICardAgent> Cards => null;//base.Cards.OfType<ICardAgent>();

        public IChannel<ICardAgent> DrawCards(uint n)
        {
            var channel = New.Channel<ICardAgent>();
            //Node.Add(New.Coroutine(DrawCardsCoro, n, channel));
            return channel;
        }

        public IEnumerator DrawCardsCoro(IGenerator self, uint n, IChannel<ICardAgent> channel)
        {
            while (n-- > 0)
            {
                var card = Draw();
                yield return self.After(card);
                if (!card.Available)
                    yield break;
                channel.Insert(card.Value);
                yield return card;
            }
        }

        public void NewGame()
        {
            throw new System.NotImplementedException();
        }

        public IFuture<ICardAgent> Draw()
        {
            var cardModel = Model.Draw();
            var futureAgent = New.Future<ICardAgent>();
            //futureAgent.Value = Arbiter.NewCardAgent(cardModel, Owner);
            return futureAgent;
        }

        public void Remove(ICardAgent card)
        {
            Model.Remove(card.Model);
        }

        protected override IEnumerator Next(IGenerator self)
        {
            yield return null;
        }
    }
}
