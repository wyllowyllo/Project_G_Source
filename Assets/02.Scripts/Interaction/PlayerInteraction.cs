using System.Collections.Generic;
using UnityEngine;

namespace Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode _interactKey = KeyCode.F;

        [Header("Target Selection")]
        [SerializeField] private float _maxDetectionRange = 5f;
        [SerializeField] [Range(0f, 1f)] private float _angleWeight = 0.6f;
        [SerializeField] [Range(0f, 1f)] private float _distanceWeight = 0.4f;

        private readonly List<IInteractable> _candidates = new();
        private IInteractable _currentTarget;
        private Transform _cameraTransform;

        public IInteractable CurrentTarget => _currentTarget;
        public bool HasTarget => _currentTarget != null && _currentTarget.CanInteract();

        private void Start()
        {
            _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            UpdateBestTarget();

            if (_currentTarget == null) return;
            if (!_currentTarget.CanInteract()) return;

            if (Input.GetKeyDown(_interactKey))
            {
                _currentTarget.Interact();
            }
        }

        public void AddCandidate(IInteractable target)
        {
            if (!_candidates.Contains(target))
            {
                _candidates.Add(target);
            }
        }

        public void RemoveCandidate(IInteractable target)
        {
            _candidates.Remove(target);

            if (_currentTarget == target)
            {
                _currentTarget.OnUnhighlight();
                _currentTarget = null;
            }
        }

        private void UpdateBestTarget()
        {
            _candidates.RemoveAll(c => c == null || c.Transform == null);

            if (_candidates.Count == 0)
            {
                SetTarget(null);
                return;
            }

            IInteractable bestTarget = null;
            float bestScore = float.MinValue;

            foreach (var candidate in _candidates)
            {
                if (!candidate.CanInteract()) continue;

                float score = CalculateScore(candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = candidate;
                }
            }

            SetTarget(bestTarget);
        }

        private float CalculateScore(IInteractable target)
        {
            Vector3 targetPosition = target.Transform.position;
            Vector3 playerPosition = transform.position;
            Vector3 cameraPosition = _cameraTransform.position;
            
            float distance = Vector3.Distance(playerPosition, targetPosition);
            float normalizedDistance = Mathf.Clamp01(distance / _maxDetectionRange);
            float distanceScore = 1f - normalizedDistance;
            
            Vector3 directionToTarget = (targetPosition - cameraPosition).normalized;
            float dot = Vector3.Dot(_cameraTransform.forward, directionToTarget);
            float angleScore = (dot + 1f) / 2f;

            return (angleScore * _angleWeight) + (distanceScore * _distanceWeight);
        }

        private void SetTarget(IInteractable target)
        {
            if (_currentTarget == target) return;

            _currentTarget?.OnUnhighlight();
            _currentTarget = target;
            _currentTarget?.OnHighlight();
        }
    }
}
