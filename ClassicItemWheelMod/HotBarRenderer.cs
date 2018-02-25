using Plukit.Base;
using Staxel;
using Staxel.Client;
using Staxel.Collections;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Input;
using Staxel.Items;
using Staxel.Logic;
using Staxel.Rendering;
using System;
using System.Collections.Generic;

namespace ClassicItemWheelMod
{
	public class HotBarRenderer
	{
		public bool IsShowing = false;

		private TextRenderer TextRenderer = new TextRenderer(false);
		private readonly Dictionary<Quad<string, string, string, string>, string> ControlHintJsonCache = new Dictionary<Quad<string, string, string, string>, string>();
		private ControlHintVerbs _prevControlHints;

		/// <summary>
		/// Called on update
		/// </summary>
		/// <param name="universe"></param>
		/// <param name="avatarController"></param>
		public void Update(Universe universe, AvatarController avatarController)
		{
			ControlHintVerbs controlHints = avatarController.GetControlHintVerbs();
			if (!this._prevControlHints.Equals(controlHints))
			{
				this.ChangeControlHints(controlHints);
				this._prevControlHints = controlHints;
			}
		}

		/// <summary>
		/// Render the item icons
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="matrix"></param>
		/// <param name="renderOrigin"></param>
		/// <param name="avatar"></param>
		/// <param name="avatarPainter"></param>
		/// <param name="universe"></param>
		public void DrawItemVoxels(DeviceContext graphics, Matrix4F matrix, Entity avatar)
		{
			if (!this.IsShowing)
			{
				return;
			}

			graphics.PushRenderState();
			Matrix4F projectionMatrix = graphics.GetProjectionMatrix();
			Matrix4F overlayMatrix = graphics.GetOverlayMatrix();
			graphics.PushShader();
			graphics.SetShader(graphics.GetShader("VoxelOverlayStipple"));

			double rotation = ClientContext.OverlayController.IsMenuOpen() ? 0 : (DateTime.UtcNow - new DateTime(0L)).TotalMilliseconds * 0.001 % 6.2831853071795862;
			int activeIndex = avatar.Inventory.ActiveItemIndex();

			for (int i = 0; i < 10; i++)
			{

				ItemStack item = avatar.Inventory.GetHotbarItem(i);
				if (item.Item != Item.NullItem)
				{
					Vector2F p = HotbarManager.Instance.Controller.GetSlotPosition(i);
					p *= Constants.UIZoomFactor;

					Vector2F pp = graphics.ScreenPosToProjectionPos(p);
					Matrix4F movedMatrix2 = Matrix4F.CreateTranslation(new Vector3F(pp.X, pp.Y, 0f));
					movedMatrix2 = Matrix4F.Multiply(overlayMatrix, movedMatrix2);
					graphics.SetProjectionMatrix(movedMatrix2);

					Vector2I viewPortSize = graphics.GetViewPortSize();
					float itemScale = Constants.ItemRenderingScale;
					itemScale *= Constants.UIZoomFactor / ((float)viewPortSize.Y / Constants.ViewPortScaleThreshold.Y);

					if (i == activeIndex)
					{
						graphics.SetShader(graphics.GetShader("VoxelOverlay"));
					}

					ClientContext.ItemRendererManager.RenderIcon(item.Item, graphics, Matrix4F.CreateScale(itemScale).Rotate((float)rotation, Vector3F.Up).Rotate(-0.35f, Vector3F.Left)
						.Translate(new Vector3F(0f, 0f, -0.2f))
						.Multiply(matrix));

					if (i == activeIndex)
					{
						graphics.SetShader(graphics.GetShader("VoxelOverlayStipple"));
					}
				}
			}

			graphics.PopShader();
			graphics.SetProjectionMatrix(projectionMatrix);
			graphics.PopRenderState();
		}

		/// <summary>
		/// Render the item hotbar counts
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="avatar"></param>
		public void DrawItemCounts(DeviceContext graphics, Entity avatar)
		{
			for (int i = 0; i < 10; i++)
			{
				int count = default(int);
				Item curItem = avatar.Inventory.GetHotbarItem(i, out count);
				if (curItem != Item.NullItem && count > 1)
				{
					Vector2F p = HotbarManager.Instance.Controller.GetSlotPosition(i);
					p *= Constants.UIZoomFactor;

					this.TextRenderer.DrawInteger(count, p + new Vector2F(Constants.ItemCountOffsetX, Constants.ItemCountOffsetY) * Constants.UIZoomFactor);
				}
			}

			this.TextRenderer.Draw(graphics);
		}

		/// <summary>
		/// Draw
		/// </summary>
		/// <param name="avatar"></param>
		public void Draw(Entity avatar)
		{
			if (avatar != null)
			{
				if (avatar.PlayerEntityLogic.LockedInConversation || ClientContext.OverlayController.Interruptions.IsOpen() || ClientContext.OverlayController.LoadingScreen.CaptureInput() || ClientContext.OverlayController.Interruptions.CaptureInput() || ClientContext.OverlayController.ParticleEditor.CaptureInput())
				{
					if (this.IsShowing)
					{
						ClientContext.WebOverlayRenderer.Call("hideHotBar", null, null, null, null, null, null);
						this.IsShowing = false;
					}
				}
				else
				{
					if (!this.IsShowing)
					{
						ClientContext.WebOverlayRenderer.Call("showHotBar", null, null, null, null, null, null);
						this.IsShowing = true;
					}
				}
			}
		}

		/// <summary>
		/// Pass the changed control hints to the UI
		/// </summary>
		/// <param name="controlHints"></param>
		public void ChangeControlHints(ControlHintVerbs controlHints)
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

			if (!this.ControlHintJsonCache.TryGetValue(key, out commandString))
			{
				Blob blob = BlobAllocator.Blob(true);
				blob.SetString("hintMain", hintMain);
				blob.SetString("hintAlt", hintAlt);
				blob.SetString("verbMain", verbMain);
				blob.SetString("verbAlt", verbAlt);

				commandString = ClientContext.WebOverlayRenderer.PrepareCallFunction("updateControlHint", blob.ToString());

				this.ControlHintJsonCache.Add(key, commandString);
				Blob.Deallocate(ref blob);
			}

			ClientContext.WebOverlayRenderer.CallPreparedFunction(commandString);
		}

		/// <summary>
		/// Reset the hotbar
		/// </summary>
		public void Reset()
		{
			this.IsShowing = false;
		}
	}
}
