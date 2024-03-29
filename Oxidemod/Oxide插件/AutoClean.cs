using System;
using Oxide.Core;
using UnityEngine;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("AutoClean", "klauz24", "1.0.6")]
    [Description("Provides automatic cleaning of objects outside of claimed areas")]
    class AutoClean : HurtworldPlugin
    {
        Dictionary<string, int> _ObjectList = new Dictionary<string, int>();
        void SaveObjects() => Interface.GetMod().DataFileSystem.WriteObject("AutoClean/ObjectList", _ObjectList);
        OwnershipStakeServer stake;

        const string perm = "autoclean.admin";

        void Init()
        {
            permission.RegisterPermission(perm, this);
        }

        void OnServerInitialized()
        {
            if ((bool)Config["AutoPruneLog"]) { PruneLog(); }
            timer.Repeat(Convert.ToSingle(Config["UpdateIntervalSeconds"]), 0, () => { DoClean(); });
            if ((bool)Config["LoadPreviousDecayIntervals"])
                _ObjectList = Interface.GetMod().DataFileSystem.ReadObject<Dictionary<string, int>>("AutoClean/ObjectList");
        }

        int destroyed = 0;
        int hasstake = 0;
        int nostake = 0;
        int num = 0;

        protected override void LoadDefaultConfig()
        {
            if (Config["UpdateIntervalSeconds"] == null) Config.Set("UpdateIntervalSeconds", 7200);
            if (Config["IntervalsBeforeCleaning"] == null) Config.Set("IntervalsBeforeCleaning", 24);
            if (Config["ShowConsoleMessages"] == null) Config.Set("ShowConsoleMessages", true);
            if (Config["LoadPreviousDecayIntervals"] == null) Config.Set("LoadPreviousDecayIntervals", true);
            if (Config["AutoPruneLog"] == null) Config.Set("AutoPruneLog", true);
            SaveConfig();
        }

        protected override void LoadDefaultMessages()
        {
            var messages = new Dictionary<string, string>
            {
                {"nopermission","AutoClean: You do not have Permission to do this!"},
                {"prunesuccess","AutoClean: ObjectList has been Pruned successfully"}
            };

            lang.RegisterMessages(messages, this);
        }
        string Msg(string msg, string SteamId = null) => lang.GetMessage(msg, this, SteamId);

        [ChatCommand("clean")]
        void manualClean(PlayerSession session)
        {
            if (permission.UserHasPermission(session.SteamId.ToString(), perm) || session.IsAdmin)
            {
                DoClean(false);
            }
            else
                hurt.SendChatMessage(session, null, Msg("nopermission", session.SteamId.ToString()));
        }

        void PruneLog()
        {
            List<string> existing = new List<string>();
            List<string> prune = new List<string>();
            int chked = 0;
            int pruned = 0;
            int safe = 0;
            foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                uLink.NetworkView nwv = obj.GetComponent<uLink.NetworkView>();
                if (nwv != null && nwv.isActiveAndEnabled)
                    existing.Add(nwv.viewID.id.ToString());
            }
            foreach (string key in _ObjectList.Keys)
            {
                chked++;
                if (existing.Contains(key))
                    safe++;
                else
                {
                    pruned++;
                    prune.Add(key);
                }
            }
            foreach (string s in prune)
                _ObjectList.Remove(s);
            Puts("Checked: " + chked);
            Puts("Correct: " + safe);
            Puts("Pruned:  " + pruned);
            SaveObjects();
        }
        void DoClean(bool automatic = true)
        {
            hasstake = 0;
            nostake = 0;
            destroyed = 0;
            num = 0;
            float t = 0;
            if (automatic)
            {
                Server.Broadcast("<color=yellow>[DECAY]</color> Building Decay updating in 5 seconds.");
                t = 5f;
            }
            timer.Once(t, () =>
            {
                foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (obj.name.Contains("Constructed") || obj.name.Contains("StructureManager") || obj.name.Contains("Dynamic") || obj.name.Contains("LadderCollider(Clone)"))
                    {
                        var thecell = ConstructionUtilities.GetOwnershipCell(obj.transform.position);
                        if (thecell != null)
                        {
                            var nView = uLink.NetworkView.Get(obj);
                            if (nView != null && nView.isActiveAndEnabled)
                            {
                                string ID = nView.viewID.id.ToString();
                                ConstructionManager.Instance.OwnershipCells.TryGetValue(ConstructionUtilities.GetOwnershipCell(obj.transform.position), out stake);
                                if (stake == null)
                                {
                                    nostake++;
                                    if (_ObjectList.TryGetValue(ID, out num))
                                    {
                                        if (num == Convert.ToInt32(Config["IntervalsBeforeCleaning"]))
                                        {
                                            try
                                            {
                                                foreach (ShackDynamicServer shack in Resources.FindObjectsOfTypeAll<ShackDynamicServer>())
                                                {
                                                    if (shack.gameObject.GetComponent<uLink.NetworkView>().isActiveAndEnabled)
                                                    {
                                                        MeshRenderer mf = shack.gameObject.transform.GetChild(3).GetComponent<MeshRenderer>();
                                                        if (!mf.bounds.Contains(obj.gameObject.transform.position) && !obj.name.Contains("Shack"))
                                                        {
                                                            Singleton<HNetworkManager>.Instance.NetDestroy(nView.HNetworkView());
                                                            _ObjectList.Remove(ID);
                                                            destroyed++;
                                                        }
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        else
                                            _ObjectList[ID] += 1;
                                    }
                                    else
                                        _ObjectList.Add(ID, 1);
                                }
                                else
                                {
                                    hasstake++;
                                    if (_ObjectList.TryGetValue(ID, out num))
                                    {
                                        _ObjectList.Remove(ID);
                                    }
                                }
                            }
                        }
                    }
                }
                SaveObjects();
                if ((bool)Config["ShowConsoleMessages"])
                {
                    Puts("Has Stake: " + hasstake);
                    Puts("No Stake : " + nostake);
                    Puts("Destroyed: " + destroyed);
                }
            });
        }
    }
}