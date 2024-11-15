using LethalMystery.GameMech;
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

        public static void InitControls()
        {
            if (Plugin.shapeshiftBind == null)
            {
                return;
            }

            InputAction shapeshift = monsterControls.AddAction("shapeshift", InputActionType.Button, binding: "<Keyboard>/" + Plugin.shapeshiftBind.Value);
            shapeshift.performed += Shapeshift_performed;
            monsterControls.Enable();

            shapeshiftRef = InputActionReference.Create(shapeshift);
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
                AutoGiveWeapon.ResetVariables();
                Commands.SpawnWeapons(Roles.CurrentRole.GetWeapon());

                yield return new WaitForSeconds(5f); // butler takes roughly 2.5s or less while nutcracker takes a bit longer
                AutoGiveWeapon.doneSpawningWeapons = true;

                yield return new WaitForSeconds(8);
                AutoGiveWeapon.doneSpawningWeapons = false;
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
