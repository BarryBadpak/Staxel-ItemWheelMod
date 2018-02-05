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
    [HarmonyPatch(typeof(OverlayRenderer), "Update")]
    class UpdatePatch
    {
        static int _prevActiveIndex;
        static ControlHintVerbs _prevControlHints;

        [HarmonyPrefix]
        static void beforeUpdate(Universe universe, AvatarController avatarController)
        {
            int activeIndex = avatarController.ActiveItemIndex();
            if(UpdatePatch._prevActiveIndex != activeIndex)
            {
                HotBarController.NotifyActiveItemChanged(activeIndex);
                UpdatePatch._prevActiveIndex = activeIndex;
            }

            ControlHintVerbs controlHints = avatarController.GetControlHintVerbs();
            if (!UpdatePatch._prevControlHints.Equals(controlHints))
            {
                HotBarController.ChangeControlHints(controlHints);
                UpdatePatch._prevControlHints = controlHints;
            }
        }
    }
}
