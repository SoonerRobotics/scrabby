using UnityEngine;
using UnityEngine.Splines;

namespace Scrabby.Gameplay
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SplineContainer))]
    [ExecuteInEditMode]
    public class StripedRoadRenderer : MonoBehaviour
    {
        private SplineContainer _splineContainer;
        private Mesh _roadMesh;
        private Mesh _stripeMesh;

        [Range(1f, 100f)]
        public float width = 1f;
        public int sampleRate = 10;
        public int splineIndex = 0;

        public float tiling = 1f;
        [Range(.05f, 2.0f)]
        public float spacing = 1f;

        [Header("Stripe Settings")]
        public Material stripeMaterial;
        public float stripeWidth = 0.05f;

        [Header("Dash Settings")]
        public float dashLength = 0.2f;
        public float gapLength = 0.2f;

        private GameObject _stripeObject;

        private void OnEnable()
        {
            _splineContainer = GetComponent<SplineContainer>();

            _roadMesh = new Mesh { name = "Road Mesh" };
            GetComponent<MeshFilter>().mesh = _roadMesh;

            SetupStripeObject();

            GenerateMeshes();

            Spline.Changed += OnSplineChanged;
        }

        private void OnDisable()
        {
            Spline.Changed -= OnSplineChanged;
        }

        private void SetupStripeObject()
        {
            if (_stripeObject == null)
            {
                _stripeObject = new GameObject("Center Stripe");
                _stripeObject.transform.SetParent(transform, false);

                var mf = _stripeObject.AddComponent<MeshFilter>();
                var mr = _stripeObject.AddComponent<MeshRenderer>();
                mr.sharedMaterial = stripeMaterial;

                _stripeMesh = new Mesh { name = "Stripe Mesh" };
                mf.mesh = _stripeMesh;
            }
        }

        private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
        {
            GenerateMeshes();
        }

        public float GetSpineLength()
        {
            return _splineContainer.CalculateLength();
        }

        private void SampleSpline(float time, out Vector3 vertLeft, out Vector3 vertRight)
        {
            _splineContainer.Evaluate(splineIndex, time, out var position, out var tangent, out var up);
            var right = Vector3.Cross(tangent, up).normalized;
            vertLeft = (Vector3)position + (right * width);
            vertRight = (Vector3)position - (right * width);

            vertLeft.y = 0;
            vertRight.y = 0;
        }

        private void SampleCenterSpline(float time, out Vector3 vertLeft, out Vector3 vertRight)
        {
            _splineContainer.Evaluate(splineIndex, time, out var position, out var tangent, out var up);
            var right = Vector3.Cross(tangent, up).normalized;

            vertLeft = (Vector3)position + (right * stripeWidth * 0.5f);
            vertRight = (Vector3)position - (right * stripeWidth * 0.5f);

            vertLeft.y = 0.01f; // Slightly above road mesh
            vertRight.y = 0.01f;
        }

        [ContextMenu("Regenerate Meshes")]
        public void GenerateMeshes()
        {
            GenerateRoadMesh();
            GenerateStripeMesh();
        }

        private void GenerateRoadMesh()
        {
            var verts = new Vector3[2 * sampleRate];
            var uvs = new Vector2[verts.Length];

            var numTris = 2 * (sampleRate - 1) + ((_splineContainer.Spline.Closed) ? 2 : 0);
            var tris = new int[numTris * 3];

            var vertIndex = 0;
            var triIndex = 0;

            var step = 1f / sampleRate;
            for (var i = 0; i < sampleRate; i++)
            {
                var t = step * i;
                SampleSpline(t, out var vertLeft, out var vertRight);
                verts[vertIndex] = vertLeft;
                verts[vertIndex + 1] = vertRight;

                var v = 1 - Mathf.Abs(2 * t - 1);
                uvs[vertIndex] = new Vector2(0, v);
                uvs[vertIndex + 1] = new Vector2(1, v);

                if (i < sampleRate - 1 || _splineContainer.Spline.Closed)
                {
                    tris[triIndex] = vertIndex;
                    tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                    tris[triIndex + 2] = vertIndex + 1;

                    tris[triIndex + 3] = vertIndex + 1;
                    tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                    tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
                }

                vertIndex += 2;
                triIndex += 6;
            }

            _roadMesh.Clear();
            _roadMesh.vertices = verts;
            _roadMesh.triangles = tris;
            _roadMesh.uv = uvs;
            _roadMesh.RecalculateNormals();

            var textureRepeat = Mathf.RoundToInt(tiling * sampleRate * spacing * .05f);
            GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        }

        private float GetTimeAtDistance(float targetDistance)
        {
            float accumulatedDistance = 0f;
            Vector3 previousPoint = _splineContainer.EvaluatePosition(splineIndex, 0f);

            int resolution = 100; // Higher = more accurate
            for (int i = 1; i <= resolution; i++)
            {
                float t = (float)i / resolution;
                Vector3 currentPoint = _splineContainer.EvaluatePosition(splineIndex, t);
                float segmentDistance = Vector3.Distance(previousPoint, currentPoint);

                if (accumulatedDistance + segmentDistance >= targetDistance)
                {
                    float overshoot = targetDistance - accumulatedDistance;
                    float segmentRatio = overshoot / segmentDistance;
                    float preciseT = Mathf.Lerp((float)(i - 1) / resolution, t, segmentRatio);
                    return preciseT;
                }

                accumulatedDistance += segmentDistance;
                previousPoint = currentPoint;
            }

            return 1f; // If overshoots, return end of spline
        }

        private void GenerateStripeMesh()
        {
            var verts = new System.Collections.Generic.List<Vector3>();
            var uvs = new System.Collections.Generic.List<Vector2>();
            var tris = new System.Collections.Generic.List<int>();

            float totalLength = _splineContainer.CalculateLength();
            float dashAndGap = dashLength + gapLength;

            int dashCount = Mathf.FloorToInt(totalLength / dashAndGap);

            int stripeVertexIndex = 0;

            for (int i = 0; i < dashCount; i++)
            {
                float startDistance = i * dashAndGap;
                float endDistance = startDistance + dashLength;

                float startT = GetTimeAtDistance(startDistance);
            float endT = GetTimeAtDistance(endDistance);

                SampleCenterSpline(startT, out var vertLeftStart, out var vertRightStart);
                SampleCenterSpline(endT, out var vertLeftEnd, out var vertRightEnd);

                verts.Add(vertLeftStart);
                verts.Add(vertRightStart);
                verts.Add(vertLeftEnd);
                verts.Add(vertRightEnd);

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));

                tris.Add(stripeVertexIndex + 0);
                tris.Add(stripeVertexIndex + 2);
                tris.Add(stripeVertexIndex + 1);

                tris.Add(stripeVertexIndex + 1);
                tris.Add(stripeVertexIndex + 2);
                tris.Add(stripeVertexIndex + 3);

                stripeVertexIndex += 4;
            }

            _stripeMesh.Clear();
            _stripeMesh.SetVertices(verts);
            _stripeMesh.SetTriangles(tris, 0);
            _stripeMesh.SetUVs(0, uvs);
            _stripeMesh.RecalculateNormals();
        }

        private void OnDrawGizmos()
        {
            if (_splineContainer == null) return;

            var step = 1f / sampleRate;
            for (var i = 0; i < sampleRate; i++)
            {
                var t = step * i;
                SampleSpline(t, out var vertLeft, out var vertRight);

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(vertLeft, .1f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(vertRight, .1f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(vertLeft, vertRight);
            }
        }
    }
}
