using UnityEngine;
using UnityEngine.Splines;

namespace Scrabby.Gameplay
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SplineContainer))]
    [ExecuteInEditMode]
    public class RoadRenderer : MonoBehaviour
    {
        private SplineContainer _splineContainer;
        private Mesh _mesh;

        [Range(1f, 100f)]
        public float width = 1f;
        public int sampleRate = 10;
        public int splineIndex = 0;
    
        public float tiling = 1f;
        [Range(.05f, 2.0f)]
        public float spacing = 1f;

        private void OnEnable()
        {
            _splineContainer = GetComponent<SplineContainer>();
            _mesh = new Mesh
            {
                name = "Road Mesh"
            };
            GetComponent<MeshFilter>().mesh = _mesh;
        
            GenerateMesh();

            Spline.Changed += OnSplineChanged;
        }

        private void Update()
        {
            GenerateMesh();
        }

        private void OnDisable()
        {
            Spline.Changed -= OnSplineChanged;
        }

        public float GetSpineLength()
        {
            return _splineContainer.CalculateLength();
        }
    
        private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
        {
            GenerateMesh();
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

        private void GenerateMesh()
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
                    tris[triIndex + 5] = (vertIndex + 3)  % verts.Length;
                }

                vertIndex += 2;
                triIndex += 6;
            }

            _mesh.Clear();
            _mesh.vertices = verts;
            _mesh.triangles = tris;
            _mesh.uv = uvs;
            _mesh.RecalculateNormals();
        
            var textureRepeat = Mathf.RoundToInt(tiling * sampleRate * spacing * .05f);
            GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        }

        private void OnDrawGizmos()
        {
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
