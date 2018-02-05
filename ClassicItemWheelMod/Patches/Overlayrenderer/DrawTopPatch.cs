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
    [HarmonyPatch(typeof(OverlayRenderer), "DrawTop")]
    class DrawTopPatch
    {
        [HarmonyPostfix]
        static void afterDrawTop(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity avatar, EntityPainter avatarPainter, Universe universe, Timestep timestep)
        {
            if(avatar != null)
            {
                if (!avatar.PlayerEntityLogic.LockedInConversation && !ClientContext.OverlayController.Interruptions.IsOpen() && !ClientContext.OverlayController.LoadingScreen.CaptureInput() && !ClientContext.OverlayController.Interruptions.CaptureInput() && !ClientContext.OverlayController.ParticleEditor.CaptureInput())
                {
                    // Without this ClearDepth call it won't render the item bar
                    graphics.ClearDepth();

                    HotBarController.Renderer.drawItemVoxels(graphics, matrix, renderOrigin, avatar, avatarPainter, universe);
                }
            }
        }
    }
}
