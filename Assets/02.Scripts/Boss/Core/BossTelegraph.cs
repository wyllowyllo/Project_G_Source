using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boss.Core
{
    // 보스 공격 예고 시스템
    // 공격 전 플레이어에게 시각적 힌트 제공
    // 프리팹 없이 런타임에 시각화 생성
    public class BossTelegraph : MonoBehaviour
    {
        [Header("색상 설정")]
        [SerializeField] private Color _warningColor = new Color(1f, 0.3f, 0.1f, 0.5f);
        [SerializeField] private Color _dangerColor = new Color(1f, 0f, 0f, 0.7f);

        [Header("선택적 프리팹 (없으면 자동 생성)")]
        [SerializeField] private GameObject _circleDecalPrefab;
        [SerializeField] private GameObject _coneDecalPrefab;
        [SerializeField] private GameObject _lineDecalPrefab;
        [SerializeField] private GameObject _markerPrefab;

        private GameObject _activeDecal;
        private GameObject _activeEffect;
        private List<GameObject> _activeMarkers = new();
        private Material _runtimeMaterial;

        public event Action OnTelegraphStart;
        public event Action OnTelegraphEnd;

        private void Awake()
        {
            CreateRuntimeMaterial();
        }

        private void CreateRuntimeMaterial()
        {
            // Unlit 투명 머티리얼 생성
            _runtimeMaterial = new Material(Shader.Find("Unlit/Color"));
            if (_runtimeMaterial == null)
            {
                _runtimeMaterial = new Material(Shader.Find("Sprites/Default"));
            }
            _runtimeMaterial.color = _warningColor;

            // 투명도 지원 설정
            _runtimeMaterial.SetFloat("_Mode", 3); // Transparent
            _runtimeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _runtimeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _runtimeMaterial.SetInt("_ZWrite", 0);
            _runtimeMaterial.DisableKeyword("_ALPHATEST_ON");
            _runtimeMaterial.EnableKeyword("_ALPHABLEND_ON");
            _runtimeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            _runtimeMaterial.renderQueue = 3000;
        }

        // 원형 범위 표시 (근접 공격, 소환 위치)
        public void ShowCircle(Vector3 center, float radius, float duration)
        {
            HideAll();

            if (_circleDecalPrefab != null)
            {
                _activeDecal = Instantiate(_circleDecalPrefab, center, Quaternion.identity);
                _activeDecal.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);
            }
            else
            {
                _activeDecal = CreateCircleIndicator(center, radius);
            }

            OnTelegraphStart?.Invoke();

            if (duration > 0f)
            {
                Invoke(nameof(HideAll), duration);
            }
        }

        // 부채꼴 범위 표시 (브레스 공격)
        public void ShowCone(Vector3 origin, Vector3 direction, float angle, float range, float duration)
        {
            HideAll();

            if (_coneDecalPrefab != null)
            {
                Quaternion rotation = Quaternion.LookRotation(direction);
                _activeDecal = Instantiate(_coneDecalPrefab, origin, rotation);
                float widthScale = range * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * 2f;
                _activeDecal.transform.localScale = new Vector3(widthScale, 1f, range);
            }
            else
            {
                _activeDecal = CreateConeIndicator(origin, direction, angle, range);
            }

            OnTelegraphStart?.Invoke();

            if (duration > 0f)
            {
                Invoke(nameof(HideAll), duration);
            }
        }

        // 직선 범위 표시 (돌진 공격, 투사체 궤적)
        public void ShowLine(Vector3 start, Vector3 end, float width, float duration)
        {
            HideAll();

            if (_lineDecalPrefab != null)
            {
                Vector3 direction = end - start;
                float length = direction.magnitude;
                Vector3 center = (start + end) * 0.5f;
                Quaternion rotation = Quaternion.LookRotation(direction);

                _activeDecal = Instantiate(_lineDecalPrefab, center, rotation);
                _activeDecal.transform.localScale = new Vector3(width, 1f, length);
            }
            else
            {
                _activeDecal = CreateLineIndicator(start, end, width);
            }

            OnTelegraphStart?.Invoke();

            if (duration > 0f)
            {
                Invoke(nameof(HideAll), duration);
            }
        }

        // 위치 마커 표시 (소환 위치, 착탄 예정 지점)
        public void ShowMarker(Vector3 position, float duration)
        {
            GameObject marker;

            if (_markerPrefab != null)
            {
                marker = Instantiate(_markerPrefab, position, Quaternion.identity);
            }
            else
            {
                marker = CreateMarkerIndicator(position);
            }

            _activeMarkers.Add(marker);

            if (duration > 0f)
            {
                Destroy(marker, duration);
            }
        }

        // 다중 마커 표시 (여러 소환 위치)
        public void ShowMarkers(Vector3[] positions, float duration)
        {
            foreach (var position in positions)
            {
                ShowMarker(position, duration);
            }

            OnTelegraphStart?.Invoke();
        }

        #region 런타임 인디케이터 생성

        private GameObject CreateCircleIndicator(Vector3 center, float radius)
        {
            // 바닥에 원형 표시 (Cylinder 상단면 활용)
            GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            circle.name = "CircleIndicator";
            circle.transform.position = center + Vector3.up * 0.05f;
            circle.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);

            // Collider 제거
            Destroy(circle.GetComponent<Collider>());

            // 머티리얼 적용
            var renderer = circle.GetComponent<Renderer>();
            renderer.material = new Material(_runtimeMaterial);
            renderer.material.color = _warningColor;

            // 테두리용 LineRenderer 추가
            AddCircleOutline(circle, radius);

            return circle;
        }

        private void AddCircleOutline(GameObject parent, float radius)
        {
            GameObject outlineObj = new GameObject("CircleOutline");
            outlineObj.transform.SetParent(parent.transform);
            outlineObj.transform.localPosition = Vector3.zero;

            LineRenderer line = outlineObj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;

            // 기본 머티리얼
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = _dangerColor;
            line.endColor = _dangerColor;

            // 원 그리기
            int segments = 32;
            line.positionCount = segments;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                // Cylinder 스케일 보정 (부모가 스케일되므로 0.5 사용)
                float x = Mathf.Cos(angle) * 0.5f;
                float z = Mathf.Sin(angle) * 0.5f;
                line.SetPosition(i, new Vector3(x, 0.5f, z));
            }
        }

        private GameObject CreateConeIndicator(Vector3 origin, Vector3 direction, float angle, float range)
        {
            GameObject cone = new GameObject("ConeIndicator");
            cone.transform.position = origin + Vector3.up * 0.05f;

            // 방향 설정 (Y축 회전만)
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                cone.transform.rotation = Quaternion.LookRotation(direction);
            }

            // 메시 생성
            MeshFilter meshFilter = cone.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = cone.AddComponent<MeshRenderer>();

            meshFilter.mesh = CreateConeMesh(angle, range);
            meshRenderer.material = new Material(_runtimeMaterial);
            meshRenderer.material.color = _warningColor;

            // 테두리 추가
            AddConeOutline(cone, angle, range);

            return cone;
        }

        private Mesh CreateConeMesh(float angle, float range)
        {
            Mesh mesh = new Mesh();

            int segments = 16;
            float halfAngle = angle * 0.5f * Mathf.Deg2Rad;

            Vector3[] vertices = new Vector3[segments + 2];
            int[] triangles = new int[segments * 3];

            // 원점
            vertices[0] = Vector3.zero;

            // 부채꼴 호
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
                float x = Mathf.Sin(currentAngle) * range;
                float z = Mathf.Cos(currentAngle) * range;
                vertices[i + 1] = new Vector3(x, 0, z);
            }

            // 삼각형
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private void AddConeOutline(GameObject parent, float angle, float range)
        {
            GameObject outlineObj = new GameObject("ConeOutline");
            outlineObj.transform.SetParent(parent.transform);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localRotation = Quaternion.identity;

            LineRenderer line = outlineObj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = _dangerColor;
            line.endColor = _dangerColor;

            int segments = 16;
            float halfAngle = angle * 0.5f * Mathf.Deg2Rad;

            // 원점 + 호 + 원점으로 돌아가기
            line.positionCount = segments + 3;
            line.SetPosition(0, Vector3.zero);

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
                float x = Mathf.Sin(currentAngle) * range;
                float z = Mathf.Cos(currentAngle) * range;
                line.SetPosition(i + 1, new Vector3(x, 0, z));
            }

            line.SetPosition(segments + 2, Vector3.zero);
        }

        private GameObject CreateLineIndicator(Vector3 start, Vector3 end, float width)
        {
            GameObject line = new GameObject("LineIndicator");

            Vector3 direction = end - start;
            float length = direction.magnitude;
            Vector3 center = (start + end) * 0.5f;

            line.transform.position = center + Vector3.up * 0.05f;

            // Y축 회전만
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                line.transform.rotation = Quaternion.LookRotation(direction);
            }

            // Quad로 라인 표시
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.transform.SetParent(line.transform);
            quad.transform.localPosition = Vector3.zero;
            quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            quad.transform.localScale = new Vector3(width, length, 1f);

            Destroy(quad.GetComponent<Collider>());

            var renderer = quad.GetComponent<Renderer>();
            renderer.material = new Material(_runtimeMaterial);
            renderer.material.color = _warningColor;

            // 테두리 추가
            AddLineOutline(line, width, length);

            return line;
        }

        private void AddLineOutline(GameObject parent, float width, float length)
        {
            GameObject outlineObj = new GameObject("LineOutline");
            outlineObj.transform.SetParent(parent.transform);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localRotation = Quaternion.identity;

            LineRenderer line = outlineObj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = _dangerColor;
            line.endColor = _dangerColor;

            float halfWidth = width * 0.5f;
            float halfLength = length * 0.5f;

            line.positionCount = 4;
            line.SetPosition(0, new Vector3(-halfWidth, 0, -halfLength));
            line.SetPosition(1, new Vector3(-halfWidth, 0, halfLength));
            line.SetPosition(2, new Vector3(halfWidth, 0, halfLength));
            line.SetPosition(3, new Vector3(halfWidth, 0, -halfLength));
        }

        private GameObject CreateMarkerIndicator(Vector3 position)
        {
            GameObject marker = new GameObject("MarkerIndicator");
            marker.transform.position = position + Vector3.up * 0.05f;

            // X 모양 마커
            LineRenderer line1 = CreateMarkerLine(marker, "Line1");
            line1.SetPosition(0, new Vector3(-0.5f, 0, -0.5f));
            line1.SetPosition(1, new Vector3(0.5f, 0, 0.5f));

            LineRenderer line2 = CreateMarkerLine(marker, "Line2");
            line2.SetPosition(0, new Vector3(-0.5f, 0, 0.5f));
            line2.SetPosition(1, new Vector3(0.5f, 0, -0.5f));

            // 원형 테두리
            GameObject circleObj = new GameObject("MarkerCircle");
            circleObj.transform.SetParent(marker.transform);
            circleObj.transform.localPosition = Vector3.zero;

            LineRenderer circle = circleObj.AddComponent<LineRenderer>();
            circle.useWorldSpace = false;
            circle.loop = true;
            circle.startWidth = 0.08f;
            circle.endWidth = 0.08f;
            circle.material = new Material(Shader.Find("Sprites/Default"));
            circle.startColor = _dangerColor;
            circle.endColor = _dangerColor;

            int segments = 16;
            circle.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                circle.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.6f, 0, Mathf.Sin(angle) * 0.6f));
            }

            return marker;
        }

        private LineRenderer CreateMarkerLine(GameObject parent, string name)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(parent.transform);
            lineObj.transform.localPosition = Vector3.zero;

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.startWidth = 0.1f;
            line.endWidth = 0.1f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = _dangerColor;
            line.endColor = _dangerColor;

            return line;
        }

        #endregion

        #region 공격 패턴별 헬퍼 메서드

        // 근접 공격 예고
        public void ShowMeleeWarning(float range)
        {
            ShowCircle(transform.position, range, 0f);
        }

        // 돌진 공격 예고
        public void ShowChargeWarning(Vector3 direction, float distance)
        {
            Vector3 start = transform.position;
            Vector3 end = start + direction * distance;
            ShowLine(start, end, 2f, 0f);
        }

        // 브레스 공격 예고
        public void ShowBreathWarning(float angle, float range)
        {
            ShowCone(transform.position, transform.forward, angle, range, 0f);
        }

        // 투사체 조준 예고
        public void ShowProjectileWarning(Vector3 targetPosition)
        {
            ShowLine(transform.position, targetPosition, 0.5f, 0f);
        }

        // 투사체 조준 업데이트
        public void UpdateProjectileWarning(Vector3 targetPosition)
        {
            if (_activeDecal != null)
            {
                Vector3 start = transform.position;
                Vector3 direction = targetPosition - start;
                direction.y = 0;
                float length = direction.magnitude;
                Vector3 center = (start + targetPosition) * 0.5f;

                _activeDecal.transform.position = center + Vector3.up * 0.05f;
                if (direction != Vector3.zero)
                {
                    _activeDecal.transform.rotation = Quaternion.LookRotation(direction);
                }

                // 자식 Quad 스케일 업데이트
                if (_activeDecal.transform.childCount > 0)
                {
                    Transform quad = _activeDecal.transform.GetChild(0);
                    quad.localScale = new Vector3(0.5f, length, 1f);
                }
            }
        }

        // 소환 위치 예고
        public void ShowSummonWarning(Vector3[] positions)
        {
            ShowMarkers(positions, 0f);
        }

        #endregion

        // 경고 이펙트 표시 (보스 몸체 발광)
        public void ShowWarningEffect(Transform target, float duration)
        {
            HideEffect();

            if (target != null)
            {
                _activeEffect = new GameObject("WarningEffect");
                _activeEffect.transform.SetParent(target);
                _activeEffect.transform.localPosition = Vector3.zero;

                // 간단한 파티클 이펙트 생성
                var ps = _activeEffect.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = _dangerColor;
                main.startSize = 0.3f;
                main.startLifetime = 0.5f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var emission = ps.emission;
                emission.rateOverTime = 20f;

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 1f;
            }

            if (duration > 0f)
            {
                Invoke(nameof(HideEffect), duration);
            }
        }

        // 모든 텔레그래프 숨기기
        public void HideAll()
        {
            CancelInvoke(nameof(HideAll));

            if (_activeDecal != null)
            {
                Destroy(_activeDecal);
                _activeDecal = null;
            }

            // 마커 정리
            foreach (var marker in _activeMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker);
                }
            }
            _activeMarkers.Clear();

            HideEffect();
            OnTelegraphEnd?.Invoke();
        }

        // 이펙트만 숨기기
        public void HideEffect()
        {
            CancelInvoke(nameof(HideEffect));

            if (_activeEffect != null)
            {
                Destroy(_activeEffect);
                _activeEffect = null;
            }
        }

        private void OnDestroy()
        {
            HideAll();

            if (_runtimeMaterial != null)
            {
                Destroy(_runtimeMaterial);
            }
        }
    }
}
