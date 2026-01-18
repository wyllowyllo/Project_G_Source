using UnityEngine;

namespace Skill
{
    public class SkillDebugVisual : MonoBehaviour
    {
        private const float DURATION = 0.5f;
        private static readonly Color DEBUG_COLOR = new Color(1f, 0.3f, 0.3f, 0.4f);

        private static Material _debugMaterial;

        private static Material DebugMaterial
        {
            get
            {
                if (_debugMaterial == null)
                {
                    _debugMaterial = new Material(Shader.Find("Sprites/Default"));
                    _debugMaterial.color = DEBUG_COLOR;
                }
                return _debugMaterial;
            }
        }

        public static void Spawn(
            SkillAreaType areaType,
            Vector3 position,
            Quaternion rotation,
            float range,
            float angle,
            float coneHeight,
            float boxWidth,
            float boxHeight,
            Vector3 positionOffset)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Vector3 origin = position + rotation * positionOffset;

            switch (areaType)
            {
                case SkillAreaType.Box:
                    SpawnBox(origin, rotation, range, boxWidth, boxHeight);
                    break;
                case SkillAreaType.Sphere:
                    SpawnSphere(origin, range);
                    break;
                case SkillAreaType.Cone:
                    SpawnCone(origin, rotation, range, angle, coneHeight);
                    break;
            }
#endif
        }

        private static void SpawnBox(Vector3 position, Quaternion rotation, float range, float width, float height)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "[Debug] SkillArea_Box";

            Destroy(go.GetComponent<Collider>());

            go.transform.position = position + rotation * new Vector3(0, height * 0.5f, range * 0.5f);
            go.transform.rotation = rotation;
            go.transform.localScale = new Vector3(width, height, range);

            SetupVisual(go);
        }

        private static void SpawnSphere(Vector3 position, float range)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "[Debug] SkillArea_Sphere";

            Destroy(go.GetComponent<Collider>());

            go.transform.position = position;
            go.transform.localScale = Vector3.one * range * 2f;

            SetupVisual(go);
        }

        private static void SpawnCone(Vector3 position, Quaternion rotation, float range, float angle, float coneHeight)
        {
            var go = new GameObject("[Debug] SkillArea_Cone");
            go.transform.position = position;
            go.transform.rotation = rotation;

            var meshFilter = go.AddComponent<MeshFilter>();
            var meshRenderer = go.AddComponent<MeshRenderer>();

            meshFilter.mesh = CreateConeMesh(range, angle, coneHeight);
            meshRenderer.material = DebugMaterial;

            go.AddComponent<SkillDebugVisual>();
        }

        private static Mesh CreateConeMesh(float range, float angle, float coneHeight)
        {
            var mesh = new Mesh();
            int segments = 20;
            float halfAngle = angle * 0.5f * Mathf.Deg2Rad;
            float halfHeight = coneHeight * 0.5f;

            int vertexCount = (segments + 2) * 2;
            int triangleCount = segments * 3 * 2 + segments * 6 + 6 * 2;

            var vertices = new Vector3[vertexCount];
            var triangles = new int[triangleCount];

            int vi = 0;
            int ti = 0;

            int bottomCenterIndex = vi++;
            vertices[bottomCenterIndex] = new Vector3(0, -halfHeight, 0);

            int bottomArcStart = vi;
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
                float x = Mathf.Sin(currentAngle) * range;
                float z = Mathf.Cos(currentAngle) * range;
                vertices[vi++] = new Vector3(x, -halfHeight, z);
            }

            for (int i = 0; i < segments; i++)
            {
                triangles[ti++] = bottomCenterIndex;
                triangles[ti++] = bottomArcStart + i + 1;
                triangles[ti++] = bottomArcStart + i;
            }

            int topCenterIndex = vi++;
            vertices[topCenterIndex] = new Vector3(0, halfHeight, 0);

            int topArcStart = vi;
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
                float x = Mathf.Sin(currentAngle) * range;
                float z = Mathf.Cos(currentAngle) * range;
                vertices[vi++] = new Vector3(x, halfHeight, z);
            }

            for (int i = 0; i < segments; i++)
            {
                triangles[ti++] = topCenterIndex;
                triangles[ti++] = topArcStart + i;
                triangles[ti++] = topArcStart + i + 1;
            }

            for (int i = 0; i < segments; i++)
            {
                int bl = bottomArcStart + i;
                int br = bottomArcStart + i + 1;
                int tl = topArcStart + i;
                int tr = topArcStart + i + 1;

                triangles[ti++] = bl;
                triangles[ti++] = br;
                triangles[ti++] = tl;

                triangles[ti++] = br;
                triangles[ti++] = tr;
                triangles[ti++] = tl;
            }

            int bottomFirst = bottomArcStart;
            int bottomLast = bottomArcStart + segments;
            int topFirst = topArcStart;
            int topLast = topArcStart + segments;

            triangles[ti++] = bottomCenterIndex;
            triangles[ti++] = bottomFirst;
            triangles[ti++] = topFirst;

            triangles[ti++] = bottomCenterIndex;
            triangles[ti++] = topFirst;
            triangles[ti++] = topCenterIndex;

            triangles[ti++] = bottomCenterIndex;
            triangles[ti++] = topLast;
            triangles[ti++] = bottomLast;

            triangles[ti++] = bottomCenterIndex;
            triangles[ti++] = topCenterIndex;
            triangles[ti++] = topLast;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private static void SetupVisual(GameObject go)
        {
            var renderer = go.GetComponent<Renderer>();
            renderer.material = DebugMaterial;

            go.AddComponent<SkillDebugVisual>();
        }

        private float _elapsed;

        private void Update()
        {
            _elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(DEBUG_COLOR.a, 0f, _elapsed / DURATION);
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                var color = renderer.material.color;
                color.a = alpha;
                renderer.material.color = color;
            }

            if (_elapsed >= DURATION)
            {
                Destroy(gameObject);
            }
        }
    }
}
