﻿using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using Rhythmic.Beatmap;
using osu.Framework.Graphics.Containers;
using Rhythmic.Overlays;
using Rhythmic.Beatmap.Drawables;

namespace Rhythmic.Screens.Play
{
    public class Play : RhythmicScreen
    {
        private FailOverlay failOverlay;

        public override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;

        public override bool HideOverlaysOnEnter => true;

        public override float BackgroundParallaxAmount => 0f;

        [Resolved]
        private BeatmapCollection collection { get; set; }

        protected override void LoadComplete()
        {
            PlayableContainer container;

            AddInternal(container = new PlayableContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddInternal(failOverlay = new FailOverlay
            {
                Depth = -100,
                RelativeSizeAxes = Axes.Both,
                OnQuit = () => { this.Exit(); container.Expire(); collection.CurrentBeatmap.Value.Song.Restart(); },
                OnRetry = () =>
                {
                    container.Clear();

                    AddInternal(container = new PlayableContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Depth = 1
                    });

                    container.OnLoadComplete += delegate
                    {
                        container.player.Health.Value = collection.CurrentBeatmap.Value.Player.Health;
                        collection.CurrentBeatmap.Value.Song.Restart();
                        container.player.OnDeath = () =>
                        {
                            container.Stop();
                            failOverlay.ToggleVisibility();
                            collection.CurrentBeatmap.Value.Song.Stop();
                        };
                    };

                    failOverlay.Retries++;
                }
            });

            container.OnLoadComplete += delegate
            {
                collection.CurrentBeatmap.Value.Song.Restart();
                container.player.OnDeath = () =>
                {
                    container.Stop();
                    failOverlay.ToggleVisibility();
                    collection.CurrentBeatmap.Value.Song.Stop();
                };
            };

            base.LoadComplete();
        }

        private class PlayableContainer : Container
        {
            [Resolved]
            private BeatmapCollection collection { get; set; }

            public Player player;

            protected override void LoadComplete()
            {
                Add(player = new Player
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                });

                foreach (var obj in collection.CurrentBeatmap.Value.Level.Level)
                {
                    Scheduler.AddDelayed(() =>
                    {
                        Add(new DrawableBeatmapObject(obj, player));
                    }, obj.Time);
                }

                base.LoadComplete();
            }

            public void Stop()
            {
                Scheduler.CancelDelayedTasks();
            }
        }
    }
}
