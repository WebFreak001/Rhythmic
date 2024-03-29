﻿using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;
using Rhythmic.Graphics.Colors;

namespace Rhythmic.Screens.Select.Carousel
{
    public abstract class DrawableCarouselItem : Container
    {
        public const float MAX_HEIGHT = 100;

        public override bool RemoveWhenNotAlive => false;

        public override bool IsPresent => base.IsPresent || Item.Visible;

        public readonly CarouselItem Item;

        private Container nestedContainer;
        private Container borderContainer;

        private Box hoverLayer;

        protected override Container<Drawable> Content => nestedContainer;

        protected DrawableCarouselItem(CarouselItem item)
        {
            Item = item;

            Height = MAX_HEIGHT;
            RelativeSizeAxes = Axes.X;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = borderContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10,
                BorderColour = new Color4(221, 255, 255, 255),
                Children = new Drawable[]
                {
                    nestedContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    hoverLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Blending = BlendingMode.Additive,
                    },
                }
            };

            hoverLayer.Colour = RhythmicColors.Blue.Opacity(0.1f);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverLayer.FadeIn(100, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverLayer.FadeOut(1000, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        public void SetMultiplicativeAlpha(float alpha) => borderContainer.Alpha = alpha;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ApplyState();
            Item.Filtered.ValueChanged += _ => Schedule(ApplyState);
            Item.State.ValueChanged += _ => Schedule(ApplyState);
        }

        protected virtual void ApplyState()
        {
            if (!IsLoaded) return;

            switch (Item.State.Value)
            {
                case CarouselItemState.NotSelected:
                    Deselected();
                    break;

                case CarouselItemState.Selected:
                    Selected();
                    break;
            }

            if (!Item.Visible)
                this.FadeOut(300, Easing.OutQuint);
            else
                this.FadeIn(250);
        }

        protected virtual void Selected()
        {
            Item.State.Value = CarouselItemState.Selected;

            borderContainer.BorderThickness = 2.5f;
            borderContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = new Color4(130, 204, 255, 150),
                Radius = 20,
                Roundness = 10,
            };
        }

        protected virtual void Deselected()
        {
            Item.State.Value = CarouselItemState.NotSelected;

            borderContainer.BorderThickness = 0;
            borderContainer.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(1),
                Radius = 10,
                Colour = Color4.Black.Opacity(100),
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            Item.State.Value = CarouselItemState.Selected;
            return true;
        }
    }
}
