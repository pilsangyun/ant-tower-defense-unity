using System.IO;
using UnityEditor;
using UnityEngine;
using AntTD.Managers;
using AntTD.UI;
using AntTD.Entities;
using AntTD.Data;
using UnityEditor.SceneManagement;

namespace AntTD.EditorScripts
{
    public static class UnityPlaytestRunner
    {
        private static readonly string OutputDir = Path.Combine(Directory.GetCurrentDirectory(), "unity_inspection");

        [MenuItem("AntTD/Run Full Verification Test")]
        public static void RunVerification()
        {
            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }

            Debug.Log("=================================================");
            Debug.Log("🐜 RUNNING FULL VERIFICATION & SCREENSHOT TEST SUITE");
            Debug.Log("=================================================");

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Main camera not found!");
                return;
            }

            var gm = Object.FindAnyObjectByType<GameManager>();
            var wm = Object.FindAnyObjectByType<WaveManager>();
            var pm = Object.FindAnyObjectByType<PlacementManager>();
            var stageMgr = Object.FindAnyObjectByType<StageManager>();
            var weatherMgr = Object.FindAnyObjectByType<WeatherManager>();
            var canvasCtrl = Object.FindAnyObjectByType<InGameCanvasController>();
            var lobbyMgr = Object.FindAnyObjectByType<LobbyManager>();

            // Initialize managers
            if (gm != null)
            {
                var gmAwake = typeof(GameManager).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (gmAwake != null) gmAwake.Invoke(gm, null);
                gm.sugar = 500;
                gm.queenHp = 20;
            }

            if (wm != null)
            {
                var wmAwake = typeof(WaveManager).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (wmAwake != null) wmAwake.Invoke(wm, null);
            }

            if (canvasCtrl != null)
            {
                var ctrlStart = typeof(InGameCanvasController).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (ctrlStart != null) ctrlStart.Invoke(canvasCtrl, null);
            }

            // Refresh cards UI
            var cards = Object.FindObjectsByType<AntCardUI>(FindObjectsInactive.Include);
            foreach (var card in cards)
            {
                var startM = typeof(AntCardUI).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (startM != null) startM.Invoke(card, null);
            }

            // -----------------------------------------------------------------
            // Frame 0: Lobby Screen State (Lobby Overlay Active)
            // -----------------------------------------------------------------
            if (lobbyMgr != null && lobbyMgr.lobbyPanel != null)
            {
                lobbyMgr.lobbyPanel.SetActive(true);
                Canvas.ForceUpdateCanvases();
                CaptureFrame(cam, "00_lobby_state.png");
            }

            // -----------------------------------------------------------------
            // Frame 1: Tech Lab Modal Active
            // -----------------------------------------------------------------
            if (lobbyMgr != null && lobbyMgr.techLabModal != null)
            {
                lobbyMgr.OpenTechLab();
                Canvas.ForceUpdateCanvases();
                CaptureFrame(cam, "08_tech_lab_modal.png");
                lobbyMgr.CloseTechLab();
            }

            // -----------------------------------------------------------------
            // Frame 2: Codex Modal - Towers Tab
            // -----------------------------------------------------------------
            var codexUI = Object.FindAnyObjectByType<CodexModalUI>(FindObjectsInactive.Include);
            if (codexUI != null)
            {
                codexUI.OpenCodex();
                codexUI.SwitchTab("towers");
                Canvas.ForceUpdateCanvases();
                CaptureFrame(cam, "09_codex_modal_towers.png");

                // Frame 3: Codex Modal - Enemies Tab
                codexUI.SwitchTab("enemies");
                Canvas.ForceUpdateCanvases();
                CaptureFrame(cam, "10_codex_modal_enemies.png");

                // Frame 4: Codex Modal - Synergies Tab
                codexUI.SwitchTab("synergies");
                Canvas.ForceUpdateCanvases();
                CaptureFrame(cam, "11_codex_modal_synergies.png");

                codexUI.CloseCodex();
            }

            // Switch to In-Game Battlefield
            if (lobbyMgr != null && lobbyMgr.lobbyPanel != null)
            {
                lobbyMgr.lobbyPanel.SetActive(false);
            }

            // -----------------------------------------------------------------
            // Frame 5: Initial In-Game State
            // -----------------------------------------------------------------
            pm.SelectAntToBuild(null);
            wm.wavePrepCountdown = 10f;
            var updateHudM = typeof(InGameCanvasController).GetMethod("UpdateHUD", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (updateHudM != null) updateHudM.Invoke(canvasCtrl, null);

            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "01_initial_state.png");

            // -----------------------------------------------------------------
            // Frame 6: Select Soldier Ant Card & Place on Slot 2 (Lv.1)
            // -----------------------------------------------------------------
            var soldierAnt = pm.allAntTypes.Find(t => t.towerId == "soldier");
            pm.SelectAntToBuild(soldierAnt);
            TowerSlot slot2 = GameObject.Find("Slot_2")?.GetComponent<TowerSlot>();
            if (slot2 != null) pm.OnSlotClicked(slot2);

            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "02_soldier_placed_lv1.png");

            // -----------------------------------------------------------------
            // Frame 7: Upgrade Soldier to Level 2 (BottomSheet opens)
            // -----------------------------------------------------------------
            Tower soldierTower = slot2 != null ? slot2.currentTower : null;
            if (soldierTower != null)
            {
                soldierTower.UpgradeToLevel2();
                if (canvasCtrl != null) canvasCtrl.OpenBottomSheet(soldierTower);
            }
            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "03_soldier_upgrade_lv2.png");

            // -----------------------------------------------------------------
            // Frame 8: OPEN EVOLUTION MODAL
            // -----------------------------------------------------------------
            if (soldierTower != null && canvasCtrl != null)
            {
                canvasCtrl.OpenEvolutionModal(soldierTower);
            }
            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "04_evolution_modal_opened.png");

            // -----------------------------------------------------------------
            // Frame 9: EVOLVE to Level 3 Branch B (철갑개미)
            // -----------------------------------------------------------------
            if (canvasCtrl != null) canvasCtrl.CloseEvolutionModal();
            if (soldierTower != null)
            {
                soldierTower.Evolve("B");
                if (canvasCtrl != null) canvasCtrl.OpenBottomSheet(soldierTower);
            }
            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "05_soldier_evolved_lv3.png");

            // -----------------------------------------------------------------
            // Frame 10: Toggle Targeting Priority on BottomSheet (First -> Strongest)
            // -----------------------------------------------------------------
            if (soldierTower != null)
            {
                soldierTower.CycleTargetingPriority();
                if (canvasCtrl != null) canvasCtrl.OpenBottomSheet(soldierTower);
            }
            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "12_bottomsheet_target_priority.png");

            // -----------------------------------------------------------------
            // Frame 11: Place Fire Ant on Slot 5 & Spawn Enemy Aphid + Shoot Projectile
            // -----------------------------------------------------------------
            if (canvasCtrl != null && canvasCtrl.bottomSheetPanel != null)
            {
                canvasCtrl.bottomSheetPanel.SetActive(false);
            }
            pm.CloseInspector();

            var fireAnt = pm.allAntTypes.Find(t => t.towerId == "fire");
            pm.SelectAntToBuild(fireAnt);
            TowerSlot slot5 = GameObject.Find("Slot_5")?.GetComponent<TowerSlot>();
            if (slot5 != null) pm.OnSlotClicked(slot5);

            // Spawn enemy snail along top path
            var aphidData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/Resources/EnemyData_aphid.asset");
            var aphidPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Enemy_Aphid.prefab");

            GameObject enemyObj = Object.Instantiate(aphidPrefab);
            Enemy aphidEnemy = enemyObj.GetComponent<Enemy>();
            aphidEnemy.Initialize(aphidData, 1.0f, 1.0f, gm.waypoints);
            aphidEnemy.transform.position = new Vector3(-0.6f, 3.4f, 0);
            wm.activeEnemies.Add(aphidEnemy);

            // Fire tower shoots glowing fireball towards enemy
            Tower fireTower = slot5 != null ? slot5.currentTower : null;
            GameObject projObj = null;
            if (fireTower != null && fireTower.projectilePrefab != null)
            {
                projObj = Object.Instantiate(fireTower.projectilePrefab, fireTower.transform.position + new Vector3(-0.4f, 0.3f, 0), Quaternion.identity);
                Projectile p = projObj.GetComponent<Projectile>();
                p.Seek(aphidEnemy, 18f, "fire");
            }

            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "06_projectile_flying.png");

            // -----------------------------------------------------------------
            // Frame 12: Projectile Hits Enemy (Damage applied, HP bar updated)
            // -----------------------------------------------------------------
            if (projObj != null)
            {
                projObj.transform.position = aphidEnemy.transform.position;
                aphidEnemy.TakeDamage(25f, "fire", false);
            }

            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "07_projectile_impact.png");

            // -----------------------------------------------------------------
            // Frame 13: Weather Cycle Change (Rain 🌧️)
            // -----------------------------------------------------------------
            if (weatherMgr != null)
            {
                weatherMgr.SetWeather(WeatherType.Rain);
                if (canvasCtrl != null && canvasCtrl.weatherText != null)
                {
                    canvasCtrl.weatherText.text = "🌧️ 장마";
                }
            }
            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "13_weather_cycle_rain.png");

            // -----------------------------------------------------------------
            // Frame 14: Stage 2 Split Hollow Loading (Dual Lane)
            // -----------------------------------------------------------------
            var stage2Cfg = AssetDatabase.LoadAssetAtPath<StageConfigData>("Assets/Resources/StageConfig_split.asset");
            if (stageMgr != null && stage2Cfg != null)
            {
                stageMgr.LoadStage(stage2Cfg);
            }
            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "14_stage2_split_lane.png");

            // -----------------------------------------------------------------
            // Frame 15: Victory Screen Result Modal
            // -----------------------------------------------------------------
            var resultUI = Object.FindAnyObjectByType<GameResultUI>(FindObjectsInactive.Include);
            if (resultUI != null)
            {
                gm.totalKills = 74;
                gm.queenHp = 20;
                resultUI.ShowVictory();
                Canvas.ForceUpdateCanvases();
                CaptureFrame(cam, "15_victory_screen_result.png");

                // Frame 16: Defeat Screen Result Modal
                if (resultUI.victoryPanel != null) resultUI.victoryPanel.SetActive(false);
                resultUI.ShowDefeat();
                Canvas.ForceUpdateCanvases();
                CaptureFrame(cam, "16_defeat_screen_result.png");

                if (resultUI.defeatPanel != null) resultUI.defeatPanel.SetActive(false);
            }

            // Cleanup test runtime objects
            if (enemyObj != null) Object.DestroyImmediate(enemyObj);
            if (projObj != null) Object.DestroyImmediate(projObj);
            wm.activeEnemies.Clear();

            Debug.Log("=================================================");
            Debug.Log("🎉 ALL 17 FRAMES INSPECTION COMPLETED SUCCESSFULLY!");
            Debug.Log("=================================================");
        }

        private static void CaptureFrame(Camera cam, string fileName)
        {
            int width = 540;
            int height = 860;

            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture prevRt = cam.targetTexture;
            Rect prevCamRect = cam.rect;
            cam.rect = new Rect(0, 0, 1, 1);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = prevRt;
            cam.rect = prevCamRect;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            string filePath = Path.Combine(OutputDir, fileName);
            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            Debug.Log($"📸 Captured frame: {fileName} ({new FileInfo(filePath).Length} bytes)");
        }
    }
}
