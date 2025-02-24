using GameNetcodeStuff;
using LethalMystery.Utils;
using UnityEngine;

namespace LethalMystery.Players
{
    public class Roles
    {

        public static List<Role> allRoles = new List<Role>();
        public static List<string> allItems = new List<string>();
        public static Role? CurrentRole;
        public static Dictionary<ulong, string> localPlayerRoles = new Dictionary<ulong, string>();

        public enum RoleType
        {
            employee,
            monster
        }

        public class Role
        {
            public string Name { get; }
            public string Desc { get; }
            public RoleType Type { get; }

            public Role(string roleName, string desc, RoleType type)
            {
                Name = roleName;
                Desc = desc;
                Type = type;
            }

            public void UseAbility()
            {
                switch (Name)
                {
                    case "shapeshifter":
                        // change into another user
                        break;
                    case "nightmare":
                        // Curse users
                        break;
                }
            }

            public string GetItem()
            {
                if (Type == RoleType.monster)
                {
                    return "knife";
                }
                else if (Name == "sheriff")
                {
                    return "shotgun";
                }
                else
                {
                    return "";
                }
            }

            public Sprite GetIcon(Sprite Icon)
            {
                if (Type == RoleType.monster && LMAssets.KnifeIcon != null)
                {
                    return LMAssets.KnifeIcon;
                }
                else
                {
                    return Icon;
                }
            }
        }



        public static void AppendRoles()
        {
            
            allRoles.Add(new Role(
            "employee",
            "Bring back items to the ship to meet the quota.",
            RoleType.employee)
            );
            

            allRoles.Add(new Role(
            "sheriff",
            "Kill the monster(s). Guessing wrong will get you killed.",
            RoleType.employee
            ));


            
           allRoles.Add(new Role(
           "monster",
           "Eliminate all the crew",
           RoleType.monster
           ));

            /*
           allRoles.Add(new Role(
           "shapeshifter",
           "Eliminate all the crew and press x to shapeshift",
           RoleType.monster
           ));
           */

        }



        public static void AssignRole()
        {
            if (!Plugin.localPlayer.IsHost) return;

            Dictionary<ulong, string> rawAllPlayers = new Dictionary<ulong, string>();
            List<Role> specialRoles = new List<Role>();
            List<Role> takenRoles = new List<Role>();
            List<ulong> playerIDS = new List<ulong>();

            playerIDS.Add(0); // adds host id
            foreach (PlayerControllerB plr in StartOfRound.Instance.allPlayerScripts)
            {
                if (plr.actualClientId == 0) continue;

                // add rest of client ids
                playerIDS.Add(plr.playerClientId);


            }

            foreach (Role rl in allRoles)
            {
                if ( (rl.Type == RoleType.monster && rl.Name != "employee") || rl.Name == "sheriff")
                {
                    specialRoles.Add(rl);
                }
            }

            // Assign Special Roles
            int specialRoleAmt = specialRoles.Count();
            for (int i = 0; i < specialRoleAmt ; i++)
            {
                if (playerIDS.Count == 0) break;

                System.Random randomNum = new System.Random();
                System.Random randomPlr = new System.Random();
                int index = randomNum.Next(0, specialRoles.Count());
                int plrIndex = randomPlr.Next(0, playerIDS.Count());
                //int plrIndex = playerIDS.IndexOf(1); // REMEMBER TO CHANGE BACK TO CODE ABOVE

                Role role = specialRoles[index];
                ulong plrID = playerIDS[plrIndex];

                if (rawAllPlayers.ContainsKey(plrID)) continue;
                rawAllPlayers.Add(plrID, role.Name);

                specialRoles.RemoveAt(index);
                playerIDS.RemoveAt(plrIndex);
            }

            // Set remaining players to have the employee role
            foreach (ulong id in playerIDS)
            {
                rawAllPlayers.Add(id, "employee");
            }

            Plugin.netHandler.SetallPlayerRoles(rawAllPlayers);

        }


        public static void ShowRole(Role role)
        {
            DialogueSegment[] array = new DialogueSegment[]
            {
                    new DialogueSegment
                    {
                        speakerText = StringAddons.Title(role.Name),
                        bodyText = role.Desc,
                        waitTime = 14f
                    }
            };
            HUDManager.Instance.ReadDialogue(array);
        }


        public static bool NameIsMonsterType(string name)
        {
            foreach (Role r in allRoles)
            {
                if ( (name.ToLower() == r.Name.ToLower()) && (r.Type == RoleType.monster) ) return true;
            }
            return false;
        }

        public static void ResetVariables()
        {
            CurrentRole = null;
            if (Plugin.isHost)
            {
                Plugin.netHandler.SetallPlayerRoles(new Dictionary<ulong, string>());
            }
        }

    }
}