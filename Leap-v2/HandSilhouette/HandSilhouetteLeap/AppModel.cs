using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Leap;
using Reactive.Bindings;

namespace HandSilhouetteLeap
{
    public class AppModel
    {
        static readonly Hand[] EmptyHands = new Hand[0];
        static readonly InclinableMapper InclinableMapper = new InclinableMapper
        {
            VirtualScreenCenter = new Vector3D(700, 400, 0),
            AngleDegrees = 40,
            PixelsPerMillimeter = 3,
        };

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
            GetHandPoints0(h)
                .Select(LeapHelper.ToPoint3D)
                .Select(InclinableMapper.ToScreenPosition)
                .Select(VectorHelper.ToPoint)
                .ToArray();

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
