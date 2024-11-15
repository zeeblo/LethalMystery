using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using LethalMystery.Players;


namespace LethalMystery.UI
{
    [HarmonyPatch]
    internal class KeybindsUI
    {


        /// <summary>
        /// Dispaly Keybind options
        /// </summary>
        [HarmonyPatch(typeof(KepRemapPanel), nameof(KepRemapPanel.LoadKeybindsUI))]
        [HarmonyPrefix]
        private static bool LoadBindsPatch(KepRemapPanel __instance)
        {
            // Prevent keybinds from being used while in keybind menu
            Controls.monsterControls.Disable();


            __instance.currentVertical = 0;
            __instance.currentHorizontal = 0;
            int sectionOffset = 0;
            int LMysteryYOffset = 0;
            Vector2 anchoredPosition = new Vector2(__instance.horizontalOffset * (float)__instance.currentHorizontal, __instance.verticalOffset * (float)__instance.currentVertical);


            // Lethal Mystery section at the top
            GameObject LMysterySection = UnityEngine.Object.Instantiate(__instance.sectionTextPrefab, __instance.keyRemapContainer);
            LMysterySection.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, -__instance.verticalOffset * sectionOffset);
            LMysterySection.GetComponentInChildren<TextMeshProUGUI>().text = "Lethal Mystery";
            __instance.keySlots.Add(LMysterySection);

            sectionOffset += 1;
            // Add buttons for "Lethal Mystery" section
            for (int i = 0; i < Plugin.AllHotkeys.Count; i++)
            {
                GameObject LMysteryButtons = UnityEngine.Object.Instantiate(__instance.keyRemapSlotPrefab, __instance.keyRemapContainer);
                __instance.keySlots.Add(LMysteryButtons);

                // Show name of keybind and set position
                LMysteryButtons.GetComponentInChildren<TextMeshProUGUI>().text = Plugin.AllHotkeys[i].Definition.Key;
                LMysteryButtons.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, LMysteryYOffset);

                // Show name of the actual button box
                SettingsOption componentInChildren = LMysteryButtons.GetComponentInChildren<SettingsOption>();
                componentInChildren.currentlyUsedKeyText.text = Plugin.AllHotkeys[i].Value;
                
                /* placeholder variable to prevent the default PushToTalk from being changed as well
                 * Setting this to null will throw an error, so im setting it to an arbitrary
                 * InputActionReference name.
                 */
                componentInChildren.rebindableAction = Controls.shapeshiftRef; 

                // move down for next button
                LMysteryYOffset -= 40;
                sectionOffset += 1;
            }

            // Adjust position for existing key bindings
            __instance.currentVertical = 0;
            __instance.currentHorizontal = 0;
            anchoredPosition = new Vector2(__instance.horizontalOffset * (float)__instance.currentHorizontal, __instance.verticalOffset * (float)(__instance.currentVertical + sectionOffset));


            // Vanilla keybinds section for the regular game
            GameObject NormalSection = UnityEngine.Object.Instantiate(__instance.sectionTextPrefab, __instance.keyRemapContainer);
            NormalSection.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, LMysteryYOffset - 60);
            NormalSection.GetComponentInChildren<TextMeshProUGUI>().text = "Normal";

            bool flag = false;
            int num = 0;
            for (int i = 0; i < __instance.remappableKeys.Count; i++)
            {
                if (__instance.remappableKeys[i].currentInput == null)
                {
                    continue;
                }


                GameObject gameObject = UnityEngine.Object.Instantiate(__instance.keyRemapSlotPrefab, __instance.keyRemapContainer);
                __instance.keySlots.Add(gameObject);
                gameObject.GetComponentInChildren<TextMeshProUGUI>().text = __instance.remappableKeys[i].ControlName;
                gameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

                SettingsOption componentInChildren = gameObject.GetComponentInChildren<SettingsOption>();
                componentInChildren.rebindableAction = __instance.remappableKeys[i].currentInput;
                componentInChildren.rebindableActionBindingIndex = __instance.remappableKeys[i].rebindingIndex;
                componentInChildren.gamepadOnlyRebinding = __instance.remappableKeys[i].gamepadOnly;

                int rebindingIndex = __instance.remappableKeys[i].rebindingIndex;
                int num2 = Mathf.Max(rebindingIndex, 0);
                componentInChildren.currentlyUsedKeyText.text = InputControlPath.ToHumanReadableString(componentInChildren.rebindableAction.action.bindings[num2].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

                if (!flag && i + 1 < __instance.remappableKeys.Count && __instance.remappableKeys[i + 1].gamepadOnly)
                {
                    num = (int)(__instance.maxVertical + 2f);
                    __instance.currentVertical = 0;
                    __instance.currentHorizontal = 0;
                    GameObject gameObject2 = UnityEngine.Object.Instantiate(__instance.sectionTextPrefab, __instance.keyRemapContainer);
                    gameObject2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, (0f - __instance.verticalOffset) * (float)(num + sectionOffset));
                    gameObject2.GetComponentInChildren<TextMeshProUGUI>().text = "REBIND CONTROLLERS";
                    __instance.keySlots.Add(gameObject2);
                    flag = true;
                }
                else
                {
                    __instance.currentVertical++;
                    if ((float)__instance.currentVertical > __instance.maxVertical)
                    {
                        __instance.currentVertical = 0;
                        __instance.currentHorizontal++;
                    }
                }
                anchoredPosition = new Vector2(__instance.horizontalOffset * (float)__instance.currentHorizontal, (0f - __instance.verticalOffset) * (float)(__instance.currentVertical + num + sectionOffset));
            }
            return false;
        }


        /// <summary>
        /// Set the new keybind and update it in the .cfg
        /// </summary>
        [HarmonyPatch(typeof(IngamePlayerSettings), nameof(IngamePlayerSettings.CompleteRebind))]
        [HarmonyPostfix]
        private static void CompleteRebindPatch(SettingsOption optionUI)
        {
            for (int i = 0; i < Plugin.AllHotkeys.Count; i++)
            {
                if (Plugin.AllHotkeys[i].Definition.Key.ToLower() == optionUI.textElement.text.ToLower())
                {
                    Plugin.AllHotkeys[i].Value = optionUI.currentlyUsedKeyText.text; // Set new keybind button
                    Plugin.AllHotkeys[i].ConfigFile.Save();
                    Controls.monsterControls.FindAction(Plugin.AllHotkeys[i].Definition.Key.ToLower()).ApplyBindingOverride($"<Keyboard>/{optionUI.currentlyUsedKeyText.text.ToLower()}");
                    
                    
                    Plugin.mls.LogInfo($">>> New Keybind button: {optionUI.currentlyUsedKeyText.text}");
                    Plugin.mls.LogInfo($">>> Name for keybind: {optionUI.textElement.text}");
                    Plugin.mls.LogInfo($"playerControls: {Controls.monsterControls.FindAction(Plugin.AllHotkeys[i].Definition.Key.ToLower()).GetBindingDisplayString()}");
                    break;
                }
            }

        }


        /// <summary>
        /// If the user closes the Keybind Menu, re-enable the custom keybinds.
        /// (If they're in a game)
        /// </summary>
        [HarmonyPatch(typeof(KepRemapPanel), nameof(KepRemapPanel.OnDisable))]
        [HarmonyPostfix]
        private static void KepOnDisablePatch()
        {
            if (Roles.CurrentRole != null && Controls.monsterControls.enabled == false)
            {
                if (Roles.CurrentRole.Type == "monster")
                {
                    Controls.monsterControls.Enable();
                }
            }

        }


    }
}
