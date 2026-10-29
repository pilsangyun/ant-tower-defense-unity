using System;
using UnityEngine;
using UnityEngine.UI;

namespace AntTD.Managers
{
    public enum WeatherType
    {
        Sunny,  // ☀️ 화창
        Rain,   // 🌧️ 장마
        Fog     // 🌫️ 안개
    }

    public class WeatherManager : MonoBehaviour
    {
        public static WeatherManager Instance { get; private set; }

        public WeatherType currentWeather { get; private set; } = WeatherType.Sunny;

        [Header("UI Element")]
        public Text hudWeatherText;

        [Header("Settings")]
        public float weatherDuration = 45f;
        private float weatherTimer = 0f;

        public event Action<WeatherType> OnWeatherChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            SetWeather(WeatherType.Sunny);
            weatherTimer = weatherDuration;
        }

        private void Update()
        {
            weatherTimer -= Time.deltaTime;
            if (weatherTimer <= 0f)
            {
                weatherTimer = weatherDuration;
                CycleNextWeather();
            }
        }

        public void CycleNextWeather()
        {
            WeatherType next;
            switch (currentWeather)
            {
                case WeatherType.Sunny: next = WeatherType.Rain; break;
                case WeatherType.Rain: next = WeatherType.Fog; break;
                case WeatherType.Fog: default: next = WeatherType.Sunny; break;
            }
            SetWeather(next);
        }

        public void SetWeather(WeatherType type)
        {
            currentWeather = type;

            if (hudWeatherText != null)
            {
                switch (type)
                {
                    case WeatherType.Sunny:
                        hudWeatherText.text = "☀️ 맑음";
                        break;
                    case WeatherType.Rain:
                        hudWeatherText.text = "🌧️ 장마";
                        break;
                    case WeatherType.Fog:
                        hudWeatherText.text = "🌫️ 안개";
                        break;
                }
            }

            OnWeatherChanged?.Invoke(currentWeather);
            Debug.Log($"🌦️ [WeatherManager] Weather changed to: {type}");
        }

        // Modifiers for battle calculations
        public float GetRangeModifier()
        {
            // Fog reduces tower range by 15%
            return (currentWeather == WeatherType.Fog) ? 0.85f : 1.0f;
        }

        public float GetAcidDamageModifier()
        {
            // Rain amplifies acid corrosion by +25%
            return (currentWeather == WeatherType.Rain) ? 1.25f : 1.0f;
        }

        public float GetEnemySpeedModifier()
        {
            // Wet ground in rain slightly slows enemies by 10%
            return (currentWeather == WeatherType.Rain) ? 0.90f : 1.0f;
        }
    }
}
