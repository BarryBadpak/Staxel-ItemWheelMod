using Staxel.Browser;
using Sunbeam;

namespace ClassicItemWheelMod
{
	public class HotbarManager : SunbeamMod
	{
		public override string ModIdentifier => "ClassicItemWheelMod";
		public static HotbarManager Instance { get; private set; }

		public HotBarController Controller;

		private string HTMLAsset { get; set; }
		private string JSAsset { get; set; }
		private string CSSAsset { get; set; }

		public HotbarManager()
		{
			HotbarManager.Instance = this;

			this.HTMLAsset = this.AssetLoader.ReadFileContent("Assets/hotbar.min.html");
			this.JSAsset = this.AssetLoader.ReadFileContent("Assets/main.min.js");
			this.CSSAsset = this.AssetLoader.ReadFileContent("Assets/style.min.css");
		}

		/// <summary>
		/// We can only instantiate the controller after the ClientContext is initialised
		/// otherwise the WeboverlayRenderer is not available
		/// </summary>
		public override void ClientContextInitializeBefore()
		{
			this.Controller = new HotBarController();
		}

		/// <summary>
		/// Inject the UI contents
		/// </summary>
		public override void IngameOverlayUILoaded(BrowserRenderSurface surface)
		{
			surface.CallPreparedFunction("(() => { const el = document.createElement('style'); el.type = 'text/css'; el.appendChild(document.createTextNode('" + this.CSSAsset + "')); document.head.appendChild(el); })();");
			surface.CallPreparedFunction("$('body').append(\"" + this.HTMLAsset + "\");");
			surface.CallPreparedFunction(this.JSAsset);

			this.Controller.Reset();
		}
	}
}
