using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Utility.Extension;
using Random = UnityEngine.Random;

namespace Managers
{
    public class CardPile 
    {
        private List<CardData> _cards;
        private Action _onPileChangeCallback;

        public int count => _cards.Count;

        #region Public Methods
    
        public CardPile(Action pileChangeCallback = null)
        {
            _cards = new List<CardData>();
            _onPileChangeCallback += pileChangeCallback;
        }
        
        public void AddOnPileChangeAction(Action method) => _onPileChangeCallback += method;
        public void RemoveOnPileChangeAction(Action method) => _onPileChangeCallback -= method;

        public void AddCard(CardData cardData)
        {
            _cards.Add(cardData);
            _onPileChangeCallback?.Invoke();
        }
    
        public void AddCards(List<CardData> cards)
        {
            for (var i = 0; i < cards.Count; i++)
                _cards.Add(cards[i]);
            _onPileChangeCallback?.Invoke();
        }

        public void RemoveCard(CardData cardData)
        {
            _cards.Remove(cardData);
            _onPileChangeCallback?.Invoke();
        }
        
        public void RemoveCards(List<CardData> cards)
        {
            for (var i = 0; i < cards.Count; i++)
                _cards.Remove(cards[i]);
            _onPileChangeCallback?.Invoke();
        }

        public void ShufflePile()
        {
            _cards.Shuffle();
            _onPileChangeCallback?.Invoke();
        }

        public CardData GetTopCard(bool removeFromPile = true)
        {
            if (_cards?.Count <= 0)
                return null;
            var card = _cards?.First();
            if(removeFromPile && card != null)
                RemoveCard(card);
            return card;
        }
    
        public CardData GetBottomCard(bool removeFromPile = true)
        {
            if (_cards?.Count <= 0)
                return null;
            var card = _cards?.Last();
            if(removeFromPile && card != null)
                RemoveCard(card);
            return card;
        }

        public List<CardData> GetAllCards(bool removeFromPile = true)
        {
            var allCards = _cards;
            if (removeFromPile)
            {
                _cards.Clear();
                _onPileChangeCallback?.Invoke();
            }
            return allCards;
        }

        public CardData GetRandomCard(bool removeFromPile = true)
        {
            if (_cards?.Count <= 0)
                return null;
            var card = _cards?[Random.Range(0, _cards.Count)];
            if(removeFromPile && card != null)
                RemoveCard(card);
            return card;
        }

        public CardData GetCardByType(CARD_TYPE type, bool removeFromPile = true)
        {
            var card = _cards.FirstOrDefault(card => card.Type == type);
            if(removeFromPile && card != null)
                RemoveCard(card);
            return card;
        }

        public int GetAmountCardsByType(CARD_TYPE type)
        {
            var count = 0;
            foreach (var card in _cards)
            {
                if (card.Type == type) //TODO: Checar erro aqui!
                    count++;
            }
            return count;
        }

        public List<CardData> GetAllCardsByType(CARD_TYPE type, bool removeFromPile = true)
        {
            var result = new List<CardData>();
            for (var i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].Type == type)
                    result.Add(_cards[i]);
            }
            if(removeFromPile)
                RemoveCards(result);
            return result;
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
