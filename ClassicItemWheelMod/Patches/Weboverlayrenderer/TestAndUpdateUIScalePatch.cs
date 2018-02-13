using Harmony;
using Staxel;
using Staxel.Browser;
using Staxel.Client;
using Staxel.Logic;
using Staxel.Rendering;


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
