using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace VHACD.Unity
{
    [ExecuteInEditMode]
    [AddComponentMenu("Physics/Complex Collider")]
    public class ComplexCollider : MonoBehaviour
    {
#if UNITY_EDITOR
#pragma warning disable CS0414
        [SerializeField, HideInInspector]
        private bool _hideColliders = true;

        [SerializeField, HideInInspector]
        private int _quality = -1;

        [SerializeField, HideInInspector, Tooltip("Disabling this will create more accurate colliders, at the expense of significantly more colliders when using sub-meshes." +
            " This will cause many more physics calculations during gameplay." +
            " Keep this enabled unless you need the upmost accuracy.")]
        private bool _combineMeshesBeforeCompute = true;

#pragma warning restore CS0414
#endif

        [SerializeField, HideInInspector]
        private Parameters _parameters;
        public Parameters Parameters => _parameters;

        [SerializeField]
        private ComplexColliderData _colliderData;

        [SerializeField]
        private List<MeshCollider> _colliders = new List<MeshCollider>();

        public List<MeshCollider> Colliders => _colliders;

        [SerializeField, Tooltip("Applies to all child colliders")]
        private bool _isTrigger = false;
        public bool IsTrigger
        {
            set { _isTrigger = value; UpdateColliders(enabled); }
            get { return _isTrigger; }
        }

        [SerializeField, Tooltip("Applies to all child colliders")]
        private PhysicMaterial _material = null;

        public PhysicMaterial Material
        {
            set { _material = value; UpdateColliders(enabled); }
            get { return _material; }
        }

        private void Start()
        {
            // Only to allow enable/disable
        }

        private void OnEnable()
        {
            UpdateColliders(true);
        }

        private void OnDisable()
        {
            UpdateColliders(false);
        }

        private void UpdateColliders(bool enabled)
        {
            for (int i = 0; i < _colliders.Count; i++)
            {
                if (_colliders[i] == null)
                    continue;

                _colliders[i].isTrigger = _isTrigger;
                _colliders[i].material = _material;
                _colliders[i].convex = true;
                _colliders[i].enabled = enabled;
                if(_colliderData != null && _colliderData.computedMeshes.Length > i)
                {
                    _colliders[i].sharedMesh = _colliderData.computedMeshes[i];
                }
            }
        }

        private void OnDestroy()
        {
            if(_colliders.Count > 0)
            {
                foreach (var item in _colliders)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(item);
                    }
                    else
                    {
#if UNITY_EDITOR
                        EditorApplication.delayCall += () =>
                        {
                            if (item && !Application.isPlaying) DestroyImmediate(item);
                        };
#endif
                    }
                }
                _colliders.Clear();
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            Collider[] cols = GetComponentsInChildren<Collider>(true);
            foreach (var item in cols)
            {
                DestroyImmediate(item);
            }
        }
#endif
    }
}
