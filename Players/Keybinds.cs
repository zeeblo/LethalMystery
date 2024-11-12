using LethalMystery.GameMech;
using UnityEngine.InputSystem;

namespace LethalMystery.Players
{
    internal class Keybinds
    {
        public static InputAction? SpawnWeaponAction;

        public static void InitControls ()
        {
            SpawnWeaponAction = new InputAction("SpawnWeapon", InputActionType.Button, "<Keyboard>/5");
            SpawnWeaponAction.performed += SpawnWeapon_performed;
        }


        private static void SpawnWeapon_performed(InputAction.CallbackContext context)
        {
            Plugin.mls.LogInfo(">>> Spawned weapon!!!!! ! !!!");
        }

    }
}
