using Plukit.Base;
using Staxel;
using Staxel.Client;
using Staxel.Logic;
using Staxel.Rendering;
using System.Collections.Generic;

namespace ClassicItemWheelMod
{
	public class HotBarController
	{
		public HotBarRenderer Renderer { get; private set; }

		private Vector2F[] Layout;
		private Vector2F[] LayoutOrigin;
		private Vector2F[] LayoutSizes;
		private int _prevActiveIndex;

		public bool IsActive = false;

		/// <summary>
		/// Construct a new HotBarController
		/// </summary>
		public HotBarController()
		{
			this.Bind();
			this.Renderer = new HotBarRenderer();
		}

		/// <summary>
		/// Bind events to the WebOverlayRenderer
		/// </summary>
		private void Bind()
		{
			WebOverlayRenderer overlay = ClientContext.WebOverlayRenderer;
			overlay.Bind("hotbarBindSlots", this.BindSlots);
			overlay.Bind("hotbarAfterShow", this.OnShow);
			overlay.Bind("hotbarAfterHide", this.OnHide);
		}

		/// <summary>
		/// Called on update
		/// </summary>
		/// <param name="universe"></param>
		/// <param name="avatarController"></param>
		public void Update(Universe universe, AvatarController avatarController)
		{
			int activeIndex = avatarController.ActiveItemIndex();
			if (this._prevActiveIndex != activeIndex)
			{
				this.NotifyActiveItemChanged(activeIndex);
				this._prevActiveIndex = activeIndex;
			}
		}

		/// <summary>
		/// Triggered by the beforeDraw patch, we call a function on the WebOverlayRenderer to resupply us with 
		/// the Layout information for the hotbar
		/// </summary>
		public void NotifyViewPortResized()
		{
			ClientContext.WebOverlayRenderer.Call("rebindHotbar", null, null, null, null, null, null);
		}

		/// <summary>
		/// Triggered whenever the ActiveItem on the avatarController changes
		/// </summary>
		public void NotifyActiveItemChanged(int index)
		{
			ClientContext.WebOverlayRenderer.Call("setActiveHotBarItem", index.ToString(), null, null, null, null, null);
		}

		/// <summary>
		/// Returns the HotBarCell position based on the index
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Vector2F GetSlotPosition(int i)
		{
			return this.Layout[i];
		}

		/// <summary>
		/// Reads the JSON provided by the UI from the WebOverlayRenderer
		/// and parses the Layout information
		/// </summary>
		/// <param name="arg"></param>
		private void BindSlots(string arg)
		{
			Blob blob = BlobAllocator.Blob(true);
			blob.ReadJson(arg);
			List<BlobEntry> layout = blob.FetchList("layout");
			this.Layout = new Vector2F[layout.Count];

			for (int k = 0; k < layout.Count; k++)
			{
				Blob entry = layout[k].Blob();
				this.Layout[k] = new Vector2F((float)entry.GetLong("left"), (float)entry.GetLong("top"));
			}

			List<BlobEntry> layoutOrigins = blob.FetchList("layoutOrigins");
			this.LayoutOrigin = new Vector2F[layoutOrigins.Count];
			for (int j = 0; j < layoutOrigins.Count; j++)
			{
				Blob entry2 = layoutOrigins[j].Blob();
				this.LayoutOrigin[j] = new Vector2F((float)entry2.GetLong("left"), (float)entry2.GetLong("top"));
			}

			List<BlobEntry> layoutSizes = blob.FetchList("layoutSizes");
			this.LayoutSizes = new Vector2F[layoutSizes.Count];
			for (int i = 0; i < layoutSizes.Count; i++)
			{
				Blob entry3 = layoutSizes[i].Blob();
				this.LayoutSizes[i] = new Vector2F((float)entry3.GetLong("width"), (float)entry3.GetLong("height"));
			}

			Blob.Deallocate(ref blob);
		}

		/// <summary>
		/// Is the layout present?
		/// </summary>
		/// <returns></returns>
		public bool IsLayoutActive()
		{
			if (this.Layout != null)
			{
				if (this.Layout.Length <= 0)
				{
					return false;
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Clear out Layout information
		/// </summary>
		public void ClearLayout()
		{
			this.Layout = null;
			this.LayoutOrigin = null;
			this.LayoutSizes = null;
		}

		/// <summary>
		/// Reset the hotbar
		/// </summary>
		public void Reset()
		{
			this.Renderer.Reset();
		}

		/// <summary>
		/// On hide of the item hotbar
		/// </summary>
		/// <param name="obj"></param>
		private void OnHide(string obj)
		{
			this.IsActive = false;
		}

		/// <summary>
		/// On show of the item hotbar
		/// </summary>
		/// <param name="obj"></param>
		private void OnShow(string obj)
		{
			this.IsActive = true;
		}
	}
}
