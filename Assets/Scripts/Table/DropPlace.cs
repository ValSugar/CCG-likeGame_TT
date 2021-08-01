using Scripts.Cards;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Table
{
    public class DropPlace : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private float _cardsPadding;

        private List<Card> _cards;

        public bool CheckCardPositionForEnterPlace(RectTransform cardRectTransform)
        {
            if (cardRectTransform.anchoredPosition.x < -_rectTransform.sizeDelta.x / 2 || cardRectTransform.anchoredPosition.y < -_rectTransform.sizeDelta.y / 2 ||
                cardRectTransform.anchoredPosition.x > _rectTransform.sizeDelta.x / 2 || cardRectTransform.anchoredPosition.y > _rectTransform.sizeDelta.y / 2)
            {
                return false;
            }

            return true;
        }

        public void AddCard(Card card)
        {
            if (_cards == null)
                _cards = new List<Card>();

            card.transform.SetParent(transform);
            _cards.Add(card);
            card.SetIsAtTable();
            RecalculateCardsPosition();
        }

        private void RecalculateCardsPosition()
        {
            var needlyPadding = ((_cards.Count - 1) / 2f) * -_cardsPadding;
            for (var i = 0; i < _cards.Count; i++)
            {
                var newPosition = _rectTransform.anchoredPosition + new Vector2(needlyPadding, 0f);
                needlyPadding += _cardsPadding;
                _cards[i].MoveToPositionAtTable(newPosition);
            }
        }
    }
}
