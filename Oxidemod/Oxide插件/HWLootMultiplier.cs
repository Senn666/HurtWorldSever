using System.IO;
using Oxide.Core;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Plugins
{
	[Info("HW Loot Multiplier", "klauz24", 1.1), Description("Simple loot multiplier for your Hurtworld server")]
	internal class HWLootMultiplier : HurtworldPlugin
	{
		private PlayerIdentity _owner;

		#region Config

		private Configuration _config;

		private class Configuration
		{
			[JsonProperty(PropertyName = "Global multiplier")]
			public int GlobalMultiplier { get; set; } = 1;

			[JsonProperty(PropertyName = "Enable global multiplier")]
			public bool EnableGlobalMultiplier { get; set; } = false;

			[JsonProperty(PropertyName = "Plants")]
			public int Plants { get; set; } = 1;

			[JsonProperty(PropertyName = "Animals")]
			public int Animals { get; set; } = 1;

			[JsonProperty(PropertyName = "Airdrop loot")]
			public int Airdrop { get; set; } = 1;

			[JsonProperty(PropertyName = "Loot frenzy loot")]
			public int LootFrenzy { get; set; } = 1;

			[JsonProperty(PropertyName = "Mining drills")]
			public int MiningDrills { get; set; } = 1;

			[JsonProperty(PropertyName = "Gather resources")]
			public int Gather { get; set; } = 1;

			[JsonProperty(PropertyName = "Pick up resources")]
			public int PickUp { get; set; } = 1;

			[JsonProperty(PropertyName = "Explodable mining rocks")]
			public int ExplodableMiningRock { get; set; } = 1;

			[JsonProperty(PropertyName = "Town event (Amount of cases)")]
			public int TownEvent { get; set; } = 1;

			[JsonProperty(PropertyName = "Town case T1")]
			public int TownHardCaseT1 { get; set; } = 1;

			[JsonProperty(PropertyName = "Town case T2")]
			public int TownHardCaseT2 { get; set; } = 1;

			[JsonProperty(PropertyName = "Fragments case T1")]
			public int FragmentsT1 { get; set; } = 1;

			[JsonProperty(PropertyName = "Fragments case T2")]
			public int FragmentsT2 { get; set; } = 1;

			[JsonProperty(PropertyName = "Fragments case T3")]
			public int FragmentsT3 { get; set; } = 1;
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

		#endregion

		#region uMod Hooks

		private void OnPlantGather(GrowingPlantUsable plant, WorldItemInteractServer player, List<ItemObject> items)
		{
			this._owner = GameManager.Instance.GetIdentity(player.networkView.owner);
			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					items[i].StackSize = defaultStack * _config.Plants;
				}

				items[i].InvalidateStack();
			}
		}

		private void OnDispenserGather(GameObject obj, HurtMonoBehavior player, List<ItemObject> items)
		{
			this._owner = GameManager.Instance.GetIdentity(player.networkView.owner);
			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					items[i].StackSize = defaultStack * _config.Gather;
				}

				items[i].InvalidateStack();
			}
		}

		private void OnDrillDispenserGather(GameObject obj, DrillMachine machine, List<ItemObject> items)
		{
			this._owner = GameManager.Instance.GetIdentity(machine.networkView.owner);
			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					items[i].StackSize = defaultStack * _config.MiningDrills;
				}

				items[i].InvalidateStack();
			}
		}

		private void OnAirdrop(GameObject obj, AirDropEvent airdrop, List<ItemObject> items)
		{
			this._owner = GameManager.Instance.GetIdentity(airdrop.networkView.owner);
			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					items[i].StackSize = defaultStack * _config.Airdrop;
				}

				items[i].InvalidateStack();
			}
		}

		private void OnCollectiblePickup(LootOnPickup node, WorldItemInteractServer player, List<ItemObject> items)
		{
			this._owner = GameManager.Instance.GetIdentity(player.networkView.owner);
			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					items[i].StackSize = defaultStack * _config.PickUp;
				}

				items[i].InvalidateStack();
			}
		}

		private void OnEntityDropLoot(GameObject obj, List<ItemObject> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					items[i].StackSize = defaultStack * _config.Animals;
				}

				items[i].InvalidateStack();
			}
		}

		private void OnControlTownDrop(GameObject obj, ControlTownEvent townEvent, List<ItemObject> items)
		{
			this._owner = GameManager.Instance.GetIdentity(townEvent.networkView.owner);
			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					items[i].StackSize = defaultStack * _config.TownEvent;
				}

				items[i].InvalidateStack();
			}
		}

		private void OnLootFrenzySpawn(GameObject obj, LootFrenzyTownEvent frenzyEvent, List<ItemObject> items)
		{
			this._owner = GameManager.Instance.GetIdentity(frenzyEvent.networkView.owner);
			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					items[i].StackSize = defaultStack * _config.LootFrenzy;
				}

				items[i].InvalidateStack();
			}
		}

		private void OnMiningRockExplode(GameObject obj, ExplodableMiningRock rock, List<ItemObject> items)
		{
			this._owner = GameManager.Instance.GetIdentity(rock.networkView.owner);
			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					items[i].StackSize = defaultStack * _config.ExplodableMiningRock;
				}

				items[i].InvalidateStack();
			}
		}

		private void OnLootCaseOpen(ItemComponentLootCase lootCase, ItemObject obj, Inventory inv, List<ItemObject> items)
		{
			this._owner = GameManager.Instance.GetIdentity(inv.networkView.owner);
			string ltn = lootCase.LootTree.name,
				caseT1 = "TownEventHardcaseLoot T1",
				caseT2 = "TownEventHardcaseLoot",
				fragmentsT1 = "Fragments Tier 1",
				fragmentsT2 = "Fragments Tier 2",
				fragmentsT3 = "Fragments Tier 3";

			for (int i = 0; i < items.Count; i++)
			{
				int defaultStack = items[i].StackSize;
				if (_config.EnableGlobalMultiplier)
				{
					items[i].StackSize = defaultStack * _config.GlobalMultiplier;
				}
				else
				{
					if (ltn.Equals(caseT1))
					{
						items[i].StackSize = defaultStack * _config.TownHardCaseT1;
					}

					if (ltn.Equals(caseT2))
					{
						items[i].StackSize = defaultStack * _config.TownHardCaseT2;
					}

					if (ltn.Equals(fragmentsT1))
					{
						items[i].StackSize = defaultStack * _config.FragmentsT1;
					}

					if (ltn.Equals(fragmentsT2))
					{
						items[i].StackSize = defaultStack * _config.FragmentsT2;
					}

					if (ltn.Equals(fragmentsT3))
					{
						items[i].StackSize = defaultStack * _config.FragmentsT3;
					}
				}

				items[i].InvalidateStack();
			}
		}

		#endregion
	}
}
 