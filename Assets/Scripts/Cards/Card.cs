using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Scripts.Cards
{
    public class Card : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public const int MIN_PARAMETER_NUMBER = 1;
        public const int MAX_PARAMETER_NUMBER = 9;
        public const int MIN_PARAMETER_NUMBER_FOR_RANDOM = -2;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _cardImage;
        [SerializeField] private Image _innerImage;
        [SerializeField] private Image _glowImage;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private TextMeshProUGUI _healthCountLabel;
        [SerializeField] private TextMeshProUGUI _attackPowerLabel;
        [SerializeField] private TextMeshProUGUI _manaCostLabel;

        private int _manaCost;
        private int _attackPower;
        private int _healthCount;

        private Action<Card, int> _onHealthChanged;
        private Action<Card> _onTaken;
        private Action<Card> _onDropped;

        private Tween _glowEffect;
        private Sequence _movementSequence;
        private Coroutine _manaCostChangeCoroutine;
        private Coroutine _attackPowerChangeCoroutine;
        private Coroutine _healthCountChangeCoroutine;

        private Vector3 _lastPositionOnStack;
        private Quaternion _lastRotationOnStack;

        private bool _isAtTable;

        private int SetManaCost
        {
            set 
            {
                if (_manaCostChangeCoroutine != null)
                    StopCoroutine(_manaCostChangeCoroutine);
                _manaCostChangeCoroutine = StartCoroutine(ChangeParameterAnimation(_manaCost, value, _manaCostLabel));
                _manaCost = value;
            }
        }
        private int SetAttackPower
        {
            set
            {
                if (_attackPowerChangeCoroutine != null)
                    StopCoroutine(_attackPowerChangeCoroutine);
                _attackPowerChangeCoroutine = StartCoroutine(ChangeParameterAnimation(_attackPower, value, _attackPowerLabel));
                _attackPower = value;
            }
        }
        private int SetHealthCount
        {
            set
            {
                if (_healthCountChangeCoroutine != null)
                    StopCoroutine(_healthCountChangeCoroutine);
                _healthCountChangeCoroutine = StartCoroutine(ChangeParameterAnimation(_healthCount, value, _healthCountLabel));
                _healthCount = value;
                _onHealthChanged.Invoke(this, _healthCount);
            }
        }

        public RectTransform RectTransform => _rectTransform;

        public void Init(Texture2D texture)
        {
            _innerImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            _manaCost = Random.Range(MIN_PARAMETER_NUMBER, MAX_PARAMETER_NUMBER + 1);
            _attackPower = Random.Range(MIN_PARAMETER_NUMBER, MAX_PARAMETER_NUMBER + 1);
            _healthCount = Random.Range(MIN_PARAMETER_NUMBER, MAX_PARAMETER_NUMBER + 1);

            _glowImage.DOFade(0f, 0f);
            _glowImage.gameObject.SetActive(false);
        }

        public void SetCallbacks(Action<Card, int> onHealthChanged, Action<Card> onTaken, Action<Card> onDropped)
        {
            _onHealthChanged = onHealthChanged;
            _onTaken = onTaken;
            _onDropped = onDropped;
        }

        public void ChangeRandomParameter()
        {
            var randomParameter = Random.Range(0, 3);
            var randomValue = Random.Range(MIN_PARAMETER_NUMBER_FOR_RANDOM, MAX_PARAMETER_NUMBER + 1);
            switch (randomParameter)
            {
                case 0:
                    SetManaCost = randomValue;
                    break;
                case 1:
                    SetAttackPower = randomValue;
                    break;
                case 2:
                    SetHealthCount = randomValue;
                    break;
            }
        }

        private IEnumerator ChangeParameterAnimation(int previousCount, int currentCount, TextMeshProUGUI label)
        {
            var timer = 0f;
            var count = 0;
            while (timer <= 1f)
            {
                count = (int)Mathf.Lerp(previousCount, currentCount, timer);
                label.SetText(count.ToString());

                timer += Time.deltaTime * 2;
                yield return null;
            }

            label.SetText(currentCount.ToString());
        }

        public void MoveToNewPosition(Vector3 newPosition, Quaternion newRotation, float delay)
        {
            if (_movementSequence != null)
                _movementSequence.Kill();

            _movementSequence = DOTween.Sequence();
            _movementSequence.Insert(delay, transform.DOMove(newPosition, 0.5f));
            _movementSequence.Insert(delay, transform.DORotateQuaternion(newRotation, 0.5f));

            _lastPositionOnStack = newPosition;
            _lastRotationOnStack = newRotation;
        }

        public void ResetPosition()
        {
            MoveToNewPosition(_lastPositionOnStack, _lastRotationOnStack, 0f);
        }

        public void MoveToPositionAtTable(Vector2 position)
        {
            _rectTransform.DOAnchorPos(position, 0.5f);
        }

        public void Suicide()
        {
            var sequence = DOTween.Sequence();
            sequence.Insert(0f, transform.DOMove(transform.position - (Vector3.up * 5), 2f));
            sequence.Insert(0f, _cardImage.DOFade(0f, 2f));
            sequence.Insert(0f, _innerImage.DOFade(0f, 2f));
            sequence.Insert(0f, _title.DOFade(0f, 2f));
            sequence.Insert(0f, _description.DOFade(0f, 2f));
            sequence.Insert(0f, _healthCountLabel.DOFade(0f, 2f));
            sequence.Insert(0f, _attackPowerLabel.DOFade(0f, 2f));
            sequence.Insert(0f, _manaCostLabel.DOFade(0f, 2f));
            sequence.OnComplete(() => gameObject.SetActive(false));
            sequence.Play();
        }

        public void SetIsAtTable()
        {
            _isAtTable = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isAtTable)
                return;

            _glowImage.gameObject.SetActive(true);
            if (_glowEffect != null)
                _glowEffect.Kill(true);

            _glowEffect = _glowImage.DOFade(1f, 0.5f);

            _onTaken.Invoke(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_glowEffect != null)
                _glowEffect.Kill();

            _glowEffect = _glowImage.DOFade(0f, 0.3f);
            _glowEffect.OnComplete(() => _glowImage.gameObject.SetActive(false));

            _onDropped.Invoke(this);
        }
    }
}
