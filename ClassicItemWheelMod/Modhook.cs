using Staxel;
using Staxel.Rendering;
using Sunbeam;

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
        protected override void GameContextInitializeInitOverride() {

            this.DomAsset = this.AssetLoader.ReadFileContent("Assets/hotbar.min.html");
            this.ScriptAsset = this.AssetLoader.ReadFileContent("Assets/main.min.js");
            this.StyleAsset = this.AssetLoader.ReadFileContent("Assets/style.min.css");
        }

        /// <summary>
        /// Initialize through UniverseUpdateAfter
        /// Since we are overriding calls from OverlayController and this gets instantiated
        /// before ClientContextInitializeBefore and ClientCOntextInitializeInit is not called yet
        /// we have to, otherwise 
        /// </summary>
        protected override void UniverseUpdateAfterOverride() {

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
