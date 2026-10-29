using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AntTD.UI;
using AntTD.Data;
using AntTD.Managers;
using System.Collections.Generic;

namespace AntTD.EditorScripts
{
    public static class PixelPerfectUICreator
    {
        public static InGameCanvasController BuildCompletePixelPerfectUI(List<TowerData> antList)
        {
            GameObject oldCanvas = GameObject.Find("PixelPerfectCanvas");
            if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);

            Camera mainCam = Camera.main;

            // Load UI Sprites
            Sprite cardNormalSp = LoadUISprite("ui_card_normal");
            Sprite cardActiveSp = LoadUISprite("ui_card_active");
            Sprite btnPrimarySp = LoadUISprite("ui_btn_primary");
            Sprite btnSecondarySp = LoadUISprite("ui_btn_secondary");
            Sprite badgeGoldSp = LoadUISprite("ui_badge_gold");
            Sprite badgeBlueSp = LoadUISprite("ui_badge_blue");
            Sprite hpBgSp = LoadUISprite("ui_hp_bg");
            Sprite hpFillSp = LoadUISprite("ui_hp_fill");
            Sprite skillCircleSp = LoadUISprite("ui_skill_circle");
            Sprite sheetPanelSp = LoadUISprite("ui_sheet_panel");
            Sprite dockBgSp = LoadUISprite("ui_dock_bg");
            Sprite hudBgSp = LoadUISprite("ui_hud_bg");

            // Load Icons
            Sprite iconSugarSp = LoadUISprite("icon_sugar");
            Sprite iconProteinSp = LoadUISprite("icon_protein");
            Sprite iconCrownSp = LoadUISprite("icon_queen_crown");
            Sprite iconSunSp = LoadUISprite("icon_weather_sun");
            Sprite iconSwarmSp = LoadUISprite("icon_skill_swarm");
            Sprite iconAcidSp = LoadUISprite("icon_skill_acid");
            Sprite iconHoneySp = LoadUISprite("icon_skill_honey");

            // 1. Create Canvas
            GameObject canvasObj = new GameObject("PixelPerfectCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCam;
            canvas.planeDistance = 10f;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 860);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            InGameCanvasController ctrl = canvasObj.AddComponent<InGameCanvasController>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // -------------------------------------------------------------------------
            // Centered 540x860 App-Shell Container
            // -------------------------------------------------------------------------
            GameObject shellObj = CreateUIObject("AppShellContent", canvasObj.transform);
            RectTransform shellRect = shellObj.GetComponent<RectTransform>();
            shellRect.anchorMin = new Vector2(0.5f, 0.5f);
            shellRect.anchorMax = new Vector2(0.5f, 0.5f);
            shellRect.pivot = new Vector2(0.5f, 0.5f);
            shellRect.sizeDelta = new Vector2(540, 860);
            shellRect.anchoredPosition = Vector2.zero;

            AspectRatioFitter arf = shellObj.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            arf.aspectRatio = 540f / 860f;

            // -------------------------------------------------------------------------
            // 2. TOP HUD (58px Height inside AppShell)
            // -------------------------------------------------------------------------
            GameObject hudObj = CreateUIObject("TopHUD", shellObj.transform);
            RectTransform hudRect = hudObj.GetComponent<RectTransform>();
            SetAnchors(hudRect, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(0, 58));
            Image hudImg = hudObj.AddComponent<Image>();
            hudImg.sprite = hudBgSp;
            hudImg.type = Image.Type.Sliced;

            // --- Row 1: Badges & Buttons ---
            GameObject row1Obj = CreateUIObject("Row1", hudObj.transform);
            RectTransform r1 = row1Obj.GetComponent<RectTransform>();
            SetAnchors(r1, new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(8, 0), new Vector2(-16, 0));

            // Wave Badge (Gold)
            GameObject waveBadgeObj = CreateSlicedImage(row1Obj.transform, "WaveBadge", badgeGoldSp, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(46, 0), new Vector2(92, 22));
            ctrl.waveText = CreateText(waveBadgeObj.transform, "Text", "WAVE 1 / 15", font, 11, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

            // Weather Badge (Blue)
            GameObject weatherBadgeObj = CreateSlicedImage(row1Obj.transform, "WeatherBadge", badgeBlueSp, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(130, 0), new Vector2(68, 22));
            CreateIcon(weatherBadgeObj.transform, "SunIcon", iconSunSp, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(14, 14));
            ctrl.weatherText = CreateText(weatherBadgeObj.transform, "Text", "맑음", font, 10, FontStyle.Bold, new Color(0.73f, 0.90f, 0.99f), new Vector2(0, 0), new Vector2(1, 1), new Vector2(8, 0), Vector2.zero, TextAnchor.MiddleCenter);

            // Speed Toggle Button
            ctrl.speedToggleBtn = CreateSlicedButton(row1Obj.transform, "SpeedBtn", btnSecondarySp, "1x", font, 11, FontStyle.Bold, Color.white, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-140, 0), new Vector2(34, 22));
            ctrl.speedToggleBtnText = ctrl.speedToggleBtn.GetComponentInChildren<Text>();

            // Wave Start Button (Primary Orange)
            ctrl.waveStartBtn = CreateSlicedButton(row1Obj.transform, "WaveStartBtn", btnPrimarySp, "웨이브 시작 [N]", font, 11, FontStyle.Bold, Color.white, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-58, 0), new Vector2(116, 22));
            ctrl.waveStartBtnText = ctrl.waveStartBtn.GetComponentInChildren<Text>();

            // --- Row 2: Queen HP & Resources ---
            GameObject row2Obj = CreateUIObject("Row2", hudObj.transform);
            RectTransform r2 = row2Obj.GetComponent<RectTransform>();
            SetAnchors(r2, new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(8, 0), new Vector2(-16, 0));

            // Queen Crown Icon
            CreateIcon(row2Obj.transform, "QueenCrown", iconCrownSp, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(16, 16));

            // Queen HP Bar
            GameObject hpBarObj = CreateSlicedImage(row2Obj.transform, "QueenHpBar", hpBgSp, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(82, 0), new Vector2(120, 12));

            GameObject hpFillObj = CreateSlicedImage(hpBarObj.transform, "Fill", hpFillSp, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ctrl.queenHpFill = hpFillObj.GetComponent<Image>();
            ctrl.queenHpFill.type = Image.Type.Filled;
            ctrl.queenHpFill.fillMethod = Image.FillMethod.Horizontal;

            ctrl.queenHpText = CreateText(hpBarObj.transform, "HpText", "20 / 20", font, 9, FontStyle.Bold, new Color(1f, 0.85f, 0.85f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

            // Resources (Sugar & Protein)
            CreateIcon(row2Obj.transform, "SugarIcon", iconSugarSp, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-130, 0), new Vector2(16, 16));
            ctrl.sugarText = CreateText(row2Obj.transform, "SugarText", "350", font, 12, FontStyle.Bold, new Color(0.96f, 0.45f, 0.71f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-95, 0), new Vector2(50, 18), TextAnchor.MiddleLeft);

            CreateIcon(row2Obj.transform, "ProteinIcon", iconProteinSp, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-55, 0), new Vector2(16, 16));
            ctrl.proteinText = CreateText(row2Obj.transform, "ProteinText", "0", font, 12, FontStyle.Bold, new Color(0.98f, 0.57f, 0.24f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-22, 0), new Vector2(45, 18), TextAnchor.MiddleLeft);

            // -------------------------------------------------------------------------
            // 3. BOTTOM DOCK (88px Height, 6 Summon Cards)
            // -------------------------------------------------------------------------
            GameObject dockObj = CreateUIObject("BottomDock", shellObj.transform);
            RectTransform dockRect = dockObj.GetComponent<RectTransform>();
            SetAnchors(dockRect, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(0, 88));
            Image dockImg = dockObj.AddComponent<Image>();
            dockImg.sprite = dockBgSp;
            dockImg.type = Image.Type.Sliced;

            float cardW = 84f;
            float cardH = 80f;
            float startX = 8f;
            float gap = 4f;

            for (int i = 0; i < antList.Count && i < 6; i++)
            {
                var ant = antList[i];
                float posX = startX + (i * (cardW + gap)) + (cardW * 0.5f);

                GameObject card = CreateUIObject($"Card_{ant.towerId}", dockObj.transform);
                RectTransform cr = card.GetComponent<RectTransform>();
                SetAnchors(cr, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(posX, 0), new Vector2(cardW, cardH));

                Image cardBg = card.AddComponent<Image>();
                cardBg.sprite = cardNormalSp;
                cardBg.type = Image.Type.Sliced;

                // Shortcut Number Badge
                GameObject badgeObj = CreateUIObject("Badge", card.transform);
                RectTransform br = badgeObj.GetComponent<RectTransform>();
                SetAnchors(br, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-4, -4), new Vector2(14, 14));
                CreateText(badgeObj.transform, "Num", (i + 1).ToString(), font, 9, FontStyle.Bold, new Color(0.7f, 0.75f, 0.8f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

                // Ant Icon (42x42 pixel art portrait)
                GameObject iconObj = CreateUIObject("Icon", card.transform);
                RectTransform ir = iconObj.GetComponent<RectTransform>();
                SetAnchors(ir, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(42, 42));
                Image iconImg = iconObj.AddComponent<Image>();
                if (ant.icon != null) iconImg.sprite = ant.icon;

                // Name
                Text nameTxt = CreateText(card.transform, "Name", ant.antName, font, 10, FontStyle.Bold, Color.white, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, -15), new Vector2(0, 14), TextAnchor.MiddleCenter);

                // Cost Row (Sugar Icon + Cost Number)
                GameObject costRow = CreateUIObject("CostRow", card.transform);
                RectTransform costRect = costRow.GetComponent<RectTransform>();
                SetAnchors(costRect, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -28), new Vector2(0, 14));

                CreateIcon(costRow.transform, "Sugar", iconSugarSp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-12, 0), new Vector2(11, 11));
                Text costTxt = CreateText(costRow.transform, "Cost", ant.baseCost.ToString(), font, 10, FontStyle.Bold, new Color(0.96f, 0.45f, 0.71f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(8, 0), new Vector2(30, 14), TextAnchor.MiddleLeft);

                Button btn = card.AddComponent<Button>();

                AntCardUI cardUI = card.AddComponent<AntCardUI>();
                cardUI.Setup(ant, cardNormalSp, cardActiveSp);
                cardUI.cardBg = cardBg;
                cardUI.iconImg = iconImg;
                cardUI.nameText = nameTxt;
                cardUI.costText = costTxt;
                cardUI.button = btn;
            }

            // -------------------------------------------------------------------------
            // 4. FLOATING SKILL DOCK (Right Side)
            // -------------------------------------------------------------------------
            GameObject skillDockObj = CreateUIObject("QueenSkillDock", shellObj.transform);
            RectTransform sdRect = skillDockObj.GetComponent<RectTransform>();
            SetAnchors(sdRect, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-10, 98), new Vector2(50, 170));

            ctrl.swarmCallBtn = CreateCircularSkillButton(skillDockObj.transform, "Skill_Q", skillCircleSp, iconSwarmSp, "Q", new Vector2(0, 114), font);
            ctrl.acidRainBtn = CreateCircularSkillButton(skillDockObj.transform, "Skill_W", skillCircleSp, iconAcidSp, "W", new Vector2(0, 58), font);
            ctrl.honeyTrapBtn = CreateCircularSkillButton(skillDockObj.transform, "Skill_E", skillCircleSp, iconHoneySp, "E", new Vector2(0, 2), font);

            QueenSkillManager qsm = canvasObj.AddComponent<QueenSkillManager>();
            qsm.swarmCallBtn = ctrl.swarmCallBtn;
            qsm.acidRainBtn = ctrl.acidRainBtn;
            qsm.honeyTrapBtn = ctrl.honeyTrapBtn;

            // -------------------------------------------------------------------------
            // 5. BOTTOM SHEET INSPECTOR (Modal Popup on Tower Click)
            // -------------------------------------------------------------------------
            GameObject sheetObj = CreateUIObject("BottomSheet", shellObj.transform);
            RectTransform sr = sheetObj.GetComponent<RectTransform>();
            SetAnchors(sr, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 88), new Vector2(0, 160));
            Image sheetBg = sheetObj.AddComponent<Image>();
            sheetBg.sprite = sheetPanelSp;
            sheetBg.type = Image.Type.Sliced;
            ctrl.bottomSheetPanel = sheetObj;

            // Sheet Title
            ctrl.sheetTitleText = CreateText(sheetObj.transform, "Title", "불개미 Lv.1", font, 13, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -16), new Vector2(-80, 20), TextAnchor.MiddleLeft);

            // Close Button
            ctrl.sheetCloseBtn = CreateSlicedButton(sheetObj.transform, "CloseBtn", btnSecondarySp, "✕", font, 12, FontStyle.Bold, Color.white, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-22, -16), new Vector2(26, 22));

            // Stats row
            ctrl.sheetStatsText = CreateText(sheetObj.transform, "Stats", "공격력: 28  |  공속: 2.5  |  사거리: 2.2", font, 11, FontStyle.Normal, new Color(0.85f, 0.9f, 0.95f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -40), new Vector2(-24, 18), TextAnchor.MiddleLeft);

            // Action Buttons (Priority, Upgrade, Sell Row)
            ctrl.sheetPriorityBtn = CreateSlicedButton(sheetObj.transform, "PriorityBtn", btnSecondarySp, "🎯 선두 우선", font, 10, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-155, 36), new Vector2(100, 32));
            ctrl.sheetPriorityBtnText = ctrl.sheetPriorityBtn.GetComponentInChildren<Text>();

            ctrl.sheetUpgradeBtn = CreateSlicedButton(sheetObj.transform, "UpgradeBtn", btnPrimarySp, "2단계 강화 (🍬120)", font, 11, FontStyle.Bold, Color.white, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-10, 36), new Vector2(165, 32));
            ctrl.sheetUpgradeBtnText = ctrl.sheetUpgradeBtn.GetComponentInChildren<Text>();

            ctrl.sheetOverclockBtn = CreateSlicedButton(sheetObj.transform, "OverclockBtn", btnPrimarySp, "한계돌파 +1 (🍬150/🥩1)", font, 11, FontStyle.Bold, Color.white, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-10, 36), new Vector2(165, 32));
            ctrl.sheetOverclockBtnText = ctrl.sheetOverclockBtn.GetComponentInChildren<Text>();

            ctrl.sheetSellBtn = CreateSlicedButton(sheetObj.transform, "SellBtn", btnSecondarySp, "철거 (🍬70)", font, 10, FontStyle.Bold, new Color(0.95f, 0.4f, 0.4f), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(145, 36), new Vector2(95, 32));
            ctrl.sheetSellBtnText = ctrl.sheetSellBtn.GetComponentInChildren<Text>();

            sheetObj.SetActive(false);

            // -------------------------------------------------------------------------
            // 6. EVOLUTION PICKER MODAL (Full 1:1 Match with HTML #evolution-modal)
            // -------------------------------------------------------------------------
            GameObject modalObj = CreateUIObject("EvolutionModal", shellObj.transform);
            RectTransform mr = modalObj.GetComponent<RectTransform>();
            SetAnchors(mr, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(504, 310));
            Image modalBg = modalObj.AddComponent<Image>();
            modalBg.sprite = sheetPanelSp;
            modalBg.type = Image.Type.Sliced;
            ctrl.evolutionModalPanel = modalObj;

            // Header Title
            ctrl.evTitleText = CreateText(modalObj.transform, "EvTitle", "🧬 3레벨 특화 진화 분기 선택", font, 14, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -18), new Vector2(-60, 22), TextAnchor.MiddleLeft);

            // Close Button
            ctrl.evCloseBtn = CreateSlicedButton(modalObj.transform, "CloseBtn", btnSecondarySp, "✕", font, 12, FontStyle.Bold, Color.white, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-22, -18), new Vector2(26, 22));

            // Subtitle
            ctrl.evSubtitleText = CreateText(modalObj.transform, "EvSubtitle", "단백질(🍖 2)을 소모하여 개미 군단의 궁극의 진화체를 선택하세요.", font, 11, FontStyle.Normal, new Color(0.7f, 0.75f, 0.8f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -40), new Vector2(-32, 18), TextAnchor.MiddleLeft);

            // --- Card A (Left) ---
            GameObject cardAObj = CreateUIObject("CardA", modalObj.transform);
            RectTransform cAr = cardAObj.GetComponent<RectTransform>();
            SetAnchors(cAr, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-122, -22), new Vector2(234, 215));
            Image cardABg = cardAObj.AddComponent<Image>();
            cardABg.sprite = cardNormalSp;
            cardABg.type = Image.Type.Sliced;

            // Card A Icon
            GameObject iconAObj = CreateUIObject("Icon", cardAObj.transform);
            RectTransform iconAr = iconAObj.GetComponent<RectTransform>();
            SetAnchors(iconAr, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -12), new Vector2(40, 40));
            ctrl.evCardAIcon = iconAObj.AddComponent<Image>();

            // Card A Name & Title
            ctrl.evCardAName = CreateText(cardAObj.transform, "Name", "[A] 군대개미", font, 13, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(58, -14), new Vector2(-12, 18), TextAnchor.MiddleLeft);
            ctrl.evCardATitle = CreateText(cardAObj.transform, "Title", "살육의 도살자", font, 11, FontStyle.Bold, new Color(0.96f, 0.62f, 0.1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(58, -32), new Vector2(-12, 16), TextAnchor.MiddleLeft);

            // Card A Description
            ctrl.evCardADesc = CreateText(cardAObj.transform, "Desc", "적에게 출혈 디버프를 부여하며, 적 처치 시 3초간 공격 속도가 2배로 폭증합니다.", font, 10, FontStyle.Normal, new Color(0.85f, 0.9f, 0.95f), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(12, 10), new Vector2(-24, 60), TextAnchor.UpperLeft);

            // Card A Cost
            ctrl.evCardACost = CreateText(cardAObj.transform, "Cost", "비용: 🍬 180 + 🍖 2", font, 11, FontStyle.Bold, new Color(0.96f, 0.45f, 0.71f), new Vector2(0, 0), new Vector2(1, 0), new Vector2(12, 48), new Vector2(-24, 16), TextAnchor.MiddleCenter);

            // Card A Button
            ctrl.evCardABtn = CreateSlicedButton(cardAObj.transform, "SelectBtn", btnPrimarySp, "[A] 진화 선택", font, 11, FontStyle.Bold, Color.white, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(210, 28));

            // --- Card B (Right) ---
            GameObject cardBObj = CreateUIObject("CardB", modalObj.transform);
            RectTransform cBr = cardBObj.GetComponent<RectTransform>();
            SetAnchors(cBr, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(122, -22), new Vector2(234, 215));
            Image cardBBg = cardBObj.AddComponent<Image>();
            cardBBg.sprite = cardNormalSp;
            cardBBg.type = Image.Type.Sliced;

            // Card B Icon
            GameObject iconBObj = CreateUIObject("Icon", cardBObj.transform);
            RectTransform iconBr = iconBObj.GetComponent<RectTransform>();
            SetAnchors(iconBr, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -12), new Vector2(40, 40));
            ctrl.evCardBIcon = iconBObj.AddComponent<Image>();

            // Card B Name & Title
            ctrl.evCardBName = CreateText(cardBObj.transform, "Name", "[B] 철갑개미", font, 13, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(58, -14), new Vector2(-12, 18), TextAnchor.MiddleLeft);
            ctrl.evCardBTitle = CreateText(cardBObj.transform, "Title", "불굴의 요새", font, 11, FontStyle.Bold, new Color(0.96f, 0.62f, 0.1f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(58, -32), new Vector2(-12, 16), TextAnchor.MiddleLeft);

            // Card B Description
            ctrl.evCardBDesc = CreateText(cardBObj.transform, "Desc", "두꺼운 철갑으로 받는 피해를 45% 경감하며 최대 3기의 적을 저지합니다.", font, 10, FontStyle.Normal, new Color(0.85f, 0.9f, 0.95f), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(12, 10), new Vector2(-24, 60), TextAnchor.UpperLeft);

            // Card B Cost
            ctrl.evCardBCost = CreateText(cardBObj.transform, "Cost", "비용: 🍬 180 + 🍖 2", font, 11, FontStyle.Bold, new Color(0.96f, 0.45f, 0.71f), new Vector2(0, 0), new Vector2(1, 0), new Vector2(12, 48), new Vector2(-24, 16), TextAnchor.MiddleCenter);

            // Card B Button
            ctrl.evCardBBtn = CreateSlicedButton(cardBObj.transform, "SelectBtn", btnPrimarySp, "[B] 진화 선택", font, 11, FontStyle.Bold, Color.white, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(210, 28));

            modalObj.SetActive(false);

            // -------------------------------------------------------------------------
            // 7. GAME RESULT OVERLAYS (Victory & Defeat)
            // -------------------------------------------------------------------------
            GameResultUI resultUI = canvasObj.AddComponent<GameResultUI>();

            // Victory Panel
            GameObject vicObj = CreateUIObject("VictoryScreen", shellObj.transform);
            RectTransform vr = vicObj.GetComponent<RectTransform>();
            SetAnchors(vr, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image vicBg = vicObj.AddComponent<Image>();
            vicBg.color = new Color(0.04f, 0.14f, 0.06f, 0.95f);
            resultUI.victoryPanel = vicObj;

            resultUI.victoryTitleText = CreateText(vicObj.transform, "Title", "🎉 작전 성공 (VICTORY)", font, 18, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(400, 36), TextAnchor.MiddleCenter);
            resultUI.victoryStatsText = CreateText(vicObj.transform, "Stats", "전 15 웨이브 완전 방어 성공!", font, 12, FontStyle.Normal, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 120), TextAnchor.MiddleCenter);
            resultUI.victoryLobbyBtn = CreateSlicedButton(vicObj.transform, "LobbyBtn", btnSecondarySp, "로비로 돌아가기", font, 12, FontStyle.Bold, Color.white, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(-75, 0), new Vector2(130, 34));
            resultUI.victoryRestartBtn = CreateSlicedButton(vicObj.transform, "RestartBtn", btnPrimarySp, "다시 출진", font, 12, FontStyle.Bold, Color.white, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(75, 0), new Vector2(130, 34));
            vicObj.SetActive(false);

            // Defeat Panel
            GameObject defObj = CreateUIObject("DefeatScreen", shellObj.transform);
            RectTransform dr = defObj.GetComponent<RectTransform>();
            SetAnchors(dr, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image defBg = defObj.AddComponent<Image>();
            defBg.color = new Color(0.14f, 0.04f, 0.04f, 0.95f);
            resultUI.defeatPanel = defObj;

            resultUI.defeatTitleText = CreateText(defObj.transform, "Title", "💀 방어선 함락 (DEFEAT)", font, 18, FontStyle.Bold, new Color(0.95f, 0.25f, 0.25f), new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(400, 36), TextAnchor.MiddleCenter);
            resultUI.defeatStatsText = CreateText(defObj.transform, "Stats", "여왕개미가 쓰러졌습니다.", font, 12, FontStyle.Normal, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 120), TextAnchor.MiddleCenter);
            resultUI.defeatLobbyBtn = CreateSlicedButton(defObj.transform, "LobbyBtn", btnSecondarySp, "로비로 돌아가기", font, 12, FontStyle.Bold, Color.white, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(-75, 0), new Vector2(130, 34));
            resultUI.defeatRestartBtn = CreateSlicedButton(defObj.transform, "RestartBtn", btnPrimarySp, "재도전", font, 12, FontStyle.Bold, Color.white, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), new Vector2(75, 0), new Vector2(130, 34));
            defObj.SetActive(false);

            return ctrl;
        }

        private static Sprite LoadUISprite(string name)
        {
            string path = $"Assets/Sprites/UI/{name}.png";
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static GameObject CreateSlicedImage(Transform parent, string name, Sprite sp, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            GameObject obj = CreateUIObject(name, parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            SetAnchors(rt, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), pos, size);
            Image img = obj.AddComponent<Image>();
            img.sprite = sp;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            return obj;
        }

        private static GameObject CreateIcon(Transform parent, string name, Sprite sp, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            GameObject obj = CreateUIObject(name, parent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            SetAnchors(rt, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), pos, size);
            Image img = obj.AddComponent<Image>();
            img.sprite = sp;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return obj;
        }

        private static Text CreateText(Transform parent, string name, string content, Font font, int fontSize, FontStyle style, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, TextAnchor align)
        {
            GameObject textObj = CreateUIObject(name, parent);
            RectTransform tr = textObj.GetComponent<RectTransform>();
            SetAnchors(tr, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), pos, size);

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
            GameObject btnObj = CreateUIObject(name, parent);
            RectTransform br = btnObj.GetComponent<RectTransform>();
            SetAnchors(br, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), pos, size);

            Image img = btnObj.AddComponent<Image>();
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;

            Button btn = btnObj.AddComponent<Button>();

            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = cb;

            CreateText(btnObj.transform, "Text", label, font, fontSize, style, textColor, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
            return btn;
        }

        private static Button CreateCircularSkillButton(Transform parent, string name, Sprite circleSprite, Sprite iconSprite, string keyText, Vector2 pos, Font font)
        {
            GameObject btnObj = CreateUIObject(name, parent);
            RectTransform br = btnObj.GetComponent<RectTransform>();
            SetAnchors(br, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(46, 46));

            Image img = btnObj.AddComponent<Image>();
            img.sprite = circleSprite;

            Button btn = btnObj.AddComponent<Button>();

            // Icon
            CreateIcon(btnObj.transform, "Icon", iconSprite, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(26, 26));

            // Key Badge
            GameObject badgeObj = CreateUIObject("KeyBadge", btnObj.transform);
            RectTransform brt = badgeObj.GetComponent<RectTransform>();
            SetAnchors(brt, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-2, 2), new Vector2(12, 12));
            CreateText(badgeObj.transform, "Text", keyText, font, 9, FontStyle.Bold, new Color(0.98f, 0.75f, 0.14f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

            return btn;
        }
    }
}
