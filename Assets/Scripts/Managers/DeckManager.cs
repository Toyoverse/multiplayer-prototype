using UnityEngine;

namespace Managers
{
    public class DeckManager : MonoBehaviour
    {
        public CardPile deck;
        public CardPile graveyard;
    
        #region Public methods
    
        public Card GetTopCard()
        {
            if (deck?.count <= 0)
                ShuffleDeckFromGraveyard();
            return deck?.GetTopCard();
        }

        public void ShuffleDeck() => deck?.ShufflePile();
        public void ShuffleGraveyard() => graveyard?.ShufflePile();

        #endregion
    
        #region Private methods
    
        private void ShuffleDeckFromGraveyard()
        {
            var graveyardCards = graveyard.GetAllCards();
            deck?.AddCards(graveyardCards);
            deck?.ShufflePile();
        }
    
        #endregion
    }
}
