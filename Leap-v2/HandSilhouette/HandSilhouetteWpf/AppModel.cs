﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Leap;
using Reactive.Bindings;

namespace HandSilhouetteWpf
{
    public class AppModel
    {
        const double ScreenWidth = 1920.0;
        const double ScreenHeight = 1080.0;
        const double MappingScale = 3.0;

        static readonly Hand[] EmptyHands = new Hand[0];

        public Controller Controller { get; } = new Controller();

        public ReadOnlyReactiveProperty<Hand[]> Hands { get; }
        public ReadOnlyReactiveProperty<PointCollection[]> HandsPoints { get; }

        public AppModel()
        {
            Controller.SetPolicy(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);

            Hands = Observable.Interval(TimeSpan.FromSeconds(1 / 60.0))
                .Select(_ =>
                {
                    using (var f = Controller.Frame())
                    {
                        return !f.Hands.IsEmpty ? f.Hands.ToArray() : EmptyHands;
                    }
                })
                .ToReadOnlyReactiveProperty(EmptyHands);

            HandsPoints = Hands
                .Select(hs => hs.Select(GetHandPoints).ToArray())
                .ObserveOn(SynchronizationContext.Current)
                .Select(pss => pss.Select(ps => new PointCollection(ps)).ToArray())
                .ToReadOnlyReactiveProperty();
        }

        static Point[] GetHandPoints(Hand h) =>
            GetHandPoints0(h).Select(ToScreenPoint).ToArray();

        static Leap.Vector[] GetHandPoints0(Hand h)
        {
            var thumb = h.Fingers.GetFinger(Finger.FingerType.TYPE_THUMB);
            var index = h.Fingers.GetFinger(Finger.FingerType.TYPE_INDEX);
            var middle = h.Fingers.GetFinger(Finger.FingerType.TYPE_MIDDLE);
            var ring = h.Fingers.GetFinger(Finger.FingerType.TYPE_RING);
            var pinky = h.Fingers.GetFinger(Finger.FingerType.TYPE_PINKY);

            return new[]
            {
                h.WristPosition,
                thumb.GetJoint(1), thumb.GetJoint(2), thumb.GetJoint(3), thumb.GetJoint(4), thumb.GetJoint(3), thumb.GetJoint(2),
                index.GetJoint(1), index.GetJoint(2), index.GetJoint(3), index.GetJoint(4), index.GetJoint(3), index.GetJoint(2), index.GetJoint(1),
                middle.GetJoint(1), middle.GetJoint(2), middle.GetJoint(3), middle.GetJoint(4), middle.GetJoint(3), middle.GetJoint(2), middle.GetJoint(1),
                ring.GetJoint(1), ring.GetJoint(2), ring.GetJoint(3), ring.GetJoint(4), ring.GetJoint(3), ring.GetJoint(2), ring.GetJoint(1),
                pinky.GetJoint(1), pinky.GetJoint(2), pinky.GetJoint(3), pinky.GetJoint(4), pinky.GetJoint(3), pinky.GetJoint(2), pinky.GetJoint(1), pinky.GetJoint(0),
            };
        }

        static Point ToScreenPoint(Leap.Vector v) =>
            new Point(ScreenWidth / 2 + MappingScale * v.x, ScreenHeight - MappingScale * v.y);
    }

    public static class HandHelper
    {
        public static Finger GetFinger(this FingerList fingers, Finger.FingerType type) =>
            fingers.FingerType(type).FirstOrDefault();

        public static Leap.Vector GetJoint(this Finger finger, int index)
        {
            if (index < 4)
            {
                var bone = finger.Bone((Bone.BoneType)index);
                return bone.PrevJoint;
            }
            else
            {
                var bone = finger.Bone(Bone.BoneType.TYPE_DISTAL);
                return bone.NextJoint;
            }
        }
    }
}
