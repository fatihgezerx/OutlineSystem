using System;
using System.Collections.Generic;
using UnityEngine;

namespace OutlineSystem
{
    /// <summary>
    /// Drop this on any GameObject to be able to show an outline around it. Works standalone - no other
    /// system required. Never touches the object's own material: on <see cref="Show"/> it appends a
    /// shared outline material as an extra slot on every <see cref="MeshRenderer"/> and
    /// <see cref="SkinnedMeshRenderer"/> found on itself or its children, and removes that slot again on
    /// <see cref="Hide"/>. If none is found (e.g. the object has no mesh at all), it logs a warning once
    /// and both methods do nothing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Outline : MonoBehaviour
    {
        private const string ShaderName = "Hidden/OutlineSystem/Outline";

        private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");

        private static Material _sharedMaterial;

        [SerializeField] private Color color = new(1f, 0.85f, 0f, 1f);
        [Min(0f)] [SerializeField] private float width = 0.02f;

        private Renderer[] _renderers = Array.Empty<Renderer>();
        private Material[][] _originalMaterials;
        private Material[][] _outlineMaterials;
        private MaterialPropertyBlock _block;
        private bool _ready;
        private bool _visible;

        /// <summary>Outline color. Takes effect immediately, whether or not the outline is currently shown.</summary>
        public Color Color
        {
            get => color;
            set
            {
                color = value;
                ApplyPropertyBlock();
            }
        }

        /// <summary>Outline width, in the object's local units. Takes effect immediately.</summary>
        public float Width
        {
            get => width;
            set
            {
                width = value;
                ApplyPropertyBlock();
            }
        }

        /// <summary>Whether the outline is currently shown.</summary>
        public bool IsVisible => _visible;

        private void Awake() => Setup();

        /// <summary>Shows the outline. Does nothing if this object has no mesh renderer.</summary>
        public void Show()
        {
            Setup();
            if (_visible || _renderers.Length == 0)
            {
                return;
            }

            _visible = true;
            for (var i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].sharedMaterials = _outlineMaterials[i];
            }

            // Only now does the outline slot actually exist on the renderers - push the block into it.
            ApplyPropertyBlock();
        }

        /// <summary>Hides the outline.</summary>
        public void Hide()
        {
            if (!_visible)
            {
                return;
            }

            _visible = false;
            for (var i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].sharedMaterials = _originalMaterials[i];
            }
        }

        private void Setup()
        {
            if (_ready)
            {
                return;
            }

            _ready = true;

            var found = GetComponentsInChildren<Renderer>(true);
            var usable = new List<Renderer>(found.Length);
            foreach (var candidate in found)
            {
                if (candidate is MeshRenderer or SkinnedMeshRenderer)
                {
                    usable.Add(candidate);
                }
            }

            if (usable.Count == 0)
            {
                Debug.LogWarning($"[Outline] '{name}' has no MeshRenderer or SkinnedMeshRenderer on itself or " +
                                  "its children; Show()/Hide() will do nothing.", this);
                return;
            }

            var material = GetSharedMaterial();
            if (material == null)
            {
                return;
            }

            _renderers = usable.ToArray();
            _originalMaterials = new Material[_renderers.Length][];
            _outlineMaterials = new Material[_renderers.Length][];
            _block = new MaterialPropertyBlock();

            for (var i = 0; i < _renderers.Length; i++)
            {
                var original = _renderers[i].sharedMaterials;
                _originalMaterials[i] = original;

                var withOutline = new Material[original.Length + 1];
                Array.Copy(original, withOutline, original.Length);
                withOutline[original.Length] = material;
                _outlineMaterials[i] = withOutline;
            }

            ApplyPropertyBlock();
        }

        private static Material GetSharedMaterial()
        {
            if (_sharedMaterial != null)
            {
                return _sharedMaterial;
            }

            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError($"[Outline] Shader '{ShaderName}' not found.");
                return null;
            }

            _sharedMaterial = new Material(shader) { name = "Outline (Shared)", hideFlags = HideFlags.HideAndDontSave };
            return _sharedMaterial;
        }

        // One property block, reapplied to every renderer's outline slot - allocation-free after Setup.
        private void ApplyPropertyBlock()
        {
            if (_block == null)
            {
                return;
            }

            _block.SetColor(ColorId, color);
            _block.SetFloat(WidthId, width);

            // The outline slot only exists on the renderers while shown - pushing it otherwise would hit
            // an index the renderer doesn't have yet (e.g. right after Setup, in Awake).
            if (!_visible)
            {
                return;
            }

            for (var i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].SetPropertyBlock(_block, _outlineMaterials[i].Length - 1);
            }
        }

        private void OnDisable() => Hide();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_ready)
            {
                ApplyPropertyBlock();
            }
        }
#endif
    }
}
