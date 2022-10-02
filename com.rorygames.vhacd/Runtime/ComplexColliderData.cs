using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VHACD.Unity
{
    public class ComplexColliderData : ScriptableObject
    {
        [HideInInspector]
        public int quality = 0;

        public Parameters parameters;

        public Mesh[] baseMeshes = new Mesh[0];

        public Mesh[] computedMeshes = new Mesh[0];

#if UNITY_EDITOR

        public static ComplexColliderData CreateAsset(string path, int quality, Parameters parameters, Mesh[] meshes, Mesh[] baseMeshes)
        {
            ComplexColliderData obj = CreateInstance<ComplexColliderData>();
            AssetDatabase.CreateAsset(obj, path);
            int c = 0;
            foreach (var mesh in meshes)
            {
                mesh.name = $"Computed Mesh {c++}";
                AssetDatabase.AddObjectToAsset(mesh, obj);
            }
            obj.quality = quality;
            obj.parameters = parameters;
            obj.baseMeshes = baseMeshes;
            obj.computedMeshes = meshes;
            return obj;
        }

        public void UpdateAsset(int quality, Parameters parameters, Mesh[] meshes, Mesh[] baseMeshes)
        {
            foreach (var mesh in computedMeshes)
            {
                AssetDatabase.RemoveObjectFromAsset(mesh);
            }
            computedMeshes = null;
            int c = 0;
            foreach (var mesh in meshes)
            {
                mesh.name = $"Computed Mesh {c++}";
                AssetDatabase.AddObjectToAsset(mesh, this);
            }
            this.quality = quality;
            this.parameters = parameters;
            this.baseMeshes = baseMeshes;
            computedMeshes = meshes;
        }


#endif
    }
}
