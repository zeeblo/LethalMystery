using LethalMystery.Utils;
using UnityEngine;


namespace LethalMystery.Players
{
    public class Roles
    {
        public class Role
        {
            public string Name { get; }
            public string Desc { get; }
            public string Type { get; }

            public Role(string roleName, string desc, string type)
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
                if (Type == "_monster")
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
                if (Type == "_monster" && Plugin.KnifeIcon != null)
                {
                    return Plugin.KnifeIcon;
                }
                else
                {
                    return Icon;
                }
            }
        }


        public static List<Role> allRoles = new List<Role>();
        public static List<string> allItems = new List<string>();
        public static string? TopText;
        public static string? BottomText;
        public static Role? CurrentRole;
        public static void AppendRoles()
        {
            /*
            allRoles.Add(new Role(
            "employee",
            "Bring back items to the ship to meet the quota.",
            "_employee")
            );
             */

            allRoles.Add(new Role(
            "sheriff",
            "Kill the monster(s). Guessing wrong will get you killed.",
            "_employee"
            ));
            
            /*
            allRoles.Add(new Role(
            "monster",
            "Eliminate all the crew",
            "_monster"
            ));

            allRoles.Add(new Role(
            "shapeshifter",
            "Eliminate all the crew and press x to shapeshift",
            "_monster"
            ));

            allRoles.Add(new Role(
            "nightmare",
            "Curse users by disrupting their vision and silencing their voice.",
            "_monster"
            ));
            */
        }


        public static void AppendItems()
        {
            allItems.Add("kitchen knife");
            allItems.Add("knifeitem");
            allItems.Add("knife");
            allItems.Add("shotgun");
            allItems.Add("shotgunitem");
            allItems.Add("clipboard");

        }


        public static void AssignRole()
        {
            System.Random randomNum = new System.Random();
            int index = randomNum.Next(0, allRoles.Count());
            Role role = allRoles[index];

            TopText = role.Name;
            BottomText = role.Desc;
            CurrentRole = role;
            
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