using System;
using System.Collections.Generic;
using UnityEngine;
using AntTD.Data;
using AntTD.Entities;
using UnityEngine.InputSystem;

namespace AntTD.Managers
{
    public class PlacementManager : MonoBehaviour
    {
        public static PlacementManager Instance { get; private set; }

        public TowerData selectedAntData { get; private set; }
        public Tower inspectedTower { get; private set; }

        public GameObject defaultTowerPrefab;
        public List<TowerData> allAntTypes = new List<TowerData>();

        public event Action<TowerData> OnAntSelectionChanged;
        public event Action<Tower> OnTowerInspected;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (selectedAntData == null && allAntTypes != null && allAntTypes.Count > 0)
            {
                TowerData defaultAnt = allAntTypes.Find(a => a.towerId == "fire") ?? allAntTypes[0];
                SelectAntToBuild(defaultAnt);
            }
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            if (Keyboard.current == null) return;

            Key[] numberKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6 };
            for (int i = 0; i < numberKeys.Length && i < allAntTypes.Count; i++)
            {
                if (Keyboard.current[numberKeys[i]].wasPressedThisFrame)
                {
                    SelectAntToBuild(allAntTypes[i]);
                }
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.nKey.wasPressedThisFrame)
            {
                if (WaveManager.Instance != null && !WaveManager.Instance.isWaveRunning)
                {
                    WaveManager.Instance.StartNextWave();
                }
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseInspector();
            }
        }

        public void SelectAntToBuild(TowerData antData)
        {
            inspectedTower = null;
            if (selectedAntData == antData)
            {
                selectedAntData = null;
            }
            else
            {
                selectedAntData = antData;
            }
            OnAntSelectionChanged?.Invoke(selectedAntData);
        }

        public void CloseInspector()
        {
            if (inspectedTower == null) return;
            inspectedTower = null;
            OnTowerInspected?.Invoke(null);
        }

        public void CloseInspectorSilently()
        {
            inspectedTower = null;
        }

        public void OnSlotClicked(TowerSlot slot)
        {
            if (slot.isOccupied)
            {
                inspectedTower = slot.currentTower;
                selectedAntData = null;
                OnAntSelectionChanged?.Invoke(null);
                OnTowerInspected?.Invoke(inspectedTower);
                return;
            }

            if (selectedAntData == null) return;

            if (GameManager.Instance.SpendSugar(selectedAntData.baseCost))
            {
                slot.BuildTower(defaultTowerPrefab, selectedAntData);
                if (GameManager.Instance.sugar < selectedAntData.baseCost)
                {
                    selectedAntData = null;
                    OnAntSelectionChanged?.Invoke(null);
                }
            }
            else
            {
                Debug.LogWarning("Not enough sugar to build " + selectedAntData.antName);
            }
        }
    }
}
