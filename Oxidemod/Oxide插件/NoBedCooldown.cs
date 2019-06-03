namespace Oxide.Plugins
{
    [Info("No Bed Cooldown", "klauz24", 1.2)]
    [Description("Players with permission wouldn't have cooldown after respawning at their beds")]
    class NoBedCooldown : HurtworldPlugin
    {
        const string perm = "nobedcooldown.use";
        const int bedPermCD = 0;
		const int bedDefaultCD = 180;

        void OnPlayerRespawn(PlayerSession session)
        {
            noBedCooldown(session);
        }

        void Init()
        {
            permission.RegisterPermission(perm, this);
        }

        void noBedCooldown(PlayerSession session)
        {
            var beds = BedMachineServer.GetEnumerator();
            while (beds.MoveNext())
            {
                var bed = beds.Current.Value;
                if (permission.UserHasPermission(session.SteamId.ToString(), perm))
                    bed.RespawnCooldownTime = bedPermCD;
                else
                    bed.RespawnCooldownTime = bedDefaultCD;
            }
        }
    }
}

