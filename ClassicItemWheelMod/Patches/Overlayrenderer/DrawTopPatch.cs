using Harmony;
using Plukit.Base;
using Staxel;
using Staxel.Draw;
using Staxel.Logic;
using Staxel.Rendering;

namespace ClassicItemWheelMod.Patches.Overlayrenderer
{
	[HarmonyPatch(typeof(OverlayRenderer), "DrawTop")]
	class DrawTopPatch
	{
		[HarmonyPostfix]
		static void afterDrawTop(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity avatar, EntityPainter avatarPainter, Universe universe, Timestep timestep)
		{
			if (avatar != null)
			{
				if (!avatar.PlayerEntityLogic.LockedInConversation && !ClientContext.OverlayController.Interruptions.IsOpen() && !ClientContext.OverlayController.LoadingScreen.CaptureInput() && !ClientContext.OverlayController.Interruptions.CaptureInput() && !ClientContext.OverlayController.ParticleEditor.CaptureInput())
				{
					// Without this ClearDepth call it won't render the item bar
					graphics.ClearDepth();
					HotbarManager.Instance.Controller.Renderer.DrawItemVoxels(graphics, matrix, renderOrigin, avatar, avatarPainter, universe);
				}
			}
		}
	}
}
