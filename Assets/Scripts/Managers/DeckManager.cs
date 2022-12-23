using UnityEngine;

namespace Managers
{
    public class DeckManager : MonoBehaviour
    {
        private ScriptsReferences Refs => ScriptsReferences.Instance;
        
        public CardPile deck;
        public CardPile graveyard;

        #region Public methods

        public void DeckInitialize()
        {
            ConstructDebugDeck();
            deck?.ShufflePile();
            graveyard = new CardPile();
        }
        
        public CardData GetTopDeckCard()
        {
            if (deck?.count <= 0)
                ShuffleDeckFromGraveyard();
            return deck?.GetTopCard();
        }

        public CardData GetDeckCardByType(CARD_TYPE type)
        {
            var card = deck?.GetCardByType(type);
            if (card != null)
                return card;
            ShuffleDeckFromGraveyard();
            card = deck?.GetCardByType(type);
            return card;
        }

        #endregion
    
        #region Private methods
    
        private void ShuffleDeckFromGraveyard()
        {
            var graveyardCards = graveyard.GetAllCards();
            deck?.AddCards(graveyardCards);
            deck?.ShufflePile();
        }

        private void ConstructDebugDeck()
        {
            deck = new CardPile();
            var type = CARD_TYPE.BOND;
            for (var i = 0; i < Refs.globalConfig.maxDeckAmount; i++)
            {
                if (deck.GetAmountCardsByType(type) < Refs.globalConfig.maxCardsPerType)
                {
                    var card = new CardData()
                    {
                        Id = i,
                        Name = type + i.ToString(),
                        Type = type
                    };
                    deck.AddCard(card);
                }
                else
                {
                    type++;
                    i--;
                }
            }
        }
    
        #endregion
    }
}
