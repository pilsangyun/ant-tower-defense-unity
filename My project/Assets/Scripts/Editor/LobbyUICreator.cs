using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AntTD.UI;
using AntTD.Data;
using AntTD.Managers;

namespace AntTD.EditorScripts
{
    public static class LobbyUICreator
    {
        public static LobbyManager BuildLobbyUI(GameObject canvasObj, Transform shellTransform)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Sprite btnPrimarySp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/ui_btn_primary.png");
            Sprite btnSecondarySp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/ui_btn_secondary.png");
            Sprite badgeGoldSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/ui_badge_gold.png");
            Sprite badgeBlueSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/ui_badge_blue.png");
            Sprite sheetPanelSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/ui_sheet_panel.png");
            Sprite cardNormalSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/ui_card_normal.png");
            Sprite cardActiveSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/ui_card_active.png");

            // 1. Lobby Overlay Panel (Full AppShell 540x860)
            GameObject lobbyObj = new GameObject("LobbyOverlay", typeof(RectTransform));
            lobbyObj.transform.SetParent(shellTransform, false);
            RectTransform lr = lobbyObj.GetComponent<RectTransform>();
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.sizeDelta = Vector2.zero;
            lr.anchoredPosition = Vector2.zero;

            Image lobbyBg = lobbyObj.AddComponent<Image>();
            lobbyBg.color = new Color(0.06f, 0.08f, 0.06f, 0.98f);
            lobbyBg.raycastTarget = true; // Block clicks through to battlefield while in lobby

            LobbyManager lm = canvasObj.GetComponent<LobbyManager>();
            if (lm == null) lm = canvasObj.AddComponent<LobbyManager>();
            lm.lobbyPanel = lobbyObj;

            // 2. Header: Title & DNA Capsule
            GameObject headerObj = new GameObject("Header", typeof(RectTransform));
            headerObj.transform.SetParent(lobbyObj.transform, false);
            RectTransform hr = headerObj.GetComponent<RectTransform>();
            hr.anchorMin = new Vector2(0, 1);
            hr.anchorMax = new Vector2(1, 1);
            hr.pivot = new Vector2(0.5f, 1);
            hr.sizeDelta = new Vector2(0, 70);
            hr.anchoredPosition = new Vector2(0, -10);

            // Title Text
            CreateText(headerObj.transform, "Title", "🐜 앤트 가디언즈 (Ant Guardians)", font, 16, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(18, 12), new Vector2(-150, 24), TextAnchor.MiddleLeft);

            // Subtitle
            CreateText(headerObj.transform, "Subtitle", "HTML5 Canvas 1:1 Pixel-Perfect Unity Port", font, 10, FontStyle.Normal, new Color(0.6f, 0.7f, 0.8f), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(18, -10), new Vector2(-150, 18), TextAnchor.MiddleLeft);

            // DNA Badge Capsule
            GameObject dnaBadge = CreateSlicedImage(headerObj.transform, "DnaBadge", badgeBlueSp, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-75, 4), new Vector2(115, 26));
            lm.dnaDisplayText = CreateText(dnaBadge.transform, "Val", "0 🧬 DNA", font, 11, FontStyle.Bold, new Color(0.38f, 0.85f, 0.98f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

            // 3. Difficulty Section (5 Tabs)
            GameObject diffSection = new GameObject("DifficultySection", typeof(RectTransform));
            diffSection.transform.SetParent(lobbyObj.transform, false);
            RectTransform dr = diffSection.GetComponent<RectTransform>();
            dr.anchorMin = new Vector2(0, 1);
            dr.anchorMax = new Vector2(1, 1);
            dr.pivot = new Vector2(0.5f, 1);
            dr.sizeDelta = new Vector2(-32, 105);
            dr.anchoredPosition = new Vector2(0, -85);

            CreateText(diffSection.transform, "Label", "⚔️ 전장 작전 난이도", font, 12, FontStyle.Bold, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -8), new Vector2(0, 18), TextAnchor.MiddleLeft);

            // 5 Tabs Row
            string[] diffNames = { "쉬움", "보통", "어려움", "악몽", "무한" };
            DifficultyType[] diffTypes = { DifficultyType.Easy, DifficultyType.Normal, DifficultyType.Hard, DifficultyType.Nightmare, DifficultyType.Infinite };
            float tabW = 96f;
            float tabGap = 6f;

            lm.difficultyTabButtons.Clear();
            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                float px = (i * (tabW + tabGap)) + (tabW * 0.5f);
                Button tabBtn = CreateSlicedButton(diffSection.transform, $"Tab_{diffTypes[i]}", (i == 0 ? btnPrimarySp : btnSecondarySp), diffNames[i], font, 11, FontStyle.Bold, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(px, -36), new Vector2(tabW, 28));
                lm.difficultyTabButtons.Add(tabBtn);

                tabBtn.onClick.AddListener(() =>
                {
                    lm.SelectDifficulty(diffTypes[idx]);
                    for (int k = 0; k < lm.difficultyTabButtons.Count; k++)
                    {
                        lm.difficultyTabButtons[k].GetComponent<Image>().sprite = (k == idx) ? btnPrimarySp : btnSecondarySp;
                    }
                });
            }

            // Difficulty Detail Box
            GameObject detailBox = CreateSlicedImage(diffSection.transform, "DetailBox", sheetPanelSp, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 18), new Vector2(0, 38));
            lm.difficultyDetailText = CreateText(detailBox.transform, "Text", "입문자를 위한 완만한 전장. 넉넉한 정비 시간과 높은 환급률.", font, 10, FontStyle.Normal, new Color(0.9f, 0.95f, 1f), Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-20, 0), TextAnchor.MiddleLeft);

            // 4. Stage Cards Container
            GameObject stageSection = new GameObject("StageSection", typeof(RectTransform));
            stageSection.transform.SetParent(lobbyObj.transform, false);
            RectTransform sr = stageSection.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0, 0.5f);
            sr.anchorMax = new Vector2(1, 0.5f);
            sr.pivot = new Vector2(0.5f, 0.5f);
            sr.sizeDelta = new Vector2(-32, 380);
            sr.anchoredPosition = new Vector2(0, 30);

            CreateText(stageSection.transform, "Label", "🗺️ 작전 전장 선택", font, 12, FontStyle.Bold, Color.white, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -6), new Vector2(0, 18), TextAnchor.MiddleLeft);

            string[] stageIds = { "sunny", "split", "canyon" };
            string[] stageTitles = { "STAGE 1. 햇살 정원", "STAGE 2. 갈라진 고목", "STAGE 3. 개미지옥 협곡" };
            string[] stageBadges = { "입문용", "전략 듀얼 레인", "소용돌이 함정 기믹" };
            string[] stageDescs = {
                "5단 직각 S자 황토 길. 초심자를 위한 정원 전장.",
                "상단 2갈래 경로가 중앙에서 합류하는 전략 맵. 양면 방어 필요!",
                "중앙 모래소용돌이 함정 기믹! 적을 빨아들이고 둔화시키는 고난도 전장."
            };

            float cardH = 104f;
            lm.stageCardButtons.Clear();
            for (int i = 0; i < 3; i++)
            {
                int sIdx = i;
                float py = -32f - (i * (cardH + 10f)) - (cardH * 0.5f);
                GameObject cardObj = CreateSlicedImage(stageSection.transform, $"StageCard_{stageIds[i]}", (i == 0 ? cardActiveSp : cardNormalSp), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, py), new Vector2(504, cardH));
                Button cardBtn = cardObj.AddComponent<Button>();
                lm.stageCardButtons.Add(cardBtn);

                // Badge & Title
                CreateText(cardObj.transform, "Title", stageTitles[i], font, 13, FontStyle.Bold, (i == 0 ? new Color(0.98f, 0.75f, 0.14f) : Color.white), new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -14), new Vector2(-120, 20), TextAnchor.MiddleLeft);
                GameObject badge = CreateSlicedImage(cardObj.transform, "Badge", (i == 2 ? badgeGoldSp : badgeBlueSp), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-60, -14), new Vector2(95, 20));
                CreateText(badge.transform, "Text", stageBadges[i], font, 9, FontStyle.Bold, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

                // Description
                CreateText(cardObj.transform, "Desc", stageDescs[i], font, 10, FontStyle.Normal, new Color(0.85f, 0.9f, 0.95f), new Vector2(0, 0), new Vector2(1, 1), new Vector2(16, -20), new Vector2(-32, -40), TextAnchor.MiddleLeft);

                cardBtn.onClick.AddListener(() =>
                {
                    lm.SelectStage(stageIds[sIdx]);
                    for (int k = 0; k < lm.stageCardButtons.Count; k++)
                    {
                        lm.stageCardButtons[k].GetComponent<Image>().sprite = (k == sIdx) ? cardActiveSp : cardNormalSp;
                    }
                });
            }

            // 5. Bottom Action Bar: [연구소], [도감], [⚔️ 출진하기]
            GameObject bottomBar = new GameObject("BottomBar", typeof(RectTransform));
            bottomBar.transform.SetParent(lobbyObj.transform, false);
            RectTransform bbr = bottomBar.GetComponent<RectTransform>();
            bbr.anchorMin = new Vector2(0, 0);
            bbr.anchorMax = new Vector2(1, 0);
            bbr.pivot = new Vector2(0.5f, 0);
            bbr.sizeDelta = new Vector2(-32, 60);
            bbr.anchoredPosition = new Vector2(0, 16);

            lm.openTechLabBtn = CreateSlicedButton(bottomBar.transform, "TechBtn", btnSecondarySp, "🧬 군집 연구소", font, 11, FontStyle.Bold, Color.white, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(65, 0), new Vector2(125, 42));
            lm.openCodexBtn = CreateSlicedButton(bottomBar.transform, "CodexBtn", btnSecondarySp, "📖 생태계 도감", font, 11, FontStyle.Bold, Color.white, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(200, 0), new Vector2(125, 42));
            lm.startBattleBtn = CreateSlicedButton(bottomBar.transform, "StartBtn", btnPrimarySp, "⚔️ 출진하기 (START)", font, 13, FontStyle.Bold, Color.white, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-110, 0), new Vector2(200, 44));

            // 6. Build Tech Lab Modal Overlay
            GameObject techModal = BuildTechLabModal(shellTransform, font, btnPrimarySp, btnSecondarySp, sheetPanelSp, badgeBlueSp);
            lm.techLabModal = techModal;
            techModal.SetActive(false);

            // 7. Build Codex Modal Overlay
            GameObject codexModal = BuildCodexModal(shellTransform, font, btnPrimarySp, btnSecondarySp, sheetPanelSp, badgeBlueSp, badgeGoldSp);
            lm.codexModal = codexModal;
            codexModal.SetActive(false);

            return lm;
        }

        private static GameObject BuildTechLabModal(Transform parent, Font font, Sprite btnPrimarySp, Sprite btnSecondarySp, Sprite sheetPanelSp, Sprite badgeBlueSp)
        {
            GameObject modalObj = new GameObject("TechLabModal", typeof(RectTransform));
            modalObj.transform.SetParent(parent, false);
            RectTransform rt = modalObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            Image bg = modalObj.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.05f, 0.98f);
            bg.raycastTarget = true;

            // Panel Container
            GameObject panel = CreateSlicedImage(modalObj.transform, "Panel", sheetPanelSp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(510, 780));

            // Header
            CreateText(panel.transform, "Title", "🧬 군집 DNA 연구소", font, 16, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -22), new Vector2(-150, 28), TextAnchor.MiddleLeft);
            CreateText(panel.transform, "Sub", "DNA를 소모하여 영구적 군집 패시브를 해금하세요.", font, 10, FontStyle.Normal, new Color(0.7f, 0.8f, 0.9f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -46), new Vector2(-150, 18), TextAnchor.MiddleLeft);

            // Close Button
            Button closeBtn = CreateSlicedButton(panel.transform, "CloseBtn", btnSecondarySp, "✕ 닫기", font, 11, FontStyle.Bold, Color.white, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-48, -25), new Vector2(68, 30));

            // Available DNA Capsule
            GameObject dnaBadge = CreateSlicedImage(panel.transform, "DnaBadge", badgeBlueSp, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-130, -25), new Vector2(85, 26));
            Text dnaValText = CreateText(dnaBadge.transform, "Val", "0 🧬", font, 11, FontStyle.Bold, new Color(0.38f, 0.85f, 0.98f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

            // 12 Nodes Container Scroll Area
            GameObject contentObj = new GameObject("NodesGrid", typeof(RectTransform));
            contentObj.transform.SetParent(panel.transform, false);
            RectTransform crt = contentObj.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 0);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(-30, -140);
            crt.anchoredPosition = new Vector2(0, -20);

            // TechLabManager Component Setup
            TechLabManager techMgr = modalObj.AddComponent<TechLabManager>();
            techMgr.techLabModal = modalObj;
            techMgr.availableDnaText = dnaValText;
            techMgr.techNodesContainer = contentObj.transform;
            techMgr.closeBtn = closeBtn;

            // Wire Reset Button
            Button resetBtn = CreateSlicedButton(panel.transform, "ResetBtn", btnSecondarySp, "↺ 특성 초기화", font, 10, FontStyle.Bold, new Color(1f, 0.5f, 0.5f), new Vector2(0, 0), new Vector2(0, 0), new Vector2(68, 30), new Vector2(100, 32));
            resetBtn.onClick.AddListener(techMgr.ResetAllTech);

            // Render 12 Node Cards
            techMgr.InitDefaultNodesIfEmpty();
            techMgr.RenderNodesUI();

            closeBtn.onClick.AddListener(() =>
            {
                modalObj.SetActive(false);
                if (LobbyManager.Instance != null) LobbyManager.Instance.UpdateDnaDisplay();
            });

            return modalObj;
        }

        private static GameObject BuildCodexModal(Transform parent, Font font, Sprite btnPrimarySp, Sprite btnSecondarySp, Sprite sheetPanelSp, Sprite badgeBlueSp, Sprite badgeGoldSp)
        {
            GameObject modalObj = new GameObject("CodexModal", typeof(RectTransform));
            modalObj.transform.SetParent(parent, false);
            RectTransform rt = modalObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            Image bg = modalObj.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.05f, 0.98f);
            bg.raycastTarget = true;

            // Panel Container
            GameObject panel = CreateSlicedImage(modalObj.transform, "Panel", sheetPanelSp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(510, 780));

            // Header
            CreateText(panel.transform, "Title", "📖 생태계 도감", font, 16, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -22), new Vector2(-150, 28), TextAnchor.MiddleLeft);

            // Close Button
            Button closeBtn = CreateSlicedButton(panel.transform, "CloseBtn", btnSecondarySp, "✕ 닫기", font, 11, FontStyle.Bold, Color.white, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-48, -22), new Vector2(68, 30));

            // 3 Tabs Row
            Button tabTowers = CreateSlicedButton(panel.transform, "TabTowers", btnPrimarySp, "🐜 아군 개미 (18종)", font, 10, FontStyle.Bold, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(85, -60), new Vector2(140, 30));
            Button tabEnemies = CreateSlicedButton(panel.transform, "TabEnemies", btnSecondarySp, "🦗 침입 곤충 (7종)", font, 10, FontStyle.Bold, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(235, -60), new Vector2(140, 30));
            Button tabSynergies = CreateSlicedButton(panel.transform, "TabSynergies", btnSecondarySp, "⚡ 진형 시너지", font, 10, FontStyle.Bold, Color.white, new Vector2(0, 1), new Vector2(0, 1), new Vector2(385, -60), new Vector2(140, 30));

            // Content Area Title
            Text secTitle = CreateText(panel.transform, "SecTitle", "🐜 군집 아군 개미 도감 (기본 6종 + 특화 진화체 12종)", font, 11, FontStyle.Bold, new Color(0.9f, 0.95f, 1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -96), new Vector2(-40, 20), TextAnchor.MiddleLeft);

            // Scroll Area
            GameObject contentObj = new GameObject("CodexGrid", typeof(RectTransform));
            contentObj.transform.SetParent(panel.transform, false);
            RectTransform crt = contentObj.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 0);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(-30, -180);
            crt.anchoredPosition = new Vector2(0, -35);

            CodexModalUI codexUI = modalObj.AddComponent<CodexModalUI>();
            codexUI.codexModalPanel = modalObj;
            codexUI.closeBtn = closeBtn;
            codexUI.tabTowersBtn = tabTowers;
            codexUI.tabEnemiesBtn = tabEnemies;
            codexUI.tabSynergiesBtn = tabSynergies;
            codexUI.contentGrid = contentObj.transform;
            codexUI.sectionTitleText = secTitle;

            tabTowers.onClick.AddListener(() =>
            {
                tabTowers.GetComponent<Image>().sprite = btnPrimarySp;
                tabEnemies.GetComponent<Image>().sprite = btnSecondarySp;
                tabSynergies.GetComponent<Image>().sprite = btnSecondarySp;
            });
            tabEnemies.onClick.AddListener(() =>
            {
                tabTowers.GetComponent<Image>().sprite = btnSecondarySp;
                tabEnemies.GetComponent<Image>().sprite = btnPrimarySp;
                tabSynergies.GetComponent<Image>().sprite = btnSecondarySp;
            });
            tabSynergies.onClick.AddListener(() =>
            {
                tabTowers.GetComponent<Image>().sprite = btnSecondarySp;
                tabEnemies.GetComponent<Image>().sprite = btnSecondarySp;
                tabSynergies.GetComponent<Image>().sprite = btnPrimarySp;
            });

            closeBtn.onClick.AddListener(() => modalObj.SetActive(false));

            return modalObj;
        }

        private static GameObject CreateSlicedImage(Transform parent, string name, Sprite sp, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = obj.AddComponent<Image>();
            img.sprite = sp;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            return obj;
        }

        private static Text CreateText(Transform parent, string name, string content, Font font, int fontSize, FontStyle style, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, TextAnchor align)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform));
            textObj.transform.SetParent(parent, false);
            RectTransform tr = textObj.GetComponent<RectTransform>();
            tr.anchorMin = anchorMin;
            tr.anchorMax = anchorMax;
            tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = pos;
            tr.sizeDelta = size;

            Text t = textObj.AddComponent<Text>();
            t.text = content;
            t.font = font;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            return t;
        }

        private static Button CreateSlicedButton(Transform parent, string name, Sprite bgSprite, string label, Font font, int fontSize, FontStyle style, Color textColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);
            RectTransform br = btnObj.GetComponent<RectTransform>();
            br.anchorMin = anchorMin;
            br.anchorMax = anchorMax;
            br.pivot = new Vector2(0.5f, 0.5f);
            br.anchoredPosition = pos;
            br.sizeDelta = size;

            Image img = btnObj.AddComponent<Image>();
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;
            img.raycastTarget = true;

            Button btn = btnObj.AddComponent<Button>();

            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = cb;

            CreateText(btnObj.transform, "Text", label, font, fontSize, style, textColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            return btn;
        }
    }
}
