using System.Collections.Generic;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Custom Death Messages", "Mr. Blue", "2.0.4")]
    [Description("Displays custom death messages")]
    class CustomDeathMessages : HurtworldPlugin
    {
        [PluginReference]
        private Plugin KillCounter;

        private void OnServerInitialized()
        {
            GameManager.Instance.ServerConfig.ChatDeathMessagesEnabled = false;
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string> {
                { "Creatures/Bor", "{Name} got killed by a Bor" },
                { "Creatures/DartBug", "{Name} got killed by a Dart Bug" },
                { "Creatures/Radiation Bor", "{Name} got killed by a Radiation Bor" },
                { "Creatures/Sasquatch", "{Name} got killed by a Sasquatch" },
                { "Creatures/Shigi", "{Name} got killed by a Shigi" },
                { "Creatures/Tokar", "{Name} got killed by a Tokar" },
                { "Creatures/Yeti", "{Name} got killed by a Yeti" },
                { "EntityStats/BinaryEffects/Asphyxiation", "{Name} has died from suffocation" },
                { "EntityStats/BinaryEffects/Burning", "{Name} has burned to death" },
                { "EntityStats/BinaryEffects/Drowning", "{Name} has drowned" },
                { "EntityStats/BinaryEffects/Hyperthermia", "{Name} has died from overheating" },
                { "EntityStats/BinaryEffects/Hypothermia", "{Name} has frozen to death" },
                { "EntityStats/BinaryEffects/Radiation Poisoning", "{Name} has died from radiation poisoning" },
                { "EntityStats/BinaryEffects/Starvation", "{Name} has starved to death" },
                { "EntityStats/BinaryEffects/Starving", "{Name} has starved to death" },
                { "EntityStats/BinaryEffects/Territory Control Lockout Damage", "{Name} got killed by Territory Control Lockout Damage" },
                { "EntityStats/Sources/Damage Over Time", "{Name} just died" },
                { "EntityStats/Sources/Explosives", "{Name} got killed by an explosion" },
                { "EntityStats/Sources/Fall Damage", "{Name} has fallen to their death" },
                { "EntityStats/Sources/Poison", "{Name} has died from poisoning" },
                { "EntityStats/Sources/Radiation", "{Name} has died from radiation" },
                { "EntityStats/Sources/Suicide", "{Name} has committed suicide" },
                { "EntityStats/Sources/a Vehicle Impact", "{Name} got run over by a vehicle" },
                { "Machines/Landmine", "{Name} got killed by a Landmine" },
                { "Machines/Medusa Vine", "{Name} got killed by a Medusa Trap" },
                { "Too Cold", "{Name} has frozen to death" },
                { "Unknown", "{Name} just died on a mystic way" },
                { "killcounter_player", "{Name} got killed by {Killer}[{Kills}]" },
                { "player", "{Name} got killed by {Killer}" }
            }, this);
        }

        private void SendMessage(string key, string name, string killerName = "", string killerKills = "")
        {
            foreach (PlayerSession s in GameManager.Instance.GetSessions().Values)
            {
                Player.Message(s, lang.GetMessage(key, this, s.SteamId.ToString())
                    .Replace("{Name}", name)
                    .Replace("{Killer}", killerName)
                    .Replace("{Kills}", killerKills));
            }

        }
        private void OnPlayerDeath(PlayerSession playerSession, EntityEffectSourceData dataSource)
        {
            string name = playerSession.Identity.Name;
            string SDKey = !string.IsNullOrEmpty(dataSource.SourceDescriptionKey) ? dataSource.SourceDescriptionKey : Singleton<GameManager>.Instance.GetDescriptionKey(dataSource.EntitySource);

            if (SDKey.EndsWith("(P)"))
            {
                string KillerName = SDKey.Substring(0, SDKey.Length - 3);

                if (KillCounter != null)
                {
                    var KillerKills = KillCounter.Call("AddKill", playerSession, dataSource);
                    SendMessage("killcounter_player", name, KillerName, (KillerKills ?? "?").ToString());
                }
                else
                    SendMessage("player", name, KillerName);
            }
            else
            {
                if (lang.GetMessage(SDKey, this) == SDKey)
                {
                    SendMessage("Unknown", name);
                    Puts("Found unknown SourceDescriptionKey: " + SDKey);
                }
                else
                    SendMessage(SDKey, name);
            }
        }
    }
}