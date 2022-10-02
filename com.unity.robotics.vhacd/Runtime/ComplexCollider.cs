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
            set { _isTrigger = value; UpdateColliders(); }
            get { return _isTrigger; }
        }

        [SerializeField, Tooltip("Applies to all child colliders")]
        private PhysicMaterial _material = null;

        public PhysicMaterial Material
        {
            set { _material = value; UpdateColliders(); }
            get { return _material; }
        }

        private void Awake()
        {
            ValidateColliders();
            UpdateColliders();
        }

        private void Start()
        {
            // Only to allow enable/disable
        }

        private void UpdateColliders()
        {
            for (int i = 0; i < _colliders.Count; i++)
            {
                _colliders[i].isTrigger = _isTrigger;
                _colliders[i].material = _material;
            }
        }

        private void OnDestroy()
        {
            if(_colliders.Count > 0)
            {
                foreach (var item in _colliders)
                {
                    if (item == null)
                        continue;

                    if (Application.isPlaying)
                    {
                        Destroy(item);
                    }
                    else
                    {
#if UNITY_EDITOR
                        EditorApplication.delayCall += () =>
                        {
                            if (item) DestroyImmediate(item);
                        };
#endif
                    }
                }
                _colliders.Clear();
            }
        }

        private void OnEnable()
        {
            foreach (var collider in _colliders)
            {
                collider.enabled = true;
            }
        }

        private void OnDisable()
        {
            foreach (var collider in _colliders)
            {
                collider.enabled = false;
            }
        }

        private void ValidateColliders()
        {
            if(_colliderData != null)
            {
                if(_colliders.Count != _colliderData.computedMeshes.Length)
                {
                    if(_colliders.Count > _colliderData.computedMeshes.Length)
                    {
                        for (int i = _colliders.Count; i > _colliderData.computedMeshes.Length; i--)
                        {
                            DestroyImmediate(_colliders[i]);
                            _colliders.RemoveAt(i);
                        }
                    }
                    else
                    {
                        for (int i = _colliders.Count; i < _colliderData.computedMeshes.Length; i++)
                        {
                            _colliders.Add(gameObject.AddComponent<MeshCollider>());
                        }
                    }
                }
                for (int i = 0; i < _colliders.Count; i++)
                {
                    _colliders[i].convex = true;
                    _colliders[i].sharedMesh = _colliderData.computedMeshes[i];
                }
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
