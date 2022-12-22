using TMPro;
using UnityEngine;

namespace Managers
{
    public class HandManager : MonoBehaviour
    {
        [Header("REFERENCES")] 
        [SerializeField] private TextMeshProUGUI bondCardsUICount;
        [SerializeField] private TextMeshProUGUI attackCardsUICount;
        [SerializeField] private TextMeshProUGUI defenseCardsUICount;
        private ScriptsReferences Refs => ScriptsReferences.Instance;
        
        private CardPile _handCards;
        private int cardsPerRound = 3;
        private int minCardPerType = 1;

        #region Public Methods

        public void BuyInitialHand()
        {
            Refs.deckManager.DeckInitialize();
            _handCards = new CardPile();
            _handCards.AddCard(Refs.deckManager.GetDeckCardByType(CARD_TYPE.BOND));
            _handCards.AddCard(Refs.deckManager.GetDeckCardByType(CARD_TYPE.DEFENSE));
            _handCards.AddCard(Refs.deckManager.GetDeckCardByType(CARD_TYPE.ATTACK));
            UpdateUI();
        }

        public void BuyRoundCards()
        {
            CheckMinimumCardsPerRound(); 
            for (var i = 0; i < cardsPerRound; i++)
                _handCards.AddCard(Refs.deckManager.GetTopDeckCard());
            UpdateUI();
        }

        public int DiscardCardsByType(CARD_TYPE type)
        {
            var cards = _handCards.GetAllCardsByType(type);
            Refs.deckManager.graveyard.AddCards(cards);
            UpdateUI();
            return cards.Count;
        }
        
        #endregion

        #region Private Methods

        private void CheckMinimumCardsPerRound()
        {
            var bondCards = _handCards.GetAmountCardsByType(CARD_TYPE.BOND);
            var atkCards = _handCards.GetAmountCardsByType(CARD_TYPE.ATTACK);
            var defCards = _handCards.GetAmountCardsByType(CARD_TYPE.DEFENSE);
            if(bondCards < minCardPerType)
                _handCards.AddCard(Refs.deckManager.GetDeckCardByType(CARD_TYPE.BOND));
            if(atkCards < minCardPerType)
                _handCards.AddCard(Refs.deckManager.GetDeckCardByType(CARD_TYPE.ATTACK));
            if(defCards < minCardPerType)
                _handCards.AddCard(Refs.deckManager.GetDeckCardByType(CARD_TYPE.DEFENSE));
        }

        private void UpdateUI()
        {
            bondCardsUICount.text = "" + _handCards.GetAmountCardsByType(CARD_TYPE.BOND);
            attackCardsUICount.text = "" + _handCards.GetAmountCardsByType(CARD_TYPE.ATTACK);
            defenseCardsUICount.text = "" + _handCards.GetAmountCardsByType(CARD_TYPE.DEFENSE);
        }

        #endregion
    }
}
