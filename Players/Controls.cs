using LethalMystery.MainGame;
using LethalMystery.Players.Abilities;
using LethalMystery.UI;
using LethalMystery.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalMystery.Players
{
    internal class Controls
    {
        private static InputActionAsset actionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        public static InputActionMap monsterControls = actionAsset.AddActionMap("MonsterControls");
        public static InputActionMap playerControls = actionAsset.AddActionMap("PlayerControls");
        public static InputActionReference spawnItemRef;
        public static InputActionReference selfcleanRef;
        public static InputActionReference shapeshiftRef;
        public static InputActionReference showMapRef;
        public static InputActionReference showVoteRef;
        public static Dictionary<string, InputActionReference> inputRefs = new Dictionary<string, InputActionReference>();
        private static bool spawningWeapon = false;
        public static bool cleaningBody = false;

        public static void InitControls()
        {

            InputAction showVote = playerControls.AddAction("show vote", InputActionType.Button, binding: "<Keyboard>/" + LMConfig.showVoteBind.Value);
            InputAction showMap = playerControls.AddAction("show map", InputActionType.Button, binding: "<Keyboard>/" + LMConfig.showMapBind.Value);
            InputAction spawnItem = playerControls.AddAction("spawn item", InputActionType.Button, binding: "<Keyboard>/" + LMConfig.spawnItemBind.Value);
            InputAction selfclean = monsterControls.AddAction("self clean", InputActionType.Value, binding: "<Keyboard>/" + LMConfig.selfcleanBind.Value);
            InputAction shapeshift = monsterControls.AddAction("shapeshift", InputActionType.Button, binding: "<Keyboard>/" + LMConfig.shapeshiftBind.Value);

            showVote.performed += ShowVote_performed;
            showMap.performed += ShowMap_performed;
            spawnItem.performed += SpawnItem_performed;
            selfclean.performed += Selfclean_performed;
            selfclean.canceled += Selfclean_canceled;
            shapeshift.performed += Shapeshift_performed;


            showVoteRef = InputActionReference.Create(showVote);
            showMapRef = InputActionReference.Create(showMap);
            spawnItemRef = InputActionReference.Create(spawnItem);
            selfcleanRef = InputActionReference.Create(selfclean);
            shapeshiftRef = InputActionReference.Create(shapeshift);

            AddInputRefs();
        }



        private static void AddInputRefs()
        {
            inputRefs.Add(showVoteRef.name.Split("/")[1], showVoteRef);
            inputRefs.Add(showMapRef.name.Split("/")[1], showMapRef);
            inputRefs.Add(spawnItemRef.name.Split("/")[1], spawnItemRef);
            inputRefs.Add(selfcleanRef.name.Split("/")[1], selfcleanRef);
            inputRefs.Add(shapeshiftRef.name.Split("/")[1], shapeshiftRef);
        }


        public static void ResetVars()
        {
            cleaningBody = false;
            spawningWeapon = false;
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





        private static void ShowVote_performed(InputAction.CallbackContext obj)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (Plugin.localPlayer == null) return;
            if (Plugin.localPlayer.quickMenuManager.isMenuOpen || Plugin.terminal.terminalInUse || Plugin.localPlayer.isTypingChat) return;


            Plugin.mls.LogInfo(">>> Opened Vote Menu");
        }



        private static void ShowMap_performed(InputAction.CallbackContext obj)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (Plugin.localPlayer == null) return;
            if (Plugin.localPlayer.quickMenuManager.isMenuOpen || Plugin.terminal.terminalInUse || Plugin.localPlayer.isTypingChat) return;

            if (MinimapUI.minimap == null)
            {
                MinimapUI.CreateMinimap();
                UnlockCursor(true);
                return;
            }

            MinimapUI.border.SetActive(!MinimapUI.border.activeSelf);
            UnlockCursor(MinimapUI.border.activeSelf);
        }

        private static void SpawnItem_performed(InputAction.CallbackContext context)
        {
            if (StartOfRound.Instance.inShipPhase) return;
            if (Plugin.localPlayer == null) return;
            if (Plugin.localPlayer.quickMenuManager.isMenuOpen || Plugin.terminal.terminalInUse || Plugin.localPlayer.isTypingChat) return;
            if (Plugin.localPlayer.isPlayerDead) return;
            StartOfRound.Instance.StartCoroutine(SpawnWeapon());
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
            CleanSuit.cleaningBloodAmt = 0f;
            HUDManager.Instance.holdFillAmount = 0f;
            HUDManager.Instance.holdInteractionCanvasGroup.alpha = 0;
        }


        private static void Shapeshift_performed(InputAction.CallbackContext context)
        {
            Plugin.mls.LogInfo(">> Shapeshifted!!");
        }



        private static IEnumerator SpawnWeapon()
        {
            if (spawningWeapon == false && Roles.CurrentRole != null)
            {
                bool weaponInInventory = GOTools.CheckForExistingWeapon();

                if (weaponInInventory)
                {
                    HUDManager.Instance.DisplayTip("Weapon Detected!", "You already have a weapon in your inventory.", isWarning: true);
                    yield break;
                }
                    

                spawningWeapon = true;
                AutoGiveItem.ResetVariables();
                Plugin.netHandler.SpawnWeaponReceive($"{Roles.CurrentRole.Type}/{Roles.CurrentRole.Name}", Plugin.localPlayer.playerClientId);

                yield return new WaitForSeconds(5f); // butler takes roughly 2.5s or less while nutcracker takes a bit longer
                AutoGiveItem.doneSpawningWeapons = true;

                yield return new WaitForSeconds(8);
                AutoGiveItem.doneSpawningWeapons = false;
                spawningWeapon = false;
            }
        }


    }
}
