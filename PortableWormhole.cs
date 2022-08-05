using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

//Original code by DaeCatt. ported to 1.4 and modified by Stonga. all other comments are written by DaeCatt unless specified otherwise

namespace PortableWormholeBeta
{
    public class PortableWormholeMod : Mod
    {

        public override void Load()
        {
            On.Terraria.Player.HasUnityPotion += HasPortableWormhole;
            On.Terraria.Player.TakeUnityPotion += TakePortableWormhole;
        }

        private static bool HasPortableWormhole(On.Terraria.Player.orig_HasUnityPotion orig, Player player)
        //we allow the player to use wormhole potion functions if they have the Portable Wormhole item instead. -Stonga
        {
            PortableWormholePlayer modPlayer = player.GetModPlayer<PortableWormholePlayer>();

            return modPlayer.hasPortableWormhole || orig(player);
        }
        private static void TakePortableWormhole(On.Terraria.Player.orig_TakeUnityPotion orig, Player player)
        //we don't want to consume a wormhole potion if the player has the Portable Wormhole item -Stonga
        {
            PortableWormholePlayer modPlayer = player.GetModPlayer<PortableWormholePlayer>();

            if (modPlayer.hasPortableWormhole)
                return;
            else
                orig(player);
        }

    }

    public class PortableWormholeSystem : ModSystem
    {
        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            PortableWormholePlayer modPlayer = Main.LocalPlayer.GetModPlayer<PortableWormholePlayer>();
            // Don't do anything the player does not have the portable wormhole
            if (!modPlayer.hasPortableWormhole)
                return;

            // Try to avoid falsely highlighting NPCs when the player intends to teleport to another player.
            if (Main.netMode == NetmodeID.MultiplayerClient && Main.LocalPlayer.team > 0 && Main.instance.unityMouseOver)
                return;

            float scale = Main.mapFullscreenScale / 16f;
            float dx = Main.screenWidth / 2 - Main.mapFullscreenPos.X * Main.mapFullscreenScale;
            float dy = Main.screenHeight / 2 - Main.mapFullscreenPos.Y * Main.mapFullscreenScale;

            for (int i = 0; i < Main.npc.Length; i++)
            {
                // Only check active NPCs that are set to townNPC.
                if (!Main.npc[i].active || !Main.npc[i].townNPC)
                    continue;

                // Ensure this NPC has a head texture
                int headIndex = NPC.TypeToDefaultHeadIndex(Main.npc[i].type);
                if (headIndex <= 0)
                    continue;

                Texture2D headTexture = TextureAssets.NpcHead[headIndex].Value;

                // Calculate the NPCs position on the screen
                float x = dx + scale * (Main.npc[i].position.X + Main.npc[i].width / 2);
                float y = dy + scale * (Main.npc[i].position.Y + Main.npc[i].gfxOffY + Main.npc[i].height / 2);

                float minX = x - headTexture.Width / 2 * Main.UIScale;
                float minY = y - headTexture.Height / 2 * Main.UIScale;
                float maxX = minX + headTexture.Width * Main.UIScale;
                float maxY = minY + headTexture.Height * Main.UIScale;

                // Determine whether the player is hovering this NPCs head.
                if (Main.mouseX >= minX && Main.mouseX <= maxX && Main.mouseY >= minY && Main.mouseY <= maxY)
                {
                    SpriteEffects effect = Main.npc[i].direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    Main.spriteBatch.Draw(headTexture, new Vector2(x, y), headTexture.Frame(), Color.White, 0f, headTexture.Frame().Size() / 2, Main.UIScale + 0.5f, effect, 0f);

                    //if (!Main.instance.unityMouseOver)
                    //	SoundEngine.PlaySound(SoundID.MenuTick);

                    //Main.instance.unityMouseOver = true;

                    // Change the tooltip to "Teleport to ..."
                    mouseText = Language.GetTextValue("Game.TeleportTo", Main.npc[i].FullName);

                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        Main.mouseLeftRelease = false;
                        Main.mapFullscreen = false;

                        // Display "Player has teleported to ..." message
                        Main.NewText(Language.GetTextValue("Game.HasTeleportedTo", Main.player[Main.myPlayer].name, Main.npc[i].FullName), 255, 255, 0);

                        // Teleport the player to the NPC
                        Main.player[Main.myPlayer].Teleport(Main.npc[i].position);
                    }

                    return;
                }
            }
        }
    }

    public class PortableWormhole : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Teleport to party members or NPCs by clicking their head on the fullscreen map\nWorks in Piggy Bank, Safe, Void Vault, and Defender's Forge");
        }

        public override void SetDefaults()
        {
            Item.width = 15;
            Item.height = 15;
            Item.maxStack = 1;
            Item.consumable = false;
            Item.rare = ItemRarityID.Orange;
        }

        public override bool CanUseItem(Player player)
        {
            return false;
        }

        public override void UpdateInventory(Player player)
        {
            PortableWormholePlayer modPlayer = player.GetModPlayer<PortableWormholePlayer>();
            modPlayer.hasPortableWormhole = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.WormholePotion, 30);
            recipe.AddTile(TileID.CrystalBall);
            recipe.Register();
        }
    }

    public class PortableWormholePlayer : ModPlayer
    {
        public bool hasPortableWormhole = false;
        public override void ResetEffects()
        {
            hasPortableWormhole = false;
        }
        
        public override void UpdateEquips()
        {
            base.UpdateEquips();

            Item[] PiggyBank = Player.bank.item;
            Item[] Safe = Player.bank2.item;
            Item[] Forge = Player.bank3.item;
            Item[] Void = Player.bank4.item;

            for (int i = 0; i < PiggyBank.Length; i++)
            {
                if (PiggyBank[i].type == ModContent.ItemType<PortableWormhole>())
                    hasPortableWormhole = true;
            }
            for (int i = 0; i < Safe.Length; i++)
            {
                if (Safe[i].type == ModContent.ItemType<PortableWormhole>())
                    hasPortableWormhole = true;
            }
            for (int i = 0; i < Forge.Length; i++)
            {
                if (Forge[i].type == ModContent.ItemType<PortableWormhole>())
                    hasPortableWormhole = true;
            }
            for (int i = 0; i < Void.Length; i++)
            {
                if (Void[i].type == ModContent.ItemType<PortableWormhole>())
                    hasPortableWormhole = true;
            }

        }
    }
}