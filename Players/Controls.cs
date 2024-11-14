using LethalMystery.GameMech;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalMystery.Players
{
    internal class Controls
    {
        public static InputAction? SpawnWeaponAction;
        public static InputActionMap monsterControls = new InputActionMap("MonsterControls");
        private static bool spawningWeapon = false;

        public static void InitControls()
        {
            InputAction shapeshift = monsterControls.AddAction("Shapeshift", InputActionType.Button, binding: "<Keyboard>/5");
            shapeshift.performed += Shapeshift_performed;

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
