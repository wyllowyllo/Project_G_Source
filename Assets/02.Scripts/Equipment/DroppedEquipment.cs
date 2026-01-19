using Interaction;
using UnityEngine;

namespace Equipment
{
    public class DroppedEquipment : InteractableBase
    {
        [Header("Equipment Data")]
        [SerializeField] private EquipmentData _equipmentData;

        [Header("Root Object")]
        [SerializeField] private GameObject _rootObject;

        [Header("Floating Effect")]
        [SerializeField] private float _floatAmplitude = 0.15f;
        [SerializeField] private float _floatSpeed = 2f;

        [Header("Rotation Effect")]
        [SerializeField] private float _rotationSpeed = 90f;

        [Header("Particle Effect")]
        [SerializeField] private GameObject _particleRoot;

        private EquipmentTooltipController _tooltipController;
        private ParticleSystem[] _particles;
        private Vector3 _initialLocalPosition;
        private bool _isHighlighted;

        public EquipmentData EquipmentData => _equipmentData;

        public override string InteractionPrompt =>
            _equipmentData != null ? $"[F] {_equipmentData.EquipmentName} 줍기" : "[F] 장비 줍기";

        protected override void Awake()
        {
            base.Awake();
            _tooltipController = GetComponent<EquipmentTooltipController>();

            if (_particleRoot != null)
            {
                _particles = _particleRoot.GetComponentsInChildren<ParticleSystem>();
            }
        }

        private Transform TargetTransform => _rootObject != null ? _rootObject.transform : transform;

        private void Start()
        {
            ApplyGradeColor();
            _initialLocalPosition = TargetTransform.localPosition;
        }

        private void Update()
        {
            var targetTransform = TargetTransform;

            float floatOffset = Mathf.Sin(Time.time * _floatSpeed) * _floatAmplitude;
            targetTransform.localPosition = _initialLocalPosition + Vector3.up * floatOffset;

            if (_isHighlighted)
            {
                targetTransform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
            }
        }

        public void Initialize(EquipmentData data)
        {
            _equipmentData = data;
            ApplyGradeColor();
            if (_tooltipController != null)
            {
                _tooltipController.UpdateTooltip(data);
            }
        }

        private void ApplyGradeColor()
        {
            if (_equipmentData == null || EquipmentGradeSettings.Instance == null) return;

            var color = EquipmentGradeSettings.Instance.GetOutlineColor(_equipmentData.Grade);
            SetOutlineColor(color);
            ApplyParticleColor(color);
        }

        private void ApplyParticleColor(Color color)
        {
            if (_particles == null) return;

            foreach (var particle in _particles)
            {
                var main = particle.main;
                main.startColor = color;
            }
        }

        public override bool CanInteract()
        {
            return _equipmentData != null;
        }

        public override void Interact()
        {
            if (!CanInteract()) return;

            var playerEquipment = FindObjectOfType<PlayerEquipment>();
            if (playerEquipment == null) return;

            if (playerEquipment.TryEquip(_equipmentData))
            {
                var target = _rootObject != null ? _rootObject : gameObject;
                Destroy(target);
            }
        }

        public override void OnHighlight()
        {
            base.OnHighlight();
            _isHighlighted = true;
            if (_tooltipController != null)
            {
                _tooltipController.ShowTooltip();
            }
        }

        public override void OnUnhighlight()
        {
            base.OnUnhighlight();
            _isHighlighted = false;
            if (_tooltipController != null)
            {
                _tooltipController.HideTooltip();
            }
        }
    }
}
