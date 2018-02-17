using Harmony;
using Staxel;
using Staxel.Browser;

namespace ClassicItemWheelMod.Patches.Weboverlayrenderer
{
	[HarmonyPatch(typeof(BrowserRenderSurface), "TestAndUpdateUIScale")]
	class TestAndUpdateUIScalePatch
	{
		[HarmonyPostfix]
		static void afterTestAndUpdateUIScale()
		{
			ClientContext.WebOverlayRenderer.Call("rebindHotbar", null, null, null, null, null, null);
		}
	}
}
