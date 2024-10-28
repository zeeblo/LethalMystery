
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
                    case "Shapeshifter":
                        // change into another user
                        break;
                    case "Nightmare":
                        // Curse users
                        break;
                }
            }

            public string GetWeapon()
            {
                if (Type == "monster")
                {
                    return "Knife";
                }
                else if (Name == "Sherif")
                {
                    return "Shotgun";
                }
                else
                {
                    return "";
                }
            }
        }


        public static List<Role> allRoles = new List<Role>();
        public static void AppendRoles()
        {

            allRoles.Add(new Role("Employee",
            "Bring back items to the ship to meet the quota.",
            "employee")
            );

            allRoles.Add(new Role(
            "Sherif",
            "Kill the monster(s). Guessing wrong will get you killed.",
            "employee"
            ));

            allRoles.Add(new Role(
            "Monster",
            "Eliminate all the crew",
            "monster"
            ));

            allRoles.Add(new Role(
            "Shapeshifter",
            "Eliminate all the crew and press x to shapeshift",
            "monster"
            ));

            allRoles.Add(new Role(
            "Nightmare",
            "Curse users by disrupting their vision and silencing their voice.",
            "monster"
            ));

        }

        public static string? TopText;
        public static string? BottomText;
        public static Role? CurrentRole;


        public static void AssignRole()
        {
            System.Random randomNum = new System.Random();
            int index = randomNum.Next(0, allRoles.Count());
            Role role = allRoles[index];

            TopText = role.Name;
            BottomText = role.Desc;
            CurrentRole = role;
            ShowRole(role);
        }


        public static void ShowRole(Role role)
        {
            DialogueSegment[] array = new DialogueSegment[]
            {
                    new DialogueSegment
                    {
                        speakerText = role.Name,
                        bodyText = role.Desc,
                        waitTime = 14f
                    }
            };
            HUDManager.Instance.ReadDialogue(array);
        }


    }
}