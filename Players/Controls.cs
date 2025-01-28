using LethalMystery.MainGame;
using LethalMystery.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalMystery.Players
{
    internal class Controls
    {
        private static InputActionAsset actionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        public static InputActionMap monsterControls = actionAsset.AddActionMap("MonsterControls");
        public static InputActionReference? shapeshiftRef;
        private static bool spawningWeapon = false;
        public static bool cleaningBody = false;

        public static void InitControls()
        {
            if (LMConfig.shapeshiftBind == null)
            {
                return;
            }

            
            InputAction selfclean = monsterControls.AddAction("self clean", InputActionType.Value, binding: "<Keyboard>/" + LMConfig.selfcleanBind.Value);
            InputAction shapeshift = monsterControls.AddAction("shapeshift", InputActionType.Button, binding: "<Keyboard>/" + LMConfig.shapeshiftBind.Value);


            selfclean.performed += Selfclean_performed;
            selfclean.canceled += Selfclean_canceled;
            shapeshift.performed += Shapeshift_performed;


            shapeshiftRef = InputActionReference.Create(shapeshift); // just for one specific KeybindsUI section, the other kebinds dont need to do this
        }


        public static void ResetVars()
        {
            cleaningBody = false;
        }

        public static void UnlockCursor(bool value)
        {
            Cursor.lockState = (value) ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.SetCursor(HUDManager.Instance.defaultCursorTex, new Vector2(0f, 0f), CursorMode.Auto);
            if (!StartOfRound.Instance.localPlayerUsingController)
            {
                Cursor.visible = value;
            }
        }



        private static void Selfclean_performed(InputAction.CallbackContext context)
        {
            Plugin.mls.LogInfo(">> Cleaning Blood");
            cleaningBody = true;
            HUDManager.Instance.holdInteractionCanvasGroup.alpha = 1;
        }

        private static void Selfclean_canceled(InputAction.CallbackContext context)
        {
            Plugin.mls.LogInfo(">> Stopped cleaning");
            StopCleaning();
        }


        public static void StopCleaning()
        {
            cleaningBody = false;
            Ability.cleaningBloodAmt = 0f;
            HUDManager.Instance.holdFillAmount = 0f;
            HUDManager.Instance.holdInteractionCanvasGroup.alpha = 0;
        }


        private static void Shapeshift_performed(InputAction.CallbackContext context)
        {
            Plugin.mls.LogInfo(">> Shapeshifted!!");
        }


        private static void SpawnWeapon_performed(InputAction.CallbackContext context)
        {
            StartOfRound.Instance.StartCoroutine(SpawnWeapon());
        }

        private static IEnumerator SpawnWeapon()
        {
            if (spawningWeapon == false && Roles.CurrentRole != null)
            {
                bool weaponInInventory = CheckForExistingWeapon();

                if (weaponInInventory)
                    yield break;

                spawningWeapon = true;
                AutoGiveItem.ResetVariables();
                Commands.SpawnWeapons(Roles.CurrentRole.GetItem());

                yield return new WaitForSeconds(5f); // butler takes roughly 2.5s or less while nutcracker takes a bit longer
                AutoGiveItem.doneSpawningWeapons = true;

                yield return new WaitForSeconds(8);
                AutoGiveItem.doneSpawningWeapons = false;
                spawningWeapon = false;
            }
        }

        private static bool CheckForExistingWeapon()
        {
            for (int i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
            {
                if (GameNetworkManager.Instance.localPlayerController.ItemSlots[i] != null)
                {
                    if ((GameNetworkManager.Instance.localPlayerController.ItemSlots[i].name.ToLower().Contains("shotgun")) || (GameNetworkManager.Instance.localPlayerController.ItemSlots[i].name.ToLower().Contains("knife")))
                    {
                        HUDManager.Instance.DisplayTip("Weapon Detected!", "You already have a weapon in your inventory.", isWarning: true);
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
