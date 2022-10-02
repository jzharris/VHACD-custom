using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;

namespace VHACD.Unity
{
    public static class VHACDProcessor
    {
        unsafe struct ConvexHull
        {
            public double* m_points;
            public uint* m_triangles;
            public uint m_nPoints;
            public uint m_nTriangles;
            public double m_volume;
            public fixed double m_center[3];
        };

        [DllImport("libvhacd")] static extern unsafe void* CreateVHACD();

        [DllImport("libvhacd")] static extern unsafe void DestroyVHACD(void* pVHACD);

        [DllImport("libvhacd")]
        static extern unsafe bool ComputeFloat(
            void* pVHACD,
            float* points,
            uint countPoints,
            uint* triangles,
            uint countTriangles,
            Parameters* parameters);

        [DllImport("libvhacd")]
        static extern unsafe bool ComputeDouble(
            void* pVHACD,
            double* points,
            uint countPoints,
            uint* triangles,
            uint countTriangles,
            Parameters* parameters);

        [DllImport("libvhacd")] static extern unsafe uint GetNConvexHulls(void* pVHACD);

        [DllImport("libvhacd")]
        static extern unsafe void GetConvexHull(
            void* pVHACD,
            uint index,
            ConvexHull* ch);

        public static unsafe bool GenerateConvexMeshes(Mesh mesh, Parameters parameters, out List<Mesh> meshes)
        {
            meshes = null;
            if (mesh == null)
            {
                return false;
            }
            var vhacd = CreateVHACD();

            var verts = mesh.vertices;
            var tris = mesh.triangles;
            fixed (Vector3* pVerts = verts)
            fixed (int* pTris = tris)
            {
                ComputeFloat(
                    vhacd,
                    (float*)pVerts, (uint)verts.Length,
                    (uint*)pTris, (uint)tris.Length / 3,
                    &parameters);
            }

            var numHulls = GetNConvexHulls(vhacd);
            meshes = new List<Mesh>((int)numHulls);
            foreach (var index in Enumerable.Range(0, (int)numHulls))
            {
                ConvexHull hull;
                GetConvexHull(vhacd, (uint)index, &hull);

                var hullMesh = new Mesh();
                var hullVerts = new Vector3[hull.m_nPoints];
                fixed (Vector3* pHullVerts = hullVerts)
                {
                    var pComponents = hull.m_points;
                    var pVerts = pHullVerts;

                    for (var pointCount = hull.m_nPoints; pointCount != 0; --pointCount)
                    {
                        pVerts->x = (float)pComponents[0];
                        pVerts->y = (float)pComponents[1];
                        pVerts->z = (float)pComponents[2];

                        pVerts += 1;
                        pComponents += 3;
                    }
                }

                hullMesh.SetVertices(hullVerts);

                var indices = new int[hull.m_nTriangles * 3];
                Marshal.Copy((System.IntPtr)hull.m_triangles, indices, 0, indices.Length);
                hullMesh.SetTriangles(indices, 0);


                meshes.Add(hullMesh);
            }
            DestroyVHACD(vhacd);
            return true;
        }

    }
}
