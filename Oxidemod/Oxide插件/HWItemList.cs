namespace Oxide.Plugins
{
    using System.IO;
    using Oxide.Core;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [Info("HW Item List", "klauz24", 1.4), Description("Allows you to get all in-game items with their properties")]
    internal class HWItemList : HurtworldPlugin
    {
        public int protocolVersion = GameManager.PROTOCOL_VERSION;

        private struct Items
        {
            public int itemId;
            public string itemName,
                itemFullNameKey,
                itemGuid;
        }

        private Configuration _config;

        private class Configuration
        {
            [JsonProperty(PropertyName = "Protocol version")]
            public int ProtocolVersion { get; set; } = GameManager.PROTOCOL_VERSION;

            [JsonProperty(PropertyName = "Reset on load (Enable it if you are using any Steam Workshop mods)")]
            public bool ResetOnLoad { get; set; } = false;

            [JsonProperty(PropertyName = "List of items")]
            public List<object> ItemList { get; set; } = new List<object>();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();

                if (_config == null)
                {
                    throw new JsonException();
                }
            }
            catch
            {
                string configPath = $"{Interface.Oxide.ConfigDirectory}{Path.DirectorySeparatorChar}{Name}";
                PrintWarning($"Could not load a valid configuration file, creating a new configuration file at {configPath}.json");
                Config.WriteObject(_config, false, $"{configPath}_invalid.json");
                LoadDefaultConfig();
                SaveConfig();
            }
        }

        protected override void LoadDefaultConfig() => _config = new Configuration();

        protected override void SaveConfig() => Config.WriteObject(_config);

        private void OnServerInitialized() => CheckConfig();

        private void CheckConfig()
        {
            if (_config.ItemList.IsNullOrEmpty())
            {
                GetItemList();
            }
            else
            {
                if (_config.ResetOnLoad)
                {
                    GetItemList();
                }

                if (_config.ProtocolVersion < protocolVersion)
                {
                    _config.ProtocolVersion = protocolVersion;
                    GetItemList();
                }
            }
        }

        private void GetItemList()
        {
            _config.ItemList.Clear();
            Dictionary<int, ItemGeneratorAsset>.Enumerator itemsEnumerator = GlobalItemManager.Instance.GetGenerators().GetEnumerator();
            while (itemsEnumerator.MoveNext())
            {
                ItemGeneratorAsset item = itemsEnumerator.Current.Value;
                Items newItemList = new Items
                {
                    itemId = item.GeneratorId,
                    itemName = item.ToString(),
                    itemFullNameKey = item.GetNameKey(),
                    itemGuid = RuntimeHurtDB.Instance.GetGuid(item)
                };
                _config.ItemList.Add(newItemList);
            }
            SaveConfig();
        }
    }
}