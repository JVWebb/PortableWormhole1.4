using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using Terraria;

namespace PortableWormhole
{
    public class PortableWormholeConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Label("Allow teleporting to NPCs")]
        [Tooltip("Allows players to teleport to town NPCs")]
        [DefaultValue(true)]
        public bool AllowTeleportingToNPCs;

        [Label("Allow using from banks")]
        [Tooltip("Allows players to use the functions of the Portable Wormhole while it is in their piggy bank, safe, void vault, or defender's forge")]
        [DefaultValue(true)]
        public bool AllowUsingFromBanks;

        [Label("Require Crystal Ball")]
        [Tooltip("Disable this to allow crafting the Portable Wormhole by hand (no crafting station needed)")]
        [ReloadRequired]
        [DefaultValue(true)]
        public bool RequireCrystalBall;

        [Label("# of Wormhole Potions to craft")]
        [Tooltip("Number of Wormhole Potions required to craft the Portable Wormhole")]
        [ReloadRequired]
        [Range(0, 300)]
        [DefaultValue(30)]
        public int NumRequiredWormholePotions;

    }
}