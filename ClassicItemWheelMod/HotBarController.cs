using Plukit.Base;
using Staxel;
using Staxel.Collections;
using Staxel.Input;
using Staxel.Logic;
using Staxel.Rendering;
using System;
using System.Collections.Generic;

namespace ClassicItemWheelMod
{
    static class HotBarController
    {
        public static bool Initialized = false;
        public static bool Active = false;
        private static readonly Dictionary<Quad<string, string, string, string>, string> ControlHintJsonCache = new Dictionary<Quad<string, string, string, string>, string>();

        public static HotBarRenderer Renderer
        {
            get;
            private set;
        }

        public static Vector2F[] Layout;
        public static Vector2F[] LayoutOrigin;
        public static Vector2F[] LayoutSizes;

        /// <summary>
        /// Initialize the HotBarController
        /// </summary>
        public static void Initialize()
        {
            HotBarController.Renderer = new HotBarRenderer();
            HotBarController.Bind(); 
            HotBarController.Initialized = true;
        }

        /// <summary>
        /// Bind events to the WebOverlayRenderer
        /// </summary>
        public static void Bind()
        {
            WebOverlayRenderer overlay = ClientContext.WebOverlayRenderer;
            overlay.Bind("hotbarBindSlots", HotBarController.BindSlots);
            overlay.Bind("hotbarAfterShow", HotBarController.OnShow);
            overlay.Bind("hotbarAfterHide", HotBarController.OnHide);
        }

        public static void ChangeControlHints(ControlHintVerbs controlHints)
        {
            bool hasMain = !string.IsNullOrEmpty(controlHints.VerbMain);
            bool hasAlt = !string.IsNullOrEmpty(controlHints.VerbAlt);
            string hintMain = "";
            string hintAlt = "";

            ControlHintContext context = default(ControlHintContext);
            if (hasMain)
            {
                if (controlHints.Rotate)
                {
                    if (ClientContext.InputSource.TryGetControlHintContext(GameLogicalButton.Next, "staxel.controlHint.itemWheel.Rotate", out context))
                    {
                        hintMain = context.Class;
                    }
                    else if (ClientContext.InputSource.TryGetControlHintContext(GameLogicalButton.Previous, "staxel.controlHint.itemWheel.Rotate", out context))
                    {
                        hintMain = context.Class;
                    }
                }
                else if (ClientContext.InputSource.TryGetControlHintContext(GameLogicalButton.Main, "staxel.controlHint.itemWheel.Main", out context))
                {
                    hintMain = context.Class;
                }
            }

            if (hasAlt && ClientContext.InputSource.TryGetControlHintContext(GameLogicalButton.Alt, "staxel.controlHint.itemWheel.Alt", out context))
            {
                hintAlt = context.Class;
            }

            string verbMain = hasMain ? ClientContext.LanguageDatabase.GetTranslationString(controlHints.VerbMain) : "";
            string verbAlt = hasAlt ? ClientContext.LanguageDatabase.GetTranslationString(controlHints.VerbAlt) : "";

            Quad<string, string, string, string> key = new Quad<string, string, string, string>(hintMain, hintAlt, verbMain, verbAlt);
            string commandString = default(string);

            if (!HotBarController.ControlHintJsonCache.TryGetValue(key, out commandString))
            {
                Blob blob = BlobAllocator.Blob(true);
                blob.SetString("hintMain", hintMain);
                blob.SetString("hintAlt", hintAlt);
                blob.SetString("verbMain", verbMain);
                blob.SetString("verbAlt", verbAlt);

                commandString = ClientContext.WebOverlayRenderer.PrepareCallFunction("updateControlHint", blob.ToString());

                HotBarController.ControlHintJsonCache.Add(key, commandString);
                Blob.Deallocate(ref blob);
            }

            ClientContext.WebOverlayRenderer.CallPreparedFunction(commandString);
        }

        /// <summary>
        /// Triggered by the beforeDraw patch, we call a function on the WebOverlayRenderer to resupply us with 
        /// the Layout information for the hotbar
        /// </summary>
        public static void NotifyViewPortResized()
        {
            ClientContext.WebOverlayRenderer.Call("rebindHotbar", null, null, null, null, null, null);
        }

        /// <summary>
        /// Triggered whenever the ActiveItem on the avatarController changes
        /// </summary>
        public static void NotifyActiveItemChanged(int index)
        {
            ClientContext.WebOverlayRenderer.Call("setActiveHotBarItem", index.ToString(), null, null, null, null, null);
        }

        /// <summary>
        /// Returns the HotBarCell position based on the index
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Vector2F GetSlotPosition(int i)
        {
            return HotBarController.Layout[i];
        }

        /// <summary>
        /// Reads the JSON provided by the UI from the WebOverlayRenderer
        /// and parses the Layout information
        /// </summary>
        /// <param name="arg"></param>
        private static void BindSlots(string arg)
        {
            Blob blob = BlobAllocator.Blob(true);
            blob.ReadJson(arg);
            List<BlobEntry> layout = blob.FetchList("layout");
            HotBarController.Layout = new Vector2F[layout.Count];

            for (int k = 0; k < layout.Count; k++)
            {
                Blob entry = layout[k].Blob();
                Logger.WriteLine("Layout-"+k.ToString());
                Logger.WriteLine(entry.GetLong("left").ToString() + "," + entry.GetLong("top").ToString());
                HotBarController.Layout[k] = new Vector2F((float)entry.GetLong("left"), (float)entry.GetLong("top"));
            }

            List<BlobEntry> layoutOrigins = blob.FetchList("layoutOrigins");
            HotBarController.LayoutOrigin = new Vector2F[layoutOrigins.Count];
            for (int j = 0; j < layoutOrigins.Count; j++)
            {
                Blob entry2 = layoutOrigins[j].Blob();
                HotBarController.LayoutOrigin[j] = new Vector2F((float)entry2.GetLong("left"), (float)entry2.GetLong("top"));
            }

            List<BlobEntry> layoutSizes = blob.FetchList("layoutSizes");
            HotBarController.LayoutSizes = new Vector2F[layoutSizes.Count];
            for (int i = 0; i < layoutSizes.Count; i++)
            {
                Blob entry3 = layoutSizes[i].Blob();
                HotBarController.LayoutSizes[i] = new Vector2F((float)entry3.GetLong("width"), (float)entry3.GetLong("height"));
            }

            Blob.Deallocate(ref blob);
        }

        /// <summary>
        /// Is the layout present?
        /// </summary>
        /// <returns></returns>
        public static bool IsLayoutActive()
        {
            if (HotBarController.Layout != null)
            {
                if (HotBarController.Layout.Length <= 0)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Is the HotBar active? (Visible)
        /// </summary>
        /// <returns></returns>
        public static bool IsActive()
        {
            return HotBarController.Active;
        }

        /// <summary>
        /// Clear out Layout information
        /// </summary>
        public static void ClearLayout()
        {
            HotBarController.Layout = null;
            HotBarController.LayoutOrigin = null;
            HotBarController.LayoutSizes = null;
        }

        private static void OnHide(string obj)
        {
            HotBarController.Active = false;
        }

        private static void OnShow(string obj)
        {
            HotBarController.Active = true;
        }
    }
}
