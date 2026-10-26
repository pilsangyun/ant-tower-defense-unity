using UnityEditor;
using UnityEngine;
using System.IO;
using AntTD.Data;
using AntTD.Entities;
using AntTD.Managers;
using AntTD.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor.SceneManagement;

namespace AntTD.EditorScripts
{
    [InitializeOnLoad]
    public static class FullPortingAutomator
    {
        static FullPortingAutomator()
        {
            EditorApplication.delayCall += ExecuteFullPorting;
        }

        [MenuItem("AntTD/▶ FULL PORTING (100% COMPLETE EVERYTHING)")]
        public static void Run()
        {
            ExecuteFullPorting();
        }

        public static void ExecuteFullPorting()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            // Clean up any legacy or broken objects
            GameObject oldStarter = GameObject.Find("AutoWaveStarter");
            if (oldStarter != null) Object.DestroyImmediate(oldStarter);

            // 1. Configure Background, Pad Ring, and UI Sprites
            ConfigureSprite("Assets/Sprites/stage1_bg_540x860.png", 100);
            ConfigureSprite("Assets/Sprites/pad_selection_ring.png", 200);

            // Configure 9-Sliced UI Sprites
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_card_normal.png", new Vector4(14, 14, 14, 14));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_card_active.png", new Vector4(14, 14, 14, 14));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_btn_primary.png", new Vector4(10, 10, 10, 10));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_btn_secondary.png", new Vector4(10, 10, 10, 10));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_badge_gold.png", new Vector4(10, 10, 10, 10));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_badge_blue.png", new Vector4(10, 10, 10, 10));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_hp_bg.png", new Vector4(5, 5, 5, 5));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_hp_fill.png", new Vector4(5, 5, 5, 5));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_sheet_panel.png", new Vector4(18, 5, 18, 18));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_dock_bg.png", new Vector4(4, 4, 4, 6));
            ConfigureSlicedSprite("Assets/Sprites/UI/ui_hud_bg.png", new Vector4(4, 6, 4, 4));
            ConfigureSprite("Assets/Sprites/UI/ui_skill_circle.png", 100);

            // Configure Icons
            string[] icons = new string[] { "icon_sugar", "icon_protein", "icon_queen_crown", "icon_weather_sun", "icon_skill_swarm", "icon_skill_acid", "icon_skill_honey" };
            foreach (var ic in icons)
            {
                ConfigureSprite($"Assets/Sprites/UI/{ic}.png", 100);
            }

            // Configure Units & Evolved Sprites
            string[] unitSprites = new string[]
            {
                "enemy_snail", "enemy_caterpillar", "enemy_ladybug", "enemy_spider", "boss_beetle", "boss_mantis",
                "fire_ant", "acid_ant", "soldier_ant", "soldier_ant_evolved", "mage_ant", "worker_ant", "worker_ant_evolved", "exploder_ant"
            };

            Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();
            foreach (var sName in unitSprites)
            {
                string path = $"Assets/Sprites/{sName}.png";
                ConfigureSprite(path, 256);
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sp != null) spriteMap[sName] = sp;
            }

            Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/pad_selection_ring.png");

            // 2. Camera Setup (Fit 540x860 portrait perfectly with Letterbox)
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 4.3f;
            mainCam.transform.position = new Vector3(0, 0, -10);
            mainCam.backgroundColor = new Color(0.08f, 0.12f, 0.08f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;

            if (mainCam.GetComponent<LetterboxCamera>() == null)
            {
                mainCam.gameObject.AddComponent<LetterboxCamera>();
            }

            if (mainCam.GetComponent<Physics2DRaycaster>() == null)
            {
                mainCam.gameObject.AddComponent<Physics2DRaycaster>();
            }

            // 3. EventSystem (Essential for UI and Button clicks)
            EventSystem es = Object.FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                es = esObj.AddComponent<EventSystem>();
                esObj.AddComponent<InputSystemUIInputModule>();
            }
            else
            {
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }

            // 4. Background Setup
            GameObject bgObj = GameObject.Find("Background");
            if (bgObj == null) bgObj = new GameObject("Background");
            SpriteRenderer bgRenderer = bgObj.GetComponent<SpriteRenderer>();
            if (bgRenderer == null) bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/stage1_bg_540x860.png");
            if (bgSprite != null)
            {
                bgRenderer.sprite = bgSprite;
                bgRenderer.sortingOrder = -20;
            }
            bgObj.transform.position = Vector3.zero;

            // 5. Managers
            GameObject gmObj = GameObject.Find("GameManager");
            if (gmObj == null) gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.GetComponent<GameManager>();
            if (gm == null) gm = gmObj.AddComponent<GameManager>();

            GameObject pmObj = GameObject.Find("PlacementManager");
            if (pmObj == null) pmObj = new GameObject("PlacementManager");
            PlacementManager pm = pmObj.GetComponent<PlacementManager>();
            if (pm == null) pm = pmObj.AddComponent<PlacementManager>();

            GameObject wmObj = GameObject.Find("WaveManager");
            if (wmObj == null) wmObj = new GameObject("WaveManager");
            WaveManager wm = wmObj.GetComponent<WaveManager>();
            if (wm == null) wm = wmObj.AddComponent<WaveManager>();

            // Global Input Raycaster
            if (gmObj.GetComponent<GlobalInputRaycaster>() == null)
            {
                gmObj.AddComponent<GlobalInputRaycaster>();
            }

            // 6. Slots Setup (11 Stage 1 Stone Mound Slots)
            Vector2[] slotCanvasPositions = new Vector2[]
            {
                new Vector2(486, 86),
                new Vector2(268, 164),
                new Vector2(45, 248),
                new Vector2(492, 260),
                new Vector2(268, 314),
                new Vector2(45, 398),
                new Vector2(492, 402),
                new Vector2(268, 462),
                new Vector2(45, 552),
                new Vector2(268, 598),
                new Vector2(492, 668)
            };

            GameObject slotsContainer = GameObject.Find("SlotsContainer");
            if (slotsContainer == null) slotsContainer = new GameObject("SlotsContainer");

            for (int i = 0; i < slotCanvasPositions.Length; i++)
            {
                int slotId = i + 1;
                string slotName = $"Slot_{slotId}";
                Transform existing = slotsContainer.transform.Find(slotName);
                GameObject slotObj = (existing != null) ? existing.gameObject : new GameObject(slotName);
                slotObj.transform.SetParent(slotsContainer.transform);

                float wx = (slotCanvasPositions[i].x - 270f) / 100f;
                float wy = (430f - slotCanvasPositions[i].y) / 100f;
                slotObj.transform.position = new Vector3(wx, wy, 0);

                TowerSlot slot = slotObj.GetComponent<TowerSlot>();
                if (slot == null) slot = slotObj.AddComponent<TowerSlot>();
                slot.slotId = slotId;

                CircleCollider2D col = slotObj.GetComponent<CircleCollider2D>();
                if (col == null) col = slotObj.AddComponent<CircleCollider2D>();
                col.radius = 0.45f;

                // Child Selection Ring
                Transform ringTransform = slotObj.transform.Find("RingHighlight");
                GameObject ringObj = (ringTransform != null) ? ringTransform.gameObject : new GameObject("RingHighlight");
                ringObj.transform.SetParent(slotObj.transform);
                ringObj.transform.localPosition = Vector3.zero;
                ringObj.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

                SpriteRenderer ringSr = ringObj.GetComponent<SpriteRenderer>();
                if (ringSr == null) ringSr = ringObj.AddComponent<SpriteRenderer>();
                ringSr.sprite = ringSprite;
                ringSr.sortingOrder = -5;
                ringSr.enabled = false;
                slot.ringHighlightRenderer = ringSr;
            }

            // 7. Ensure Resources Folder
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // 8. Create Projectile Prefab
            GameObject projPrefab = CreateProjectilePrefab();

            // 9. Create All 6 Enemy Types Data & Prefabs
            var aphidData = CreateEnemyData("aphid", "달팽이", 45f, 0.85f, 0f, 12, 0, false, spriteMap.GetValueOrDefault("enemy_snail"));
            var ladybugData = CreateEnemyData("ladybug", "무당벌레", 240f, 0.45f, 0.7f, 35, 1, false, spriteMap.GetValueOrDefault("enemy_ladybug"));
            var spiderData = CreateEnemyData("spider", "거미", 170f, 1.1f, 0.15f, 30, 1, false, spriteMap.GetValueOrDefault("enemy_spider"));
            var moleData = CreateEnemyData("mole", "뿔애벌레", 130f, 0.75f, 0.1f, 25, 0, false, spriteMap.GetValueOrDefault("enemy_caterpillar"));
            var beetleBossData = CreateEnemyData("beetle_boss", "풍뎅이 타이탄", 2200f, 0.35f, 0.8f, 250, 5, true, spriteMap.GetValueOrDefault("boss_beetle"));
            var mantisBossData = CreateEnemyData("mantis_boss", "사마귀 맹장 타이탄", 3500f, 0.5f, 0.4f, 400, 8, true, spriteMap.GetValueOrDefault("boss_mantis"));

            Dictionary<string, GameObject> enemyPrefabs = new Dictionary<string, GameObject>();
            enemyPrefabs["aphid"] = CreateEnemyPrefab("Enemy_Aphid", aphidData, spriteMap.GetValueOrDefault("enemy_snail"), 0.65f);
            enemyPrefabs["ladybug"] = CreateEnemyPrefab("Enemy_Ladybug", ladybugData, spriteMap.GetValueOrDefault("enemy_ladybug"), 0.70f);
            enemyPrefabs["spider"] = CreateEnemyPrefab("Enemy_Spider", spiderData, spriteMap.GetValueOrDefault("enemy_spider"), 0.70f);
            enemyPrefabs["mole"] = CreateEnemyPrefab("Enemy_Mole", moleData, spriteMap.GetValueOrDefault("enemy_caterpillar"), 0.70f);
            enemyPrefabs["beetle_boss"] = CreateEnemyPrefab("Enemy_BeetleBoss", beetleBossData, spriteMap.GetValueOrDefault("boss_beetle"), 1.10f);
            enemyPrefabs["mantis_boss"] = CreateEnemyPrefab("Enemy_MantisBoss", mantisBossData, spriteMap.GetValueOrDefault("boss_mantis"), 1.15f);

            // 10. Create 15 Stage Waves Data
            StageWaveData stageData = CreateFull15Waves(aphidData, ladybugData, spiderData, moleData, beetleBossData, mantisBossData);

            // 11. Create All 6 Tower Types Data & Prefabs
            var workerData = CreateTowerData("worker", "일개미", AntRole.ResourceSupport, 50, 60, 1.2f, 0f, 1.0f, 0, spriteMap.GetValueOrDefault("worker_ant"), "가위개미", "꿀단지개미", spriteMap);
            var soldierData = CreateTowerData("soldier", "병정개미", AntRole.MeleeBlock, 80, 95, 0.95f, 28f, 1.1f, 1, spriteMap.GetValueOrDefault("soldier_ant"), "군단개미", "철갑개미", spriteMap);
            var fireData = CreateTowerData("fire", "불개미", AntRole.RangedRapid, 100, 120, 2.0f, 18f, 2.5f, 0, spriteMap.GetValueOrDefault("fire_ant"), "맹독 불개미", "화염방사 개미", spriteMap);
            var acidData = CreateTowerData("acid", "산성개미", AntRole.RangedCorrosion, 120, 145, 2.5f, 38f, 0.7f, 0, spriteMap.GetValueOrDefault("acid_ant"), "부식 산성개미", "포자개미", spriteMap);
            var exploderData = CreateTowerData("exploder", "자폭개미", AntRole.SpecialNuke, 150, 180, 1.1f, 180f, 0.2f, 0, spriteMap.GetValueOrDefault("exploder_ant"), "연쇄자폭 개미", "신경독 개미", spriteMap);
            var pheromoneData = CreateTowerData("pheromone", "페로몬개미", AntRole.DebuffSupport, 130, 155, 1.8f, 0f, 1.5f, 0, spriteMap.GetValueOrDefault("mage_ant"), "환각 페로몬", "광폭 페로몬", spriteMap);

            GameObject baseTowerPrefab = CreateTowerPrefab("Tower_Base", spriteMap.GetValueOrDefault("fire_ant"), projPrefab);

            // 12. Wire into Scene Managers
            gm.currentStageData = stageData;
            gm.sugar = 350;
            gm.queenHp = 20;

            wm.enemyPrefab = enemyPrefabs["aphid"];
            wm.enemyPrefabList = new List<WaveManager.EnemyPrefabEntry>();
            foreach (var kvp in enemyPrefabs)
            {
                wm.enemyPrefabList.Add(new WaveManager.EnemyPrefabEntry { enemyId = kvp.Key, prefab = kvp.Value });
            }
            wm.InitPrefabMap();

            pm.defaultTowerPrefab = baseTowerPrefab;
            List<TowerData> antList = new List<TowerData> { workerData, soldierData, fireData, acidData, exploderData, pheromoneData };
            pm.allAntTypes = antList;
            pm.SelectAntToBuild(fireData); // Pre-select Fire Ant

            // 13. Build Pixel-Perfect UI
            PixelPerfectUICreator.BuildCompletePixelPerfectUI(antList);

            // 14. Save Scene and Assets
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SampleScene.unity");
            AssetDatabase.SaveAssets();

            Debug.Log("🎉 [AntTD] Pixel-Perfect HTML-Match Full Porting 100% Complete & Saved!");
        }

        private static EnemyData CreateEnemyData(string id, string name, float hp, float speed, float armor, int sugar, int prot, bool isBoss, Sprite icon)
        {
            string path = $"Assets/Resources/EnemyData_{id}.asset";
            EnemyData data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(data, path);
            }
            data.enemyId = id;
            data.enemyName = name;
            data.baseHp = hp;
            data.baseSpeed = speed;
            data.baseArmor = armor;
            data.sugarReward = sugar;
            data.proteinReward = prot;
            data.isBoss = isBoss;
            data.radius = isBoss ? 0.6f : 0.35f;
            data.sprite = icon;
            EditorUtility.SetDirty(data);
            return data;
        }

        private static GameObject CreateEnemyPrefab(string prefabName, EnemyData data, Sprite sprite, float scale)
        {
            string path = $"Assets/Resources/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) AssetDatabase.DeleteAsset(path);

            GameObject obj = new GameObject(prefabName);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;
            obj.transform.localScale = new Vector3(scale, scale, 1f);
            obj.AddComponent<CircleCollider2D>().radius = 0.35f;
            Enemy e = obj.AddComponent<Enemy>();
            e.data = data;

            prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static TowerData CreateTowerData(string id, string name, AntRole role, int cost, int upCost, float range, float dmg, float spd, int blocks, Sprite icon, string evoA, string evoB, Dictionary<string, Sprite> spriteMap)
        {
            string path = $"Assets/Resources/TowerData_{id}.asset";
            TowerData data = AssetDatabase.LoadAssetAtPath<TowerData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<TowerData>();
                AssetDatabase.CreateAsset(data, path);
            }
            data.towerId = id;
            data.antName = name;
            data.role = role;
            data.baseCost = cost;
            data.upgradeCost = upCost;
            data.baseRange = range;
            data.baseDamage = dmg;
            data.baseAttackSpeed = spd;
            data.baseBlockCount = blocks;
            data.icon = icon;

            data.branchA = new TowerEvolutionBranch { name = evoA, sugarCost = 180, proteinCost = 2 };
            data.branchB = new TowerEvolutionBranch { name = evoB, sugarCost = 180, proteinCost = 2 };

            if (id == "soldier")
            {
                data.branchA.name = "군대개미";
                data.branchA.title = "살육의 도살자";
                data.branchA.description = "적에게 출혈 디버프를 부여하며, 적 처치 시 3초간 공격 속도가 2배로 폭증합니다.";
                data.branchA.evolvedSprite = spriteMap.GetValueOrDefault("soldier_ant_evolved");
                data.branchA.damage = 55f; data.branchA.attackSpeed = 1.6f; data.branchA.blockCount = 2;

                data.branchB.name = "철갑개미";
                data.branchB.title = "불굴의 요새";
                data.branchB.description = "두꺼운 철갑으로 받는 피해를 45% 경감하며 최대 3기의 적을 저지합니다. 체력 소진 시 5초 후 재투입됩니다.";
                data.branchB.evolvedSprite = spriteMap.GetValueOrDefault("soldier_ant_evolved");
                data.branchB.damage = 42f; data.branchB.attackSpeed = 0.9f; data.branchB.blockCount = 3;
            }
            else if (id == "worker")
            {
                data.branchA.name = "가위개미";
                data.branchA.title = "버섯 농장주";
                data.branchA.description = "버섯 균사체 농장을 가꾸어 당분 생산량이 2배로 증가하고 주기적으로 단백질을 생산합니다.";
                data.branchA.evolvedSprite = spriteMap.GetValueOrDefault("worker_ant");

                data.branchB.name = "꿀단지개미";
                data.branchB.title = "살아있는 보급소";
                data.branchB.description = "거대한 꿀주머니에서 꿀을 보급하여 사거리 내 모든 아군 타워의 사거리와 공속을 대폭 증가시킵니다.";
                data.branchB.evolvedSprite = spriteMap.GetValueOrDefault("worker_ant_evolved");
            }
            else if (id == "fire")
            {
                data.branchA.name = "맹독 불개미";
                data.branchA.title = "보스 킬러";
                data.branchA.description = "보라빛 독화염을 발사하여 화상 중첩 한도를 무한대로 늘려 대형 적과 보스를 녹여버립니다.";
                data.branchA.evolvedSprite = spriteMap.GetValueOrDefault("fire_ant");
                data.branchA.damage = 26f; data.branchA.range = 1.8f; data.branchA.attackSpeed = 3.2f;

                data.branchB.name = "화염방사 개미";
                data.branchB.title = "광역 청소부";
                data.branchB.description = "전방 부채꼴 범위로 끊김 없는 화염을 연속 방사하여 몰려오는 적 떼를 일망타진합니다.";
                data.branchB.evolvedSprite = spriteMap.GetValueOrDefault("fire_ant");
                data.branchB.damage = 18f; data.branchB.range = 1.4f; data.branchB.attackSpeed = 4.5f;
            }
            else if (id == "acid")
            {
                data.branchA.name = "부식 폭격개미";
                data.branchA.title = "산성 웅덩이 폭격";
                data.branchA.description = "착탄 지점에 산성 웅덩이를 생성해 밟는 모든 적에게 지속 피해와 방어력 감소를 겁니다.";
                data.branchA.evolvedSprite = spriteMap.GetValueOrDefault("acid_ant");
                data.branchA.damage = 52f; data.branchA.range = 2.4f; data.branchA.attackSpeed = 0.8f;

                data.branchB.name = "저격 개미";
                data.branchB.title = "산성 레일건";
                data.branchB.description = "초장거리에서 경로 상의 모든 적을 일직선으로 관통하는 강력한 산성 빔을 발사합니다.";
                data.branchB.evolvedSprite = spriteMap.GetValueOrDefault("acid_ant");
                data.branchB.damage = 70f; data.branchB.range = 2.8f; data.branchB.attackSpeed = 0.5f;
            }
            else if (id == "exploder")
            {
                data.branchA.name = "연쇄폭발 개미";
                data.branchA.title = "연쇄 대참사";
                data.branchA.description = "대폭발을 일으키며, 이 폭발로 적이 사망하면 해당 위치에서 2차 폭발이 발생합니다.";
                data.branchA.evolvedSprite = spriteMap.GetValueOrDefault("exploder_ant");

                data.branchB.name = "신경독 개미";
                data.branchB.title = "군중 마비";
                data.branchB.description = "폭발 시 짙은 청록색 독가스 구름을 잔류시켜 범위 내 모든 적을 3초간 완전 기절시킵니다.";
                data.branchB.evolvedSprite = spriteMap.GetValueOrDefault("exploder_ant");
            }
            else if (id == "pheromone")
            {
                data.branchA.name = "혼란 페로몬";
                data.branchA.title = "최면 역주행";
                data.branchA.description = "주변 적 이속을 45% 늦추고, 주기적으로 역주행시킵니다.";
                data.branchA.evolvedSprite = spriteMap.GetValueOrDefault("mage_ant");

                data.branchB.name = "광란 페로몬";
                data.branchB.title = "군집 광란";
                data.branchB.description = "사거리 내 모든 아군 타워에게 공격 속도 +50%, 치명타 확률 +30%의 광란 오라를 부여합니다.";
                data.branchB.evolvedSprite = spriteMap.GetValueOrDefault("mage_ant");
            }

            EditorUtility.SetDirty(data);
            return data;
        }

        private static GameObject CreateTowerPrefab(string prefabName, Sprite sprite, GameObject projPrefab)
        {
            string path = $"Assets/Resources/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) AssetDatabase.DeleteAsset(path);

            GameObject obj = new GameObject(prefabName);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;
            obj.transform.localScale = new Vector3(0.60f, 0.60f, 1f);

            Tower t = obj.AddComponent<Tower>();
            t.projectilePrefab = projPrefab;

            prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static GameObject CreateProjectilePrefab()
        {
            string path = "Assets/Resources/Projectile_Base.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) AssetDatabase.DeleteAsset(path);

            GameObject obj = new GameObject("Projectile_Base");
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();

            Texture2D tex = new Texture2D(32, 32);
            Vector2 center = new Vector2(15.5f, 15.5f);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= 6f)
                    {
                        tex.SetPixel(x, y, new Color(1f, 0.98f, 0.8f, 1f));
                    }
                    else if (dist <= 11f)
                    {
                        float t = (dist - 6f) / 5f;
                        tex.SetPixel(x, y, Color.Lerp(new Color(1f, 0.55f, 0.1f, 1f), new Color(0.95f, 0.2f, 0.1f, 0.85f), t));
                    }
                    else if (dist <= 15f)
                    {
                        float t = (dist - 11f) / 4f;
                        tex.SetPixel(x, y, new Color(0.95f, 0.2f, 0.1f, Mathf.Lerp(0.85f, 0f, t)));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100);
            sr.sortingOrder = 25;
            obj.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

            Projectile p = obj.AddComponent<Projectile>();
            p.speed = 4.2f;

            prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            return prefab;
        }

        private static StageWaveData CreateFull15Waves(EnemyData aphid, EnemyData ladybug, EnemyData spider, EnemyData mole, EnemyData beetleBoss, EnemyData mantisBoss)
        {
            string path = "Assets/Resources/Stage1_Full15Waves.asset";
            StageWaveData data = AssetDatabase.LoadAssetAtPath<StageWaveData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<StageWaveData>();
                AssetDatabase.CreateAsset(data, path);
            }
            data.stageId = "stage1";
            data.stageName = "햇살 정원";
            data.waves.Clear();

            data.waves.Add(MakeWave(1, "정찰 달팽이", (aphid, 8, 1.0f, 0.5f)));
            data.waves.Add(MakeWave(2, "달팽이 대군단", (aphid, 14, 0.8f, 0.5f)));
            data.waves.Add(MakeWave(3, "단단한 무당벌레", (aphid, 6, 0.8f, 0.5f), (ladybug, 3, 2.0f, 4.0f)));
            data.waves.Add(MakeWave(4, "그림자 거미", (spider, 4, 1.8f, 0.5f), (aphid, 10, 0.6f, 5.0f)));
            data.waves.Add(MakeWave(5, "뿔애벌레 기습", (mole, 4, 2.2f, 0.5f), (ladybug, 3, 2.0f, 5.0f)));
            data.waves.Add(MakeWave(6, "중간 보스: 풍뎅이 타이탄", (aphid, 8, 0.7f, 0.5f), (beetleBoss, 1, 1.0f, 4.0f)));
            data.waves.Add(MakeWave(7, "거미와 무당벌레", (spider, 6, 1.4f, 0.5f), (ladybug, 5, 1.8f, 5.0f)));
            data.waves.Add(MakeWave(8, "애벌레 특공대", (mole, 6, 1.8f, 0.5f), (spider, 4, 1.2f, 6.0f)));
            data.waves.Add(MakeWave(9, "철갑 군세", (ladybug, 8, 1.6f, 0.5f), (mole, 4, 2.0f, 6.0f)));
            data.waves.Add(MakeWave(10, "쌍둥이 타이탄", (spider, 5, 1.2f, 0.5f), (beetleBoss, 2, 4.0f, 4.0f)));
            data.waves.Add(MakeWave(11, "달팽이 폭주", (spider, 10, 0.9f, 0.5f), (aphid, 20, 0.4f, 5.0f)));
            data.waves.Add(MakeWave(12, "야간 정예 강습", (mole, 8, 1.4f, 0.5f), (ladybug, 7, 1.5f, 4.0f)));
            data.waves.Add(MakeWave(13, "총력 공습", (ladybug, 10, 1.2f, 0.5f), (spider, 8, 0.8f, 5.0f)));
            data.waves.Add(MakeWave(14, "파멸의 군단", (aphid, 25, 0.35f, 0.5f), (ladybug, 8, 1.2f, 4.0f), (spider, 8, 0.9f, 7.0f)));
            data.waves.Add(MakeWave(15, "최종 보스: 사마귀 타이탄", (beetleBoss, 1, 1.0f, 1.0f), (mantisBoss, 1, 1.0f, 6.0f), (ladybug, 8, 1.2f, 10.0f)));

            EditorUtility.SetDirty(data);
            return data;
        }

        private static WaveDefinition MakeWave(int waveNum, string title, params (EnemyData data, int count, float interval, float delay)[] spawns)
        {
            WaveDefinition w = new WaveDefinition { waveNumber = waveNum, title = title };
            foreach (var sp in spawns)
            {
                w.spawns.Add(new SpawnGroup { enemyData = sp.data, count = sp.count, interval = sp.interval, delay = sp.delay });
            }
            return w;
        }

        private static void ConfigureSprite(string path, float ppu)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
                if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
                if (importer.spritePixelsPerUnit != ppu) { importer.spritePixelsPerUnit = ppu; changed = true; }
                if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; changed = true; }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }

                if (changed)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        private static void ConfigureSlicedSprite(string path, Vector4 border)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
                if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
                if (importer.spritePixelsPerUnit != 100) { importer.spritePixelsPerUnit = 100; changed = true; }
                if (importer.spriteBorder != border) { importer.spriteBorder = border; changed = true; }
                if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; changed = true; }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }

                if (changed)
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
        }
    }
}
