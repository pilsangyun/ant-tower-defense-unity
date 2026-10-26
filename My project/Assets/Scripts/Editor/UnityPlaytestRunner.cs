using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using AntTD.Data;
using AntTD.Entities;
using AntTD.Managers;
using AntTD.UI;
using AntTD.EditorScripts;

namespace AntTD.EditorScripts
{
    public static class UnityPlaytestRunner
    {
        private const string OutputDir = "c:/Users/82103/Desktop/workspace/unity_inspection";

        [MenuItem("AntTD/Run Full Inspection Playtest")]
        public static void Run()
        {
            Debug.Log("=================================================");
            Debug.Log("▶ STARTING UNITY ENHANCED PLAYTEST INSPECTION");
            Debug.Log("=================================================");

            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }

            // 1. Rebuild scene with latest sprites and UI
            FullPortingAutomator.Run();

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Main camera not found!");
                return;
            }

            GameManager gm = Object.FindAnyObjectByType<GameManager>();
            PlacementManager pm = Object.FindAnyObjectByType<PlacementManager>();
            WaveManager wm = Object.FindAnyObjectByType<WaveManager>();
            InGameCanvasController canvasCtrl = Object.FindAnyObjectByType<InGameCanvasController>();

            // Ensure Singleton instances are bound in Editor simulation
            var gmAwake = typeof(GameManager).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (gmAwake != null) gmAwake.Invoke(gm, null);

            var pmAwake = typeof(PlacementManager).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pmAwake != null) pmAwake.Invoke(pm, null);

            var wmAwake = typeof(WaveManager).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (wmAwake != null) wmAwake.Invoke(wm, null);

            if (canvasCtrl != null)
            {
                var ctrlStart = typeof(InGameCanvasController).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (ctrlStart != null) ctrlStart.Invoke(canvasCtrl, null);
            }

            // Refresh cards UI
            var cards = Object.FindObjectsByType<AntCardUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var card in cards)
            {
                var startM = typeof(AntCardUI).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (startM != null) startM.Invoke(card, null);
            }

            // -----------------------------------------------------------------
            // Frame 1: Initial Game State with Automatic 10s Wave Countdown
            // -----------------------------------------------------------------
            pm.SelectAntToBuild(null);
            wm.wavePrepCountdown = 10f;
            var updateHudM = typeof(InGameCanvasController).GetMethod("UpdateHUD", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (updateHudM != null) updateHudM.Invoke(canvasCtrl, null);

            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "01_initial_state.png");

            // -----------------------------------------------------------------
            // Frame 2: Select Soldier Ant Card & Place on Slot 2 (Lv.1)
            // -----------------------------------------------------------------
            var soldierAnt = pm.allAntTypes.Find(t => t.towerId == "soldier");
            pm.SelectAntToBuild(soldierAnt);
            TowerSlot slot2 = GameObject.Find("Slot_2")?.GetComponent<TowerSlot>();
            if (slot2 != null) pm.OnSlotClicked(slot2);

            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "02_soldier_placed_lv1.png");

            // -----------------------------------------------------------------
            // Frame 3: Upgrade Soldier to Level 2 (BottomSheet opens)
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
            // Frame 4: OPEN EVOLUTION MODAL (Shows Branch A vs Branch B full details!)
            // -----------------------------------------------------------------
            if (soldierTower != null && canvasCtrl != null)
            {
                canvasCtrl.OpenEvolutionModal(soldierTower);
            }
            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "04_evolution_modal_opened.png");

            // -----------------------------------------------------------------
            // Frame 5: EVOLVE to Level 3 Branch B (철갑개미 - Evolved Sprite + Aura!)
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
            // Frame 6: Place Fire Ant on Slot 5 & Spawn Enemy Aphid + Shoot Projectile
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
            // Frame 7: Projectile Hits Enemy (Damage applied, HP bar updated)
            // -----------------------------------------------------------------
            if (projObj != null)
            {
                projObj.transform.position = aphidEnemy.transform.position;
                aphidEnemy.TakeDamage(25f, "fire", false);
            }

            Canvas.ForceUpdateCanvases();
            CaptureFrame(cam, "07_projectile_impact.png");

            // Cleanup test runtime objects
            if (enemyObj != null) Object.DestroyImmediate(enemyObj);
            if (projObj != null) Object.DestroyImmediate(projObj);
            wm.activeEnemies.Clear();

            Debug.Log("=================================================");
            Debug.Log("✅ ENHANCED PLAYTEST INSPECTION COMPLETED SUCCESSFULLY!");
            Debug.Log("=================================================");
        }

        private static void CaptureFrame(Camera cam, string fileName)
        {
            int width = 540;
            int height = 860;

            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture prevRt = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = prevRt;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            string filePath = Path.Combine(OutputDir, fileName);
            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            Debug.Log($"📸 Captured frame: {fileName} ({new FileInfo(filePath).Length} bytes)");
        }
    }
}
