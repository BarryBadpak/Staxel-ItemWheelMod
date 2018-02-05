using Harmony;
using Plukit.Base;
using Staxel;
using Staxel.Client;
using Staxel.Draw;
using Staxel.Logic;
using Staxel.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ClassicItemWheelMod.Patches.Overlayrenderer
{
    [HarmonyPatch(typeof(OverlayRenderer), "Draw")]
    class DrawPatch
    {
        static bool _showingback;
        static Vector2I _prevViewPortSize;

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

            if(startIdx != -1 && endIdx != -1)
            {
                codes[startIdx - 1].opcode = OpCodes.Nop; // Otherwise we're removing a jump target
                codes.RemoveRange(startIdx, endIdx - startIdx + 1);
            }

            return codes.AsEnumerable();
        }

        /**
         * Hooks in before Draw to check if the viewport size has changed, if so we request to rebind the slots for the hotbar
         */
        [HarmonyPrefix]
        static void beforeDraw(DeviceContext graphics, Matrix4F matrix, Entity avatar, Universe universe, AvatarController avatarController)
        {
            Vector2I viewPortSize = graphics.GetViewPortSize();
            if (viewPortSize != DrawPatch._prevViewPortSize)
            {
                DrawPatch._prevViewPortSize = viewPortSize;
                HotBarController.NotifyViewPortResized();
            }
        }

        [HarmonyPostfix]
        static void afterDraw(DeviceContext graphics, Matrix4F matrix, Entity avatar, Universe universe, AvatarController avatarController)
        {
            if(avatar != null)
            {
                // Without this ClearDepth call it won't render the item bar
                graphics.ClearDepth();

                Vector2I viewPortSize = graphics.GetViewPortSize();
                if (viewPortSize != DrawPatch._prevViewPortSize)
                {
                    DrawPatch._prevViewPortSize = viewPortSize;
                    HotBarController.NotifyViewPortResized();
                }

                if (avatar.PlayerEntityLogic.LockedInConversation || ClientContext.OverlayController.Interruptions.IsOpen() || ClientContext.OverlayController.LoadingScreen.CaptureInput() || ClientContext.OverlayController.Interruptions.CaptureInput() || ClientContext.OverlayController.ParticleEditor.CaptureInput())
                {
                    if (DrawPatch._showingback)
                    {
                        ClientContext.WebOverlayRenderer.Call("hideHotBar", null, null, null, null, null, null);
                        DrawPatch._showingback = false;
                    }
                }
                else
                {
                    if (!DrawPatch._showingback)
                    {
                        ClientContext.WebOverlayRenderer.Call("showHotBar", null, null, null, null, null, null);
                        DrawPatch._showingback = true;
                    }
                }
            }
        }
    }
}
