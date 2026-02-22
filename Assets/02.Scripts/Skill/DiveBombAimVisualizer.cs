using UnityEngine;

namespace Skill
{
    public class DiveBombAimVisualizer : MonoBehaviour
    {
        [Header("Landing Indicator")]
        [SerializeField] private GameObject _landingIndicatorPrefab;
        [SerializeField] private Material _indicatorMaterial;
        [SerializeField] private Color _indicatorColor = new Color(1f, 0.3f, 0f, 0.5f);

        [Header("Trajectory Line")]
        [SerializeField] private Material _trajectoryMaterial;
        [SerializeField] private int _trajectorySegments = 30;
        [SerializeField] private float _lineWidth = 0.1f;
        [SerializeField] private Color _lineColor = new Color(1f, 0.5f, 0f, 0.8f);
        [SerializeField] private Vector3 _trajectoryStartOffset = Vector3.zero;

        private GameObject _landingIndicator;
        private LineRenderer _trajectoryLine;
        private float _currentRadius;

        private void Awake()
        {
            CreateLandingIndicator();
            CreateTrajectoryLine();
            Hide();
        }

        private void CreateLandingIndicator()
        {
            if (_landingIndicatorPrefab != null)
            {
                _landingIndicator = Instantiate(_landingIndicatorPrefab, transform);
            }
            else
            {
                _landingIndicator = CreateDefaultIndicator();
            }
        }

        private GameObject CreateDefaultIndicator()
        {
            var indicator = new GameObject("LandingIndicator");
            indicator.transform.SetParent(transform);

            var lineRenderer = indicator.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = true;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.positionCount = 32;

            if (_indicatorMaterial != null)
            {
                lineRenderer.material = _indicatorMaterial;
            }
            lineRenderer.startColor = _indicatorColor;
            lineRenderer.endColor = _indicatorColor;

            return indicator;
        }

        private void CreateTrajectoryLine()
        {
            var lineObj = new GameObject("TrajectoryLine");
            lineObj.transform.SetParent(transform);

            _trajectoryLine = lineObj.AddComponent<LineRenderer>();
            _trajectoryLine.useWorldSpace = true;
            _trajectoryLine.startWidth = _lineWidth;
            _trajectoryLine.endWidth = _lineWidth;
            _trajectoryLine.positionCount = _trajectorySegments;

            if (_trajectoryMaterial != null)
            {
                _trajectoryLine.material = _trajectoryMaterial;
            }
            _trajectoryLine.startColor = _lineColor;
            _trajectoryLine.endColor = _lineColor;
        }

        public void Show()
        {
            _landingIndicator?.SetActive(true);
            if (_trajectoryLine != null)
                _trajectoryLine.enabled = true;
        }

        public void Hide()
        {
            _landingIndicator?.SetActive(false);
            if (_trajectoryLine != null)
                _trajectoryLine.enabled = false;
        }

        public void SetRadius(float radius)
        {
            _currentRadius = radius;
        }

        public void UpdateTarget(Vector3 targetPosition, Vector3 startPosition, float arcHeight)
        {
            UpdateLandingIndicator(targetPosition);
            DrawParabolicTrajectory(startPosition, targetPosition, arcHeight);
        }

        private void UpdateLandingIndicator(Vector3 position)
        {
            if (_landingIndicator == null) return;

            var lineRenderer = _landingIndicator.GetComponent<LineRenderer>();
            if (lineRenderer == null) return;

            int segments = lineRenderer.positionCount;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * 2f * Mathf.PI;
                float x = position.x + _currentRadius * Mathf.Cos(angle);
                float z = position.z + _currentRadius * Mathf.Sin(angle);
                lineRenderer.SetPosition(i, new Vector3(x, position.y + 0.05f, z));
            }
        }

        private void DrawParabolicTrajectory(Vector3 start, Vector3 end, float arcHeight)
        {
            if (_trajectoryLine == null) return;

            Vector3 offsetStart = start + _trajectoryStartOffset;

            for (int i = 0; i < _trajectorySegments; i++)
            {
                float t = (float)i / (_trajectorySegments - 1);
                Vector3 position = CalculateParabolicPosition(offsetStart, end, t, arcHeight);
                _trajectoryLine.SetPosition(i, position);
            }
        }

        private Vector3 CalculateParabolicPosition(Vector3 start, Vector3 end, float t, float arcHeight)
        {
            Vector3 linear = Vector3.Lerp(start, end, t);
            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
            return linear + Vector3.up * arc;
        }
    }
}
