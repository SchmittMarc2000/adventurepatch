using BrilliantSkies.Core.Logger;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.PlayerProfiles._Modules._KeyMapping;
using UnityEngine;

namespace AdventurePatch
{
    public static class CustomKeycodes
    {
        public const KeyCode Return = (KeyCode)13;
        public const KeyCode F8 = (KeyCode)289;
    }

    public static class CustomBindingActions
    {
        public static void StartWaveFromKey()
        {
            AdvLogger.LogInfo("AdventurePatch Start Wave key pressed!");
            SpawnWaveMode.StartWaveMode();
        }

        public static void StopWaveFromKey()
        {
            AdvLogger.LogInfo("AdventurePatch Stop Wave key pressed!");
            SpawnWaveMode.StopWaveMode();
        }
    }
    public static class CustomBindingManager
    {
        private static bool _isInitialized = false;
        private static bool _startWasPressed = false;
        private static bool _stopWasPressed = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            // Register injectors for default keys
            InputWrapper.AddInjector(new KeyInjectorProvideAHitOnThisKey(
                CustomKeycodes.Return, KeyInputEventType.Down));
            InputWrapper.AddInjector(new KeyInjectorProvideAHitOnThisKey(
                CustomKeycodes.Return, KeyInputEventType.Held));
            InputWrapper.AddInjector(new KeyInjectorProvideAHitOnThisKey(
                CustomKeycodes.Return, KeyInputEventType.Up));

            InputWrapper.AddInjector(new KeyInjectorProvideAHitOnThisKey(
                CustomKeycodes.F8, KeyInputEventType.Down));
            InputWrapper.AddInjector(new KeyInjectorProvideAHitOnThisKey(
                CustomKeycodes.F8, KeyInputEventType.Held));
            InputWrapper.AddInjector(new KeyInjectorProvideAHitOnThisKey(
                CustomKeycodes.F8, KeyInputEventType.Up));

            _isInitialized = true;
            AdvLogger.LogInfo("AdventurePatch Custom keybindings initialized");
        }

        public static void Update()
        {
            if (!_isInitialized) return;

            var config = ProfileManager.Instance.GetModule<AP_MConfig>();

            // Check Start Wave key
            bool startPressed = InputWrapper.GetKeyDown(config.StartWaveKey) || InputWrapper.GetKeyUp(config.StartWaveKey) || InputWrapper.GetKey(config.StartWaveKey);
            if (startPressed && !_startWasPressed)
            {
                CustomBindingActions.StartWaveFromKey();
            }
            _startWasPressed = startPressed;

            // Check Stop Wave key
            bool stopPressed = InputWrapper.GetKeyDown(config.StopWaveKey) || InputWrapper.GetKeyUp(config.StopWaveKey) || InputWrapper.GetKey(config.StopWaveKey);
            if (stopPressed && !_stopWasPressed)
            {
                CustomBindingActions.StopWaveFromKey();
            }
            _stopWasPressed = stopPressed;
        }
    }
}