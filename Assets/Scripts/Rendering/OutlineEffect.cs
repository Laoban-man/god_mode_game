using UnityEngine;

namespace DivineDrift.Rendering
{
    /// <summary>
    /// Drives the screen-space black-edge post effect (Built-in pipeline). Attach to
    /// the main camera. Requests depth+normals so the outline shader can detect edges.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class OutlineEffect : MonoBehaviour
    {
        public Shader outlineShader;          // DivineDrift/OutlinePostProcess
        [Range(0, 3)] public float thickness = 1f;
        public Color outlineColor = Color.black;
        [Range(0, 1)] public float depthThreshold = 0.02f;
        [Range(0, 1)] public float normalThreshold = 0.4f;

        private Material _mat;
        private Camera _cam;

        private void OnEnable()
        {
            _cam = GetComponent<Camera>();
            _cam.depthTextureMode |= DepthTextureMode.DepthNormals;
            if (outlineShader != null && _mat == null)
                _mat = new Material(outlineShader) { hideFlags = HideFlags.HideAndDontSave };
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (_mat == null) { Graphics.Blit(src, dst); return; }
            _mat.SetColor("_OutlineColor", outlineColor);
            _mat.SetFloat("_Thickness", thickness);
            _mat.SetFloat("_DepthThreshold", depthThreshold);
            _mat.SetFloat("_NormalThreshold", normalThreshold);
            Graphics.Blit(src, dst, _mat);
        }
    }
}
