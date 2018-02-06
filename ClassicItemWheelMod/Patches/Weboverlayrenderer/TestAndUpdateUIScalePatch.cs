using Harmony;
using Staxel;
using Staxel.Client;
using Staxel.Logic;
using Staxel.Rendering;


namespace ClassicItemWheelMod.Patches.Weboverlayrenderer
{
    [HarmonyPatch(typeof(WebOverlayRenderer), "TestAndUpdateUIScale")]
    class TestAndUpdateUIScalePatch
    {
        [HarmonyPostfix]
        static void afterTestAndUpdateUIScale()
        {
            ClientContext.WebOverlayRenderer.Call("rebindHotbar", null, null, null, null, null, null);
        }
    }
}
