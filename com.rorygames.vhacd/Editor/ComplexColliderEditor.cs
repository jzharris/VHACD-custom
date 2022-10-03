using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;
using System;

namespace VHACD.Unity
{
    [CustomEditor(typeof(ComplexCollider))]
    public class ComplexColliderEditor : Editor
    {
        private ComplexCollider _base;

        private SerializedProperty _script;
        private SerializedProperty _quality;

        private SerializedProperty _parameters;
        private SerializedProperty _paramResolution;
        private SerializedProperty _paramConcavity;
        private SerializedProperty _paramPlaneDownsample;
        private SerializedProperty _paramConvexHullDownsample;
        private SerializedProperty _paramAlpha;
        private SerializedProperty _paramBeta;
        private SerializedProperty _paramPCA;
        private SerializedProperty _paramMode;
        private SerializedProperty _paramMaxVertices;
        private SerializedProperty _paramMinVolume;
        private SerializedProperty _paramConvexHullApprox;
        private SerializedProperty _paramOCLAccel;
        private SerializedProperty _paramMaxConvexHull;
        private SerializedProperty _paramProjectHullVertices;

        private SerializedProperty _colliderData;
        private SerializedProperty _colliders;
        private SerializedProperty _isTrigger;
        private SerializedProperty _physicMaterial;
        private SerializedProperty _hideColliders;
        private SerializedProperty _combineBeforeCompute;

        private string[] _qualityNames = { "Low", "Medium", "High", "Insane", "Custom" };

        public override void OnInspectorGUI()
        {
            Properties();

            ScriptField();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            ValidateColliders();

            QualityButtons();

            CalculateColliderButtons();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DataSettings();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            ColliderSettings();
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void Properties()
        {
            _base = (ComplexCollider)target;

            _script = serializedObject.FindProperty("m_Script");

            _quality = serializedObject.FindProperty("_quality");
            _colliderData = serializedObject.FindProperty("_colliderData");
            _colliders = serializedObject.FindProperty("_colliders");

            _parameters = serializedObject.FindProperty("_parameters");
            _paramResolution = _parameters.FindPropertyRelative("m_resolution");
            _paramConcavity = _parameters.FindPropertyRelative("m_concavity");
            _paramPlaneDownsample = _parameters.FindPropertyRelative("m_planeDownsampling");
            _paramConvexHullDownsample = _parameters.FindPropertyRelative("m_convexhullDownsampling");
            _paramAlpha = _parameters.FindPropertyRelative("m_alpha");
            _paramBeta = _parameters.FindPropertyRelative("m_beta");
            _paramPCA = _parameters.FindPropertyRelative("m_pca");
            _paramMode = _parameters.FindPropertyRelative("m_mode");
            _paramMaxVertices = _parameters.FindPropertyRelative("m_maxNumVerticesPerCH");
            _paramMinVolume = _parameters.FindPropertyRelative("m_minVolumePerCH");
            _paramConvexHullApprox = _parameters.FindPropertyRelative("m_convexhullApproximation");
            _paramOCLAccel = _parameters.FindPropertyRelative("m_oclAcceleration");
            _paramMaxConvexHull = _parameters.FindPropertyRelative("m_maxConvexHulls");
            _paramProjectHullVertices = _parameters.FindPropertyRelative("m_projectHullVertices");

            _isTrigger = serializedObject.FindProperty("_isTrigger");
            _physicMaterial = serializedObject.FindProperty("_material");
            _hideColliders = serializedObject.FindProperty("_hideColliders");

            _combineBeforeCompute = serializedObject.FindProperty("_combineMeshesBeforeCompute");
        }

        private void ScriptField()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_script);
            EditorGUI.EndDisabledGroup();
        }

        #region Quality

        private void QualityButtons()
        {
            EditorGUILayout.LabelField("Collider Quality", EditorStyles.boldLabel);

            if(_quality.intValue == -1)
            {
                _quality.intValue = 2;
                ApplyDefaultParams(2);
            }

            if (!IsQualityEqual() || !IsConvexEqual())
            {
                _quality.intValue = 4;
            }

            int val = GUILayout.Toolbar(_quality.intValue, _qualityNames);
            if(val != _quality.intValue)
            {
                _quality.intValue = val;
                if(val < 4)
                {
                    ApplyDefaultParams(val);
                }
            }

            EditorGUILayout.PropertyField(_paramResolution);
            EditorGUILayout.PropertyField(_paramMaxConvexHull);
            if(val == 4)
            {
                EditorGUILayout.PropertyField(_paramConcavity);
                EditorGUILayout.PropertyField(_paramPlaneDownsample);
                EditorGUILayout.PropertyField(_paramConvexHullDownsample);
                EditorGUILayout.PropertyField(_paramAlpha);
                EditorGUILayout.PropertyField(_paramBeta);
                EditorGUILayout.PropertyField(_paramPCA);
                EditorGUILayout.PropertyField(_paramMode);
                EditorGUILayout.PropertyField(_paramMaxVertices);
                EditorGUILayout.PropertyField(_paramMinVolume);
                EditorGUILayout.PropertyField(_paramConvexHullApprox);
                EditorGUILayout.PropertyField(_paramOCLAccel);
                EditorGUILayout.PropertyField(_paramProjectHullVertices);
            }
        }

        private bool IsQualityEqual()
        {
            if(_quality.intValue >= 4)
            {
                return false;
            }
            else
            {
                return _paramResolution.intValue == Resolution(_quality.intValue);
            }
        }

        private bool IsConvexEqual()
        {
            if (_quality.intValue >= 4)
            {
                return false;
            }
            else
            {
                return _paramMaxConvexHull.intValue == ConvexHulls(_quality.intValue);
            }
        }

        private int Resolution(int quality)
        {
            switch (quality)
            {
                case 0:
                    return 10000;
                case 1:
                    return 200000;
                case 2:
                    return 1000000;
                case 3:
                    return 4000000;
            }
            return -1;
        }

        private int ConvexHulls(int quality)
        {
            switch (quality)
            {
                case 0:
                    return 32;
                case 1:
                    return 64;
                case 2:
                    return 128;
                case 3:
                    return 256;
            }
            return -1;
        }

        private void ApplyDefaultParams(int quality)
        {
            _paramResolution.intValue = Resolution(quality);
            _paramMaxConvexHull.intValue = ConvexHulls(quality);
            _paramConcavity.doubleValue = 0.001;
            _paramPlaneDownsample.intValue = 4;
            _paramConvexHullDownsample.intValue = 4;
            _paramAlpha.doubleValue = 0.05;
            _paramBeta.doubleValue = 0.05;
            _paramPCA.intValue = 0;
            _paramMode.intValue = 0;
            _paramMaxVertices.intValue = 64;
            _paramMinVolume.doubleValue = 0.0001;
            _paramConvexHullApprox.intValue = 1;
            _paramOCLAccel.intValue = 0;
            _paramProjectHullVertices.boolValue = true;
        }

        #endregion

        #region Collider Logic

        private void CalculateColliderButtons()
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUI.BeginDisabledGroup(!_base.TryGetComponent<MeshFilter>(out var filter));
            if (GUILayout.Button($"Calculate Colliders From Current Mesh Filter"))
            {
                if(_colliderData.objectReferenceValue != null)
                {
                    if (EditorUtility.DisplayDialog("Caution", $"This will overwrite the meshes stored within the current asset. All other instances in use will have their colliders updated on validate or awake." +
                    $"\nAre you sure you wish to continue?", "Continue", "Cancel"))
                    {
                        CalculateColliders(_base.Parameters, (ComplexColliderData)_colliderData.objectReferenceValue, false);
                    }
                }
                else
                {
                    CalculateColliders(_base.Parameters, null, false);
                }
            }
            EditorGUI.EndDisabledGroup();
            MeshFilter[] filters = _base.GetComponentsInChildren<MeshFilter>(true);
            EditorGUI.BeginDisabledGroup(filters.Length == 0);
            if (GUILayout.Button($"Calculate Colliders From All Child Mesh Filters"))
            {
                if (_colliderData.objectReferenceValue != null && ((ComplexColliderData)_colliderData.objectReferenceValue).quality == _quality.intValue)
                {
                    if (EditorUtility.DisplayDialog("Caution", $"This will overwrite the meshes stored within the current asset. All other instances in use will have their colliders updated on validate or awake." +
                    $"\nAre you sure you wish to continue?", "Continue", "Cancel"))
                    {
                        CalculateColliders(_base.Parameters, (ComplexColliderData)_colliderData.objectReferenceValue, true);
                    }
                }
                else
                {
                    CalculateColliders(_base.Parameters, null, true);
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(_combineBeforeCompute);

            EditorGUI.EndDisabledGroup();
        }

        private void CalculateColliders(Parameters parameters, ComplexColliderData data, bool combine)
        { 
            Mesh mesh = null;
            string path = "";
            EditorUtility.DisplayProgressBar("Calculating Colliders", "Discovering meshes...", 0.1f);
            List<Mesh> originalMeshes = new List<Mesh>();

            List<Mesh> meshesToCalc = new List<Mesh>();
            List<Matrix4x4> transformsToCalc = new List<Matrix4x4>();

            if (combine)
            {
                MeshFilter[] filters = _base.GetComponentsInChildren<MeshFilter>(true);
                List<MeshFilter> foundMeshes = new List<MeshFilter>();
                foreach (var item in filters)
                {
                    if(item.sharedMesh != null)
                    {
                        foundMeshes.Add(item);
                    }
                }
                if(foundMeshes.Count == 0)
                {
                    EditorUtility.ClearProgressBar();
                    ShowMissingMeshes();
                    return;
                }
                else
                {
                    for (int i = 0; i < foundMeshes.Count; i++)
                    {
                        if(path == "")
                        {
                            path = AssetDatabase.GetAssetPath(foundMeshes[i].sharedMesh);
                        }
                        for (int j = 0; j < foundMeshes[i].sharedMesh.subMeshCount; j++)
                        {
                            meshesToCalc.Add(ExtractSubmesh(foundMeshes[i].sharedMesh, j));
                            transformsToCalc.Add(_base.transform.worldToLocalMatrix * foundMeshes[i].transform.localToWorldMatrix);
                        }
                        originalMeshes.Add(foundMeshes[i].sharedMesh);
                    }
                }
            }
            else
            {
                if(_base.TryGetComponent<MeshFilter>(out var filter))
                {
                    if(filter.sharedMesh == null)
                    {
                        EditorUtility.ClearProgressBar();
                        ShowMissingMeshes();
                        return;
                    }
                    else
                    {
                        for (int j = 0; j < filter.sharedMesh.subMeshCount; j++)
                        {
                            meshesToCalc.Add(ExtractSubmesh(filter.sharedMesh, j));
                            transformsToCalc.Add(_base.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix);
                        }
                        originalMeshes.Add(filter.sharedMesh);
                        path = AssetDatabase.GetAssetPath(mesh);
                    }
                }
                else
                {
                    EditorUtility.ClearProgressBar();
                    ShowMissingMeshFilter();
                    return;
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayProgressBar("Calculating Colliders", "Combining meshes...", 0.3f);

            List<Mesh> meshes = new List<Mesh>();
            if (_combineBeforeCompute.boolValue)
            {
                mesh = CombineMeshes(meshesToCalc, transformsToCalc);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayProgressBar("Calculating Colliders", $"Processing mesh... (this can take a while)", 0.5f);
                VHACDProcessor.GenerateConvexMeshes(mesh, parameters, out meshes);
            }
            else
            {
                List<Mesh> tempMeshes = new List<Mesh>();
                int c = 1;
                foreach (var item in meshesToCalc)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayProgressBar("Calculating Colliders", $"Processing mesh... ({c++}/{meshesToCalc.Count}) (this can take a while)",
                        Mathf.Lerp(0.4f, 0.7f, Mathf.InverseLerp(1, meshesToCalc.Count+1, c)));
                    VHACDProcessor.GenerateConvexMeshes(item, parameters, out tempMeshes);
                    meshes.AddRange(tempMeshes);
                }
            }

            EditorUtility.ClearProgressBar();

            if(meshes.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", $"The object you are trying to calculate colliders for did not compute any submeshes.\nTry modifying your quality parameters and try again", "Ok");
                return;
            }

            EditorUtility.DisplayProgressBar("Calculating Colliders", "Storing submeshes...", 0.8f);

            if (data == null)
            {
                // Deal with non existent asset folder
                if (string.IsNullOrEmpty(path) || path.StartsWith("Assets/") == false)
                {
                    path = $"Assets/ComplexColliders/{_base.name}.asset";
                    if (AssetDatabase.IsValidFolder("Assets/ComplexColliders") == false)
                    {
                        AssetDatabase.CreateFolder("Assets", "ComplexColliders");
                    }
                }
                path = Path.ChangeExtension(path, null) + $"_Colliders_{_qualityNames[_quality.intValue]}.asset";
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                data = ComplexColliderData.CreateAsset(path, _quality.intValue, parameters, meshes.ToArray(), originalMeshes.ToArray());
            }
            else
            {
                data.UpdateAsset(_quality.intValue, parameters, meshes.ToArray(), originalMeshes.ToArray());
            }

            AssetDatabase.SaveAssets();
            _colliderData.objectReferenceValue = data;
            serializedObject.ApplyModifiedProperties();

            ValidateColliders();

            EditorUtility.ClearProgressBar();

            EditorGUIUtility.PingObject(data);
        }

        private Mesh CombineMeshes(List<Mesh> meshes, List<Matrix4x4> transform)
        {
            if(meshes.Count == 1)
            {
                return meshes[0];
            }
            CombineInstance[] combineMesh = new CombineInstance[meshes.Count];
            for (int i = 0; i < meshes.Count; i++)
            {
                combineMesh[i].mesh = meshes[i];
                combineMesh[i].transform = transform[i];
            }
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combineMesh);
            return mesh;
        }

        private Mesh ExtractSubmesh(Mesh mesh, int submesh)
        {
            Mesh newMesh = new Mesh();
            SubMeshDescriptor descriptor = mesh.GetSubMesh(submesh);
            newMesh.vertices = RangeSubset(mesh.vertices, descriptor.firstVertex, descriptor.vertexCount);

            var triangles = RangeSubset(mesh.triangles, descriptor.indexStart, descriptor.indexCount);
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] -= descriptor.firstVertex;
            }

            newMesh.triangles = triangles;

            if (mesh.normals != null && mesh.normals.Length == mesh.vertices.Length)
            {
                newMesh.normals = RangeSubset(mesh.normals, descriptor.firstVertex, descriptor.vertexCount);
            }
            else
            {
                newMesh.RecalculateNormals();
            }

            newMesh.Optimize();
            newMesh.OptimizeIndexBuffers();
            newMesh.RecalculateBounds();
            newMesh.name = mesh.name + $" Submesh {submesh}";
            return newMesh;
        }

        private T[] RangeSubset<T>(T[] array, int startIndex, int length)
        {
            T[] subset = new T[length];
            Array.Copy(array, startIndex, subset, 0, length);
            return subset;
        }

        private void ValidateColliders()
        {
            if (_colliderData.objectReferenceValue != null)
            {
                ComplexColliderData data = (ComplexColliderData)_colliderData.objectReferenceValue;
                for (int i = 0; i < _colliders.arraySize; i++)
                {
                    if(_colliders.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    {
                        _colliders.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                }
                if (_colliders.arraySize != data.computedMeshes.Length)
                {
                    if (_colliders.arraySize > data.computedMeshes.Length)
                    {
                        for (int i = _colliders.arraySize - 1; i >= data.computedMeshes.Length; i--)
                        {
                            DestroyImmediate(_colliders.GetArrayElementAtIndex(i).objectReferenceValue);
                            _colliders.DeleteArrayElementAtIndex(i);
                        }
                    }
                    else
                    {
                        for (int i = _colliders.arraySize; i < data.computedMeshes.Length; i++)
                        {
                            _colliders.InsertArrayElementAtIndex(_colliders.arraySize);
                            _colliders.GetArrayElementAtIndex(i).objectReferenceValue = _base.gameObject.AddComponent<MeshCollider>();
                        }
                    }
                }
                for (int i = 0; i < _colliders.arraySize; i++)
                {
                    MeshCollider mc = (MeshCollider)_colliders.GetArrayElementAtIndex(i).objectReferenceValue;
                    mc.convex = true;
                    mc.sharedMesh = data.computedMeshes[i];
                    mc.hideFlags = _hideColliders.boolValue ? HideFlags.HideInInspector : HideFlags.None;
                    mc.isTrigger = _isTrigger.boolValue;
                    mc.material = (PhysicMaterial)_physicMaterial.objectReferenceValue;
                }
            }
            else
            {
                for (int i = 0; i < _colliders.arraySize; i++)
                {
                    DestroyImmediate(_colliders.GetArrayElementAtIndex(i).objectReferenceValue);
                    _colliders.DeleteArrayElementAtIndex(i);
                    i--;
                }
            }
        }

        #endregion

        #region Data

        private void DataSettings()
        {
            EditorGUILayout.LabelField("Data Settings", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            var oldData = _colliderData.objectReferenceValue;
            EditorGUILayout.PropertyField(_colliderData);
            EditorGUI.EndDisabledGroup();
            if (oldData != _colliderData.objectReferenceValue)
            {
                ValidateColliders();
            }
            EditorGUI.BeginDisabledGroup(_colliderData.objectReferenceValue == null);
            ComplexColliderData data = (ComplexColliderData)_colliderData.objectReferenceValue;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"Quality: {(data == null ? "N/A" :  _qualityNames[data.quality])}");
            EditorGUILayout.LabelField($"Sub-Colliders: {(data == null ? 0 : data.computedMeshes.Length)}");

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        private void ColliderSettings()
        {
            EditorGUILayout.LabelField("Collider Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_isTrigger);
            EditorGUILayout.PropertyField(_physicMaterial);
            EditorGUILayout.PropertyField(_hideColliders);
        }

        #endregion

        private void ShowMissingMeshFilter()
        {
            EditorUtility.DisplayDialog("Error", $"The object you are trying to calculate colliders for has no mesh filter. Calculation has been aborted.", "Continue");
        }

        private void ShowMissingMeshes()
        {
            EditorUtility.DisplayDialog("Error", $"The object you are trying to calculate colliders for has mesh filters but no associated meshes. Calculation has been aborted.", "Continue");
        }

    }
}
