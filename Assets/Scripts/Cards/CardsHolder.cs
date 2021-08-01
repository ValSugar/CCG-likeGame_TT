using Scripts.Handlers;
using Scripts.Table;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Scripts.Cards
{
	public class CardsHolder : MonoBehaviour, IUpdateHandler
	{
		[SerializeField] private Button _changeRandomParameterButton;
		[SerializeField] private Transform _leftPoint;
		[SerializeField] private Transform _leftUpPoint;
		[SerializeField] private Transform _rightPoint;
		[SerializeField] private Transform _rightUpPoint;
		[SerializeField] private Card _cardPrefab;
		[SerializeField] private int _thresholdCardsCountForProportionalPlacement;
		[SerializeField] private int _minStartCardsCount;
		[SerializeField] private int _maxStartCardsCount;
		[SerializeField] private float _standartT;
		[SerializeField] private float _zRotation;
		[SerializeField] private DropPlace _dropPlace;

		private int _startCardsCount;
		private List<Card> _cards;
		private int _cardIndex;

		private Camera _mainCamera;
		private Card _takenCard;
		private int _takenCardChildIndex;

		private void Start()
		{
			_mainCamera = Camera.main;
			_changeRandomParameterButton.onClick.AddListener(ChangeRandomParameterOnCard);

			_cards = new List<Card>();
			_startCardsCount = Random.Range(_minStartCardsCount, _maxStartCardsCount + 1);

			StartCoroutine(ImageLoader.LoadTextures(100, 100, _startCardsCount, SpawnCards));
		}

		public void OnUpdate()
		{
			_takenCard.transform.position = _mainCamera.ScreenToWorldPoint(Input.mousePosition) - _mainCamera.transform.position;
		}

		private void SpawnCards(Texture2D[] textures)
		{
			CalculateCardsPosition(_startCardsCount, CreateCard);

			void CreateCard(int index, float t)
			{
				var card = Instantiate(_cardPrefab, transform);
				card.Init(textures[index]);
				card.SetCallbacks(OnCardHealthChanged, OnCardTaken, OnCardDropped);
				var newPosition = GetBezierPointByT(t);
				var newRotation = Quaternion.Euler(0f, 0f, GetCardRotationByT(t));
				card.MoveToNewPosition(newPosition, newRotation, 0f);
				_cards.Add(card);
			}
		}

		private void SetNewPositionForCards()
		{
			var cardsCount = _cards.Count;
			CalculateCardsPosition(cardsCount, MoveCardToNewPosition);

			void MoveCardToNewPosition(int index, float t)
			{
				var newPosition = GetBezierPointByT(t);
				var newRotation = Quaternion.Euler(0f, 0f, GetCardRotationByT(t));
				_cards[index].MoveToNewPosition(newPosition, newRotation, 0.1f * index);
			}
		}

		private void CalculateCardsPosition(int cardsCount, Action<int, float> action)
		{
			var tDelta = 1f / (cardsCount - 1);
			var curentThresholdT = 1f / (_thresholdCardsCountForProportionalPlacement - 1);
			//If the maps are less than the threshold value, we will keep a large distance to make it look nice
			if (tDelta > curentThresholdT)
			{
				var specialCardCount = cardsCount / 2f - 0.5f;
				var startT = 0.5f - (specialCardCount * _standartT);
				for (var i = 0; i < cardsCount; i++)
				{
					action.Invoke(i, startT);
					startT += _standartT;
				}
			}
			else
			{
				for (var i = 0; i < cardsCount; i++)
				{
					var t = i * tDelta;
					action.Invoke(i, t);
				}
			}
		}

		public void ChangeRandomParameterOnCard()
		{
			if (_cards.Count == 0)
				return;

			_cards[_cardIndex].ChangeRandomParameter();
			_cardIndex++;
			if (_cardIndex >= _cards.Count)
				_cardIndex = 0;
		}

		private float GetCardRotationByT(float t)
		{
			var rotation = 0f;
			if (t >= 0.5f)
				rotation = (_zRotation / 0.5f) * (0.5f - t);
			else
				rotation = (_zRotation / 0.5f) * (t - 0.5f) * -1;

			return rotation;
		}

		private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			float u = 1 - t;
			float tt = t * t;
			float uu = u * u;
			float uuu = uu * u;
			float ttt = tt * t;

			Vector3 p = uuu * p0;
			p += 3 * uu * t * p1;
			p += 3 * u * tt * p2;
			p += ttt * p3;
			return p;
		}

		private Vector3 GetBezierPointByT(float t)
		{
			return CalculateBezierPoint(t, _leftPoint.position, _leftUpPoint.position, _rightUpPoint.position, _rightPoint.position);
		}

		private void OnCardHealthChanged(Card card, int healthCount)
		{
			if (healthCount >= 1)
				return;

			card.Suicide();
			_cards.Remove(card);
			SetNewPositionForCards();
		}

		private void OnCardTaken(Card card)
		{
			Juggler.AddUpdateHandler(this);
			_takenCard = card;
			_takenCard.transform.rotation = Quaternion.identity;
			_takenCardChildIndex = _takenCard.transform.GetSiblingIndex();
			_takenCard.transform.SetAsLastSibling();
		}

		private void OnCardDropped(Card card)
		{
			if (card != _takenCard)
				return;

			if (!_dropPlace.CheckCardPositionForEnterPlace(card.RectTransform))
			{
				_takenCard.ResetPosition();
			}
			else
			{
				_dropPlace.AddCard(_takenCard);
				_cards.Remove(_takenCard);
				SetNewPositionForCards();
			}
			Juggler.RemoveUpdateHandler(this);
			_takenCard.transform.SetSiblingIndex(_takenCardChildIndex);
			_takenCard = null;
		}

		private void OnDestroy()
		{
			_changeRandomParameterButton.onClick.RemoveListener(ChangeRandomParameterOnCard);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			var t = 0f;
			while (t < 1f)
			{
				var previousT = t;
				t += 0.05f;
				Gizmos.DrawLine(GetBezierPointByT(previousT), GetBezierPointByT(t));
			}
		}
	}
}
