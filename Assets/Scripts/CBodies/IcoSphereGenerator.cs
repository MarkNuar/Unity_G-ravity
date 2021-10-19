using System.Collections.Generic;
using UnityEngine;

namespace CBodies
{
    // Start from a d8, and obtain a sphere, by increasing the resolution
    public class IcoSphereGenerator
    {
        // Output:
        public Vector3[] Vertices => _vertices.items;

        public int[] Triangles => _triangles.items;

        // Internal:
        private FixedSizeList<Vector3> _vertices;
        private FixedSizeList<int> _triangles;
        private int _numDivisions;
        private int _numVertsPerFace;
    
        // Indices of the vertex pairs that make up each of the initial 12 edges
        private static readonly int[] VertexPairs = { 0, 1, 0, 2, 0, 3, 0, 4, 1, 2, 2, 3, 3, 4, 4, 1, 5, 1, 5, 2, 5, 3, 5, 4 };
        // Indices of the edge triplets that make up the initial 8 faces
        private static readonly int[] EdgeTriplets = { 0, 1, 4, 1, 2, 5, 2, 3, 6, 3, 0, 7, 8, 9, 4, 9, 10, 5, 10, 11, 6, 11, 8, 7 };
        // The six initial vertices
        private static readonly Vector3[] BaseVertices = { Vector3.up, Vector3.left, Vector3.back, Vector3.right, Vector3.forward, Vector3.down };
    
        public void Generate (int resolution) {
            _numDivisions = Mathf.Max (0, resolution);
            _numVertsPerFace = ((_numDivisions + 3) * (_numDivisions + 3) - (_numDivisions + 3)) / 2;
            int numVerts = _numVertsPerFace * 8 - (_numDivisions + 2) * 12 + 6;
            int numTrisPerFace = (_numDivisions + 1) * (_numDivisions + 1);
    
            _vertices = new FixedSizeList<Vector3> (numVerts);
            _triangles = new FixedSizeList<int> (numTrisPerFace * 8 * 3);
    
            _vertices.AddRange (BaseVertices);
    
            // Create 12 edges, with n vertices added along them (n = numDivisions)
            Edge[] edges = new Edge[12];
            for (int i = 0; i < VertexPairs.Length; i += 2) {
                Vector3 startVertex = _vertices.items[VertexPairs[i]];
                Vector3 endVertex = _vertices.items[VertexPairs[i + 1]];
    
                int[] edgeVertexIndices = new int[_numDivisions + 2];
                edgeVertexIndices[0] = VertexPairs[i];
    
                // Add vertices along edge
                for (int divisionIndex = 0; divisionIndex < _numDivisions; divisionIndex++) {
                    float t = (divisionIndex + 1f) / (_numDivisions + 1f);
                    edgeVertexIndices[divisionIndex + 1] = _vertices.nextIndex;
                    _vertices.Add (Vector3.Slerp (startVertex, endVertex, t));
                }
                edgeVertexIndices[_numDivisions + 1] = VertexPairs[i + 1];
                int edgeIndex = i / 2;
                edges[edgeIndex] = new Edge (edgeVertexIndices);
            }
    
            // Create faces
            for (int i = 0; i < EdgeTriplets.Length; i += 3) {
                int faceIndex = i / 3;
                bool reverse = faceIndex >= 4;
                CreateFace (edges[EdgeTriplets[i]], edges[EdgeTriplets[i + 1]], edges[EdgeTriplets[i + 2]], reverse);
            }
        }
    
        void CreateFace (Edge sideA, Edge sideB, Edge bottom, bool reverse) {
            int numPointsInEdge = sideA.vertexIndices.Length;
            var vertexMap = new FixedSizeList<int> (_numVertsPerFace);
            vertexMap.Add (sideA.vertexIndices[0]); // top of triangle
    
            for (int i = 1; i < numPointsInEdge - 1; i++) {
                // Side A vertex
                vertexMap.Add (sideA.vertexIndices[i]);
    
                // Add vertices between sideA and sideB
                Vector3 sideAVertex = _vertices.items[sideA.vertexIndices[i]];
                Vector3 sideBVertex = _vertices.items[sideB.vertexIndices[i]];
                int numInnerPoints = i - 1;
                for (int j = 0; j < numInnerPoints; j++) {
                    float t = (j + 1f) / (numInnerPoints + 1f);
                    vertexMap.Add (_vertices.nextIndex);
                    _vertices.Add (Vector3.Slerp (sideAVertex, sideBVertex, t));
                }
    
                // Side B vertex
                vertexMap.Add (sideB.vertexIndices[i]);
            }
    
            // Add bottom edge vertices
            for (int i = 0; i < numPointsInEdge; i++) {
                vertexMap.Add (bottom.vertexIndices[i]);
            }
    
            // Triangulate
            int numRows = _numDivisions + 1;
            for (int row = 0; row < numRows; row++) {
                // vertices down left edge follow quadratic sequence: 0, 1, 3, 6, 10, 15...
                // the nth term can be calculated with: (n^2 - n)/2
                int topVertex = ((row + 1) * (row + 1) - row - 1) / 2;
                int bottomVertex = ((row + 2) * (row + 2) - row - 2) / 2;
    
                int numTrianglesInRow = 1 + 2 * row;
                for (int column = 0; column < numTrianglesInRow; column++) {
                    int v0, v1, v2;
    
                    if (column % 2 == 0) {
                        v0 = topVertex;
                        v1 = bottomVertex + 1;
                        v2 = bottomVertex;
                        topVertex++;
                        bottomVertex++;
                    } else {
                        v0 = topVertex;
                        v1 = bottomVertex;
                        v2 = topVertex - 1;
                    }
    
                    _triangles.Add (vertexMap.items[v0]);
                    _triangles.Add (vertexMap.items[(reverse) ? v2 : v1]);
                    _triangles.Add (vertexMap.items[(reverse) ? v1 : v2]);
                }
            }
    
        }
    
        // Convenience classes:

        private class Edge {
            public int[] vertexIndices;
    
            public Edge (int[] vertexIndices) {
                this.vertexIndices = vertexIndices;
            }
        }

        private class FixedSizeList<T> {
            public T[] items;
            public int nextIndex;
    
            public FixedSizeList (int size) {
                items = new T[size];
            }
    
            public void Add (T item) {
                items[nextIndex] = item;
                nextIndex++;
            }
    
            public void AddRange (IEnumerable<T> items) {
                foreach (var item in items) {
                    Add (item);
                }
            }
        }
    }
}
