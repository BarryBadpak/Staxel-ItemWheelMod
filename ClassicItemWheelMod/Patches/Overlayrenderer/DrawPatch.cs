using Harmony;
using Plukit.Base;
using Staxel.Client;
using Staxel.Draw;
using Staxel.Logic;
using Staxel.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ClassicItemWheelMod.Patches.Overlayrenderer
{
	[HarmonyPatch(typeof(OverlayRenderer), "Draw")]
	class DrawPatch
	{
		static Vector2I _prevViewPortSize;

		/// <summary>
		/// Removes the render call from the staxel code
		/// </summary>
		/// <param name="instructions"></param>
		/// <returns></returns>
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> TranspileRenderWheelItemVoxels(IEnumerable<CodeInstruction> instructions)
		{
			int startIdx = -1;
			int endIdx = -1;
			string targetOperand = "Void Draw(Staxel.Draw.DeviceContext, Plukit.Base.Matrix4F, Staxel.Logic.Entity, Staxel.Client.AvatarController)";
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand.ToString() == "Staxel.Rendering.ItemWheelRenderer _itemWheel")
				{
					startIdx = i;
					continue;
				}

				if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == targetOperand)
				{
					endIdx = i;
					break;
				}
			}

			if (startIdx != -1 && endIdx != -1)
			{
				codes[startIdx - 1].opcode = OpCodes.Nop; // Otherwise we're removing a jump target
				codes.RemoveRange(startIdx, endIdx - startIdx + 1);
			}

			return codes.AsEnumerable();
		}

		/// <summary>
		/// Checks for viewport size changes
		/// </summary>
		/// <param name="graphics"></param>
		[HarmonyPrefix]
		static void beforeDraw(DeviceContext graphics)
		{
			Vector2I viewPortSize = graphics.GetViewPortSize();
			if (viewPortSize != DrawPatch._prevViewPortSize)
			{
				DrawPatch._prevViewPortSize = viewPortSize;
				HotbarManager.Instance.Controller.NotifyViewPortResized();
			}
		}

		/// <summary>
		/// After draw call our HotbarRenderer
		/// </summary>
		/// <param name="avatar"></param>
		[HarmonyPostfix]
		static void afterDraw(Entity avatar)
		{
			HotbarManager.Instance.Controller.Renderer.Draw(avatar);
		}
	}
}
