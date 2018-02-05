using Plukit.Base;
using Staxel;
using Staxel.Client;
using Staxel.Core;
using Staxel.Draw;
using Staxel.Items;
using Staxel.Logic;
using Staxel.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClassicItemWheelMod
{
    class HotBarRenderer
    {
        public void drawItemVoxels(DeviceContext graphics, Matrix4F matrix, Vector3D renderOrigin, Entity avatar, EntityPainter avatarPainter, Universe universe)
        {
            if(!HotBarController.IsLayoutActive())
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
                    Vector2F p = HotBarController.GetSlotPosition(i);
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
            graphics.ApplyRenderState();

            
            TextRenderer renderer = new TextRenderer(false);
            for (int i = 0; i < 10; i++)
            {
                int count = default(int);
                Item curItem = avatar.Inventory.GetHotbarItem(i, out count);
                if (curItem != Item.NullItem && count > 1)
                {
                    Vector2F p = HotBarController.GetSlotPosition(i);
                    p *= Constants.UIZoomFactor;

                    renderer.DrawInteger(count, p + new Vector2F(Constants.ItemCountOffsetX, Constants.ItemCountOffsetY) * Constants.UIZoomFactor);
                }
            }

            renderer.Draw(graphics);
            graphics.ClearDepth();
        }
    }
}
