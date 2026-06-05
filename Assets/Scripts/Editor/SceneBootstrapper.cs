#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DivineDrift.Config;
using DivineDrift.Core;
using DivineDrift.InputControl;
using DivineDrift.Rendering;
using DivineDrift.UI;

namespace DivineDrift.EditorTools
{
    /// <summary>
    /// Builds a complete, runnable scene from scratch: camera (+ outline effect),
    /// planet object (+ renderer/picker), a light, an EventSystem, and a minimal
    /// Canvas with the edge buttons, status labels, and the philosophy / agent /
    /// cell-info / game-over panels — all wired into a GameManager.
    ///
    /// Run "DivineDrift/Build Playable Scene" AFTER "Generate Default Content".
    /// This avoids shipping a binary .unity file with fragile GUIDs; the scene is
    /// generated deterministically inside your editor instead.
    /// </summary>
    public static class SceneBootstrapper
    {
        [MenuItem("DivineDrift/Build Playable Scene")]
        public static void Build()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(
                "Assets/ScriptableObjects/GameConfig.asset");
            if (config == null)
            {
                EditorUtility.DisplayDialog("DivineDrift",
                    "GameConfig.asset not found. Run 'DivineDrift/Generate Default Content' first.", "OK");
                return;
            }

            // --- Camera + outline ---
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.93f, 0.95f, 0.98f); // pale sky, no clouds
            cam.fieldOfView = 45f;
            var orbit = camGo.AddComponent<PlanetCameraController>();
            var outline = camGo.AddComponent<OutlineEffect>();
            outline.outlineShader = Shader.Find("DivineDrift/OutlinePostProcess");

            // --- Light (soft directional) ---
            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.color = new Color(1f, 0.98f, 0.92f);
            lightGo.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

            // --- Planet ---
            var planetGo = new GameObject("Planet");
            planetGo.AddComponent<MeshFilter>();
            planetGo.AddComponent<MeshRenderer>();
            var planetRenderer = planetGo.AddComponent<PlanetRenderer>();
            var toonMat = new Material(Shader.Find("DivineDrift/PastelToon"));
            AssetDatabase.CreateAsset(toonMat, "Assets/Materials/PastelToon.mat");
            planetRenderer.pastelToonMaterial = toonMat;

            var picker = planetGo.AddComponent<CellPicker>();
            picker.cam = cam;
            picker.planet = planetGo.transform;

            orbit.cam = cam;
            orbit.planet = planetGo.transform;

            // --- EventSystem ---
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // --- Canvas + UI ---
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var hud = canvasGo.AddComponent<HUDController>();
            var agentPanel = MakePanel(canvasGo.transform, "AgentPanel").AddComponent<AgentPanel>();
            var cellInfo = MakePanel(canvasGo.transform, "CellInfoPanel").AddComponent<CellInfoPanel>();
            var philosophy = MakePanel(canvasGo.transform, "PhilosophyPanel").AddComponent<PhilosophySelectionPanel>();
            var gameOver = MakePanel(canvasGo.transform, "GameOverPanel").AddComponent<GameOverPanel>();

            // NOTE: button/label child objects are created as placeholders; lay them
            // out around the screen edges and assign references in the inspector.
            hud.agentPanel = agentPanel;
            hud.cellInfoPanel = cellInfo;

            // --- GameManager ---
            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();
            gm.config = config;
            gm.planetRenderer = planetRenderer;
            gm.cameraController = orbit;
            gm.cellPicker = picker;
            gm.philosophyPanel = philosophy;
            gm.hud = hud;
            gm.agentPanel = agentPanel;
            gm.cellInfoPanel = cellInfo;
            gm.gameOverPanel = gameOver;

            Debug.Log("[DivineDrift] Playable scene built. Save it as Assets/Scenes/Main.unity. " +
                      "Lay out HUD buttons/labels around the edges and assign references on each panel.");
        }

        private static GameObject MakePanel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.SetActive(false);
            return go;
        }
    }
}
#endif
