using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using LethalMystery.Players;
using LethalMystery.Utils;
using UnityEngine.UI;



namespace LethalMystery.UI
{
    [HarmonyPatch]
    internal class KeybindsUI
    {

        private static List<string> ControlNames = new List<string>();
        private static int LMysteryYOffset = 0;

        /// <summary>
        /// Dispaly Keybind options
        /// </summary>
        [HarmonyPatch(typeof(KepRemapPanel), nameof(KepRemapPanel.LoadKeybindsUI))]
        [HarmonyPrefix]
        private static bool LoadBindsPatch(KepRemapPanel __instance)
        {
            // Prevent keybinds from being used while in keybind menu
            Controls.monsterControls.Disable();
            GameObject raw1 = GameObject.Find("Canvas/MenuContainer/SettingsPanel/KeybindsPanel/Scroll View");
            GameObject raw2 = GameObject.Find("Systems/UI/Canvas/QuickMenu/SettingsPanel/KeybindsPanel/Scroll View");
            GameObject ScrollView = (raw1) ? raw1 : raw2;

            ScrollRect scrollMax = ScrollView.GetComponent<ScrollRect>();
            

            __instance.currentVertical = 0;
            __instance.currentHorizontal = 0;
            int sectionOffset = 0;
            LMysteryYOffset = 0;
            Vector2 anchoredPosition = new Vector2(__instance.horizontalOffset * (float)__instance.currentHorizontal, __instance.verticalOffset * (float)__instance.currentVertical);
            scrollMax.content.anchorMax = new Vector2(scrollMax.content.anchorMax.x, 1); // default scroll max

            // Lethal Mystery section at the top
            GameObject LMysterySection = UnityEngine.Object.Instantiate(__instance.sectionTextPrefab, __instance.keyRemapContainer);
            __instance.keySlots.Add(LMysterySection);
            LMysterySection.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, -__instance.verticalOffset * sectionOffset);
            LMysterySection.GetComponentInChildren<TextMeshProUGUI>().text = "Lethal Mystery";


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
                componentInChildren.currentlyUsedKeyText.text = StringAddons.ConvertToPrefix(Plugin.AllHotkeys[i].Value);

                /* placeholder variable to prevent the default PushToTalk from being changed as well.
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
            __instance.keySlots.Add(NormalSection);
            NormalSection.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, LMysteryYOffset - 60);
            NormalSection.GetComponentInChildren<TextMeshProUGUI>().text = "Normal";

            // Increases the range that the scrollwheel can go
            scrollMax.content.anchorMax = new Vector2(scrollMax.content.anchorMax.x, (scrollMax.content.anchorMax.y) + (float)Math.Abs(LMysteryYOffset) / 100f);

            return true;
        }





        [HarmonyPatch(typeof(KepRemapPanel), nameof(KepRemapPanel.LoadKeybindsUI))]
        [HarmonyPostfix]
        private static void PositionBindsPatch(KepRemapPanel __instance)
        {
            GameObject raw1 = GameObject.Find("Canvas/MenuContainer/SettingsPanel/KeybindsPanel/Scroll View/Viewport/Content/RemapKeysContainer");
            GameObject raw2 = GameObject.Find("Systems/UI/Canvas/QuickMenu/SettingsPanel/KeybindsPanel/Scroll View/Viewport/Content/RemapKeysContainer");
            GameObject raw_binds = (raw1) ? raw1 : raw2;

            List<GameObject> binds = GOTools.GetAllChildren(raw_binds);

            ControlNames.Clear();
            for (int i = 0; i < __instance.remappableKeys.Count; i++)
            {
                ControlNames.Add(__instance.remappableKeys[i].ControlName.ToLower());
            }


            foreach (GameObject obj in binds)
            {

                // Check for the default remappable keys and move them down
                List<GameObject> objChildren = GOTools.GetAllChildren(obj);
                foreach (GameObject objChild in objChildren)
                {
                    if (objChild.gameObject.GetComponent<TextMeshProUGUI>() != null)
                    {
                        string name = Array.Find(ControlNames.ToArray(), elem => elem.Equals(objChild.gameObject.GetComponent<TextMeshProUGUI>().text.ToLower()));

                        if (string.IsNullOrEmpty(name) != true)
                        {
                            RectTransform bindPosition = obj.GetComponent<RectTransform>();
                            bindPosition.anchoredPosition = new Vector2(bindPosition.anchoredPosition.x, bindPosition.anchoredPosition.y + LMysteryYOffset - 60);
                        }
                    }
                }

                // Move the "rebind controllers" text down
                if (obj.gameObject.GetComponent<TextMeshProUGUI>() != null)
                {
                    string name = obj.gameObject.GetComponent<TextMeshProUGUI>().text;

                    if (name.ToLower() == "rebind controllers")
                    {
                        RectTransform bindPosition = obj.GetComponent<RectTransform>();
                        bindPosition.anchoredPosition = new Vector2(bindPosition.anchoredPosition.x, bindPosition.anchoredPosition.y + LMysteryYOffset - 60);
                    }
                }

                // move everything up
                RectTransform allKeybindPositions = obj.GetComponent<RectTransform>();
                allKeybindPositions.anchoredPosition = new Vector2(allKeybindPositions.anchoredPosition.x, allKeybindPositions.anchoredPosition.y - LMysteryYOffset);

                // reposition scroll wheel to be at the top
                GameObject raw_view1 = GameObject.Find("Canvas/MenuContainer/SettingsPanel/KeybindsPanel/Scroll View/");
                GameObject raw_view2 = GameObject.Find("Systems/UI/Canvas/QuickMenu/SettingsPanel/KeybindsPanel/Scroll View");

                GameObject ScrollView = (raw_view1) ? raw_view1 : raw_view2;
                ScrollRect ScrollViewRect = ScrollView.GetComponent<ScrollRect>();
                ScrollViewRect.normalizedPosition = new Vector2(ScrollViewRect.normalizedPosition.x, 1);

            }

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
                    if (StringAddons.InConvertableChars(prefix: optionUI.currentlyUsedKeyText.text))
                    {
                        Plugin.AllHotkeys[i].Value = StringAddons.ConvertToSymbols(optionUI.currentlyUsedKeyText.text);
                        Plugin.AllHotkeys[i].ConfigFile.Save();
                        break;
                    }
                    else
                    {
                        Plugin.AllHotkeys[i].Value = optionUI.currentlyUsedKeyText.text; // Set new keybind button
                        Plugin.AllHotkeys[i].ConfigFile.Save();
                        Controls.monsterControls.FindAction(Plugin.AllHotkeys[i].Definition.Key.ToLower()).ApplyBindingOverride($"<Keyboard>/{optionUI.currentlyUsedKeyText.text.ToLower()}");
                        break;
                    }

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
                if (Roles.CurrentRole.Type == "_monster")
                {
                    Controls.monsterControls.Enable();
                }
            }

        }


    }
}
