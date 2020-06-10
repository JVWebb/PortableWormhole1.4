using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Mono.Cecil.Cil.OpCodes;

namespace PortableWormhole {
	public class PortableWormholeMod : Mod {
		private protected static Func<Player, bool> PortableWormholeDelegate = (player) => {
			PortableWormholePlayer modPlayer = player.GetModPlayer<PortableWormholePlayer>();
			return modPlayer.hasPortableWormhole;
		};

		private void HookHasUnityPotion(ILContext il) {
			ILCursor cursor = new ILCursor(il);

			// if (portableWormholePlayer.hasPortableWormhole) return true;
			cursor.Emit(Ldarg_0);
			cursor.EmitDelegate(PortableWormholeDelegate);

			ILLabel label = il.DefineLabel();
			cursor.Emit(Brfalse, label);
			cursor.Emit(Ldc_I4_1);
			cursor.Emit(Ret);
			cursor.MarkLabel(label);
		}

		private void HookTakeUnityPotion(ILContext il) {
			ILCursor cursor = new ILCursor(il);

			// if (portableWormholePlayer.hasPortableWormhole) return;
			cursor.Emit(Ldarg_0);
			cursor.EmitDelegate(PortableWormholeDelegate);

			ILLabel label = il.DefineLabel();
			cursor.Emit(Brfalse, label);
			cursor.Emit(Ret);
			cursor.MarkLabel(label);
		}

		public override void Load() {
			IL.Terraria.Player.HasUnityPotion += HookHasUnityPotion;
			IL.Terraria.Player.TakeUnityPotion += HookTakeUnityPotion;
		}

		public override void PostDrawFullscreenMap(ref string mouseText) {
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

			for (int i = 0; i < Main.npc.Length; i++) {
				// Only check active NPCs that are set to townNPC.
				if (!Main.npc[i].active || !Main.npc[i].townNPC)
					continue;

				// Ensure this NPC has a head texture
				int headIndex = NPC.TypeToHeadIndex(Main.npc[i].type);
				if (headIndex <= 0)
					continue;

				Texture2D headTexture = Main.npcHeadTexture[headIndex];

				// Calculate the NPCs position on the screen
				float x = dx + scale * (Main.npc[i].position.X + Main.npc[i].width / 2);
				float y = dy + scale * (Main.npc[i].position.Y + Main.npc[i].gfxOffY + Main.npc[i].height / 2);

				float minX = x - headTexture.Width / 2 * Main.UIScale;
				float minY = y - headTexture.Height / 2 * Main.UIScale;
				float maxX = minX + headTexture.Width * Main.UIScale;
				float maxY = minY + headTexture.Height * Main.UIScale;

				// Determine whether the player is hovering this NPCs head.
				if (Main.mouseX >= minX && Main.mouseX <= maxX && Main.mouseY >= minY && Main.mouseY <= maxY) {
					SpriteEffects effect = Main.npc[i].direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

					Main.spriteBatch.Draw(headTexture, new Vector2(x, y), headTexture.Frame(), Color.White, 0f, headTexture.Frame().Size() / 2, Main.UIScale + 0.5f, effect, 0f);

					if (!Main.instance.unityMouseOver)
						Main.PlaySound(SoundID.MenuTick);

					Main.instance.unityMouseOver = true;

					// Change the tooltip to "Teleport to ..."
					mouseText = Language.GetTextValue("Game.TeleportTo", Main.npc[i].FullName);

					if (Main.mouseLeft && Main.mouseLeftRelease) {
						Main.mouseLeftRelease = false;
						Main.mapFullscreen = false;

						// TODO(1.4): Only allow player to teleport to NPC if they're not too unhappy

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

	public class PortableWormhole : ModItem {
		public override void SetDefaults() {
			item.width = 14;
			item.height = 14;
			item.maxStack = 1;
			item.consumable = false;
			item.rare = ItemRarityID.Orange;
		}

		public override bool CanUseItem(Player player) {
			return false;
		}

		public override void UpdateInventory(Player player) {
			PortableWormholePlayer modPlayer = player.GetModPlayer<PortableWormholePlayer>();
			modPlayer.hasPortableWormhole = true;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.WormholePotion, 30);
			recipe.AddTile(TileID.CrystalBall);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class PortableWormholePlayer : ModPlayer {
		public bool hasPortableWormhole = false;
		public override void ResetEffects() {
			hasPortableWormhole = false;
		}
	}
}