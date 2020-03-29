using Chroma.Settings;
using Chroma.Utils;

namespace Chroma.Misc
{
    internal static class Greetings
    {
        //Portal quotes, Doki-Doki Literature Club quotes, you know - Skynet type stuff, with the occasional wholesome comment thrown in
        private static string[] GetGreeting(ulong id, string name)
        {
            if (name.ToLower() == "cyansnow") return new string[] { "Cyan is a furry" };

            int i = UnityEngine.Random.Range(0, 24);

            switch (i)
            {
                default:
                    return new string[] {
                        $"Welcome, {name} !",
                    };

                case 0:
                    return new string[] {
                        $"HELLO {id}- WHOOPS I MEAN {name.ToUpper()} HAHA!",
                    };

                case 1:
                    return new string[] {
                        $"Greetings, meatba- I MEAN {name}!",
                    };

                case 2:
                    return new string[] {
                        $"Hi {name}!  Good to see you again!  Isn't the weather great today?!",
                    };

                case 3:
                    return new string[] {
                        $"gosgh%SYyh6&Ulj;d.sfgn <<< {name} <<< hdfh%$66778G;'s/dfg6,74",
                    };
                case 4:
                    return new string[] {
                        "NullReferenceException: object reference not set to an instance of an object",
                        "at Human.GetFriends()",
                        $"at Human.FindByName(\"{name}\").GetFriends().Count()"
                    };

                case 5:
                    return new string[] {
                        $"Hey {name} did you know, if you subtract {(id - 10)} from your ID you get how attractive you are?",
                    };

                case 6:
                    return new string[] {
                        $"I really like the sound of rain, {name}..."
                    };

                case 7:
                    return new string[] {
                        $"{name}, do you believe in God?"
                    };

                case 8:
                    return new string[] {
                        $"{name}, have you ever wondered what it feels like to die?"
                    };

                case 9:
                    return new string[] {
                        "Hey, are you having a bad day or anything like that?"
                    };

                case 10:
                    return new string[] {
                        $"{name}, do you get good sleep?"
                    };

                case 11:
                    return new string[] {
                        "I was thinking about >>> G%$&ik;,'kjgh] >>> earlier..."
                    };
                case 12:
                    return new string[] {
                        $"Stay with me forever, {name}"
                    };

                case 13:
                    return new string[] {
                        $"Hello {name}, welcome to the Chroma computer-aided enrichment center."
                    };

                case 14:
                    return new string[] {
                        $"You look great, by the way, {name}. Very healthy."
                    };

                case 15:
                    return new string[] {
                        "This next test involves blocks.",
                        $"You remember those, right, {name}?",
                    };

                case 16:
                    return new string[] {
                        "I honestly, truly didn’t think you’d fall for this..."
                    };

                case 17:
                    return new string[] {
                        $"gosgh%SYyh6&Ulj;d.sfgn <<< {name} <<< hdfh%$66778G;'s/dfg6,74",
                    };

                case 18:
                    return new string[] {
                        $"Oh, it’s {name}. It’s been a long time. How have you been?",
                    };

                case 19:
                    return new string[] {
                        $"{name.ToUpper()} IS YOU",
                    };

                case 20:
                    return new string[] {
                        $"What is a {name}? A miserable little pile of blocks!",
                    };

                case 21:
                    return new string[] {
                        $"{name}. {name} never changes.",
                    };

                case 22:
                    return new string[] {
                        $"The Chroma Enrichment Center is required to remind you that you will be baked, and then there will be cake.",
                    };

                case 23:
                    return new string[] {
                        $"The colors, {name}! What do they mean?",
                    };

                case 24:
                    return new string[] {
                        "You've met with a terrible fate, haven't you?",
                    };
            }
        }

        internal static void RegisterChromaSideMenu()
        {
            SidePanelUtil.RegisterTextPanel("CHROMA",
                ResourceTextFiles.chromaNotes
                .Replace("%VER%", Plugin.Version)
                .Replace("%USERNAME%", ChromaConfig.Username ?? "null")
                .Replace("%GREETING%", string.Join("\n", GetGreeting(ChromaConfig.UserID, ChromaConfig.Username ?? "null")))
                );

            SidePanelUtil.RegisterTextPanel("CHROMAWAIVER", ResourceTextFiles.safetyWaiver);
        }
    }
}