using LethalMystery.Utils;
using UnityEngine;


namespace LethalMystery.Players
{
    public class Roles
    {

        public static List<Role> allRoles = new List<Role>();
        public static List<string> allItems = new List<string>();
        public static string? TopText;
        public static string? BottomText;
        public static Role? CurrentRole;
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
                if (Type == RoleType.monster && Plugin.KnifeIcon != null)
                {
                    return Plugin.KnifeIcon;
                }
                else
                {
                    return Icon;
                }
            }
        }


        public static void AppendRoles()
        {
            /*
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
            */

            allRoles.Add(new Role(
            "monster",
            "Eliminate all the crew",
            RoleType.monster
            ));

            allRoles.Add(new Role(
            "shapeshifter",
            "Eliminate all the crew and press x to shapeshift",
            RoleType.monster
            ));

            
        }



        public static void AssignRole()
        {
            System.Random randomNum = new System.Random();
            int index = randomNum.Next(0, allRoles.Count());
            Role role = allRoles[index];

            TopText = role.Name;
            BottomText = role.Desc;
            CurrentRole = role;

            SlotAmount();
        }


        private static void SlotAmount()
        {
            if (CurrentRole?.Name == "sheriff")
            {
                MoreSlots.AllowMoreSlots();
            }
            else if (CurrentRole?.Type == RoleType.monster)
            {
                MoreSlots.AllowMoreSlots();
            }
            else
            {
                MoreSlots.DefaultSlots();
            }
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


        public static void ResetVariables()
        {
            TopText = null;
            BottomText = null;
            CurrentRole = null;
        }

    }
}