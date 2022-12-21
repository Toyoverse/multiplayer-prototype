using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Utility.Extension;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class CardPile : MonoBehaviour
    {
        private List<Card> _cards;
        private Action _onPileChangeCallback;

        public int count => _cards.Count;

        #region Public Methods
    
        public void AddOnPileChangeAction(Action method) => _onPileChangeCallback += method;

        public void AddCard(Card card)
        {
            _cards.Add(card);
            _onPileChangeCallback?.Invoke();
        }
    
        public void AddCards(List<Card> cards)
        {
            for (var i = 0; i < cards.Count; i++)
                _cards.Add(cards[i]);
            _onPileChangeCallback?.Invoke();
        }

        public void RemoveCard(Card card)
        {
            _cards.Remove(card);
            _onPileChangeCallback?.Invoke();
        }

        public void ShufflePile()
        {
            _cards.Shuffle();
            _onPileChangeCallback?.Invoke();
        }

        public Card GetTopCard(bool removeFromPile = true)
        {
            if (_cards?.Count <= 0)
                return null;
            var card = _cards?.First();
            if(removeFromPile)
                RemoveCard(card);
            return card;
        }
    
        public Card GetBottomCard(bool removeFromPile = true)
        {
            if (_cards?.Count <= 0)
                return null;
            var card = _cards?.Last();
            if(removeFromPile)
                RemoveCard(card);
            return card;
        }

        public List<Card> GetAllCards(bool removeFromPile = true)
        {
            var allCards = _cards;
            if (removeFromPile)
            {
                _cards.Clear();
                _onPileChangeCallback?.Invoke();
            }
            return allCards;
        }

        public Card GetRandomCard(bool removeFromPile = true)
        {
            if (_cards?.Count <= 0)
                return null;
            var card = _cards?[Random.Range(0, _cards.Count)];
            if(removeFromPile)
                RemoveCard(card);
            return card;
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
