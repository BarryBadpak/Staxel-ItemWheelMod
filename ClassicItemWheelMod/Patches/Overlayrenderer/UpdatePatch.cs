using Harmony;
using Staxel.Client;
using Staxel.Logic;
using Staxel.Rendering;

namespace ClassicItemWheelMod.Patches.Overlayrenderer
{
	[HarmonyPatch(typeof(OverlayRenderer), "Update")]
	class UpdatePatch
	{
		[HarmonyPrefix]
		static void beforeUpdate(Universe universe, AvatarController avatarController)
		{
			HotbarManager.Instance.Controller.Renderer.Update(universe, avatarController);
			HotbarManager.Instance.Controller.Update(universe, avatarController);
		}
	}
}
