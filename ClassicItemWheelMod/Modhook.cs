using Plukit.Base;
using Staxel;
using Staxel.Core;
using Staxel.Items;
using Staxel.Logic;
using Staxel.Rendering;
using Staxel.Tiles;
using Sunbeam;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ClassicItemWheelMod
{
    public class Modhook: BaseMod
    {
        public override string ModIdentifier => "ClassicItemWheelMod";
        private string DomAsset { get; set; }
        private string ScriptAsset { get; set; }
        private string StyleAsset { get; set; }

        /// <summary>
        /// Load assets
        /// </summary>
        public override void GameContextInitializeInit() {

            this.DomAsset = AssetLoader.ReadFileContent("Assets/hotbar.min.html");
            this.ScriptAsset = AssetLoader.ReadFileContent("Assets/main.min.js");
            this.StyleAsset = AssetLoader.ReadFileContent("Assets/style.min.css");
        }

        /// <summary>
        /// Initialize through UniverseUpdateAfter
        /// Since we are overriding calls from OverlayController and this gets instantiated
        /// before ClientContextInitializeBefore and ClientCOntextInitializeInit is not called yet
        /// we have to, otherwise 
        /// </summary>
        public override void UniverseUpdateAfter() {

            WebOverlayRenderer overlay = ClientContext.WebOverlayRenderer;
            if (overlay != null && !HotBarController.Initialized)
            {
                overlay.CallPreparedFunction("(() => { const el = document.createElement('style'); el.type = 'text/css'; el.appendChild(document.createTextNode('" + this.StyleAsset + "')); document.head.appendChild(el); })();");
                overlay.CallPreparedFunction("$('body').append(\"" + this.DomAsset + "\");");
                overlay.CallPreparedFunction(this.ScriptAsset);

                HotBarController.Initialize();
            }
        }
    }
}
