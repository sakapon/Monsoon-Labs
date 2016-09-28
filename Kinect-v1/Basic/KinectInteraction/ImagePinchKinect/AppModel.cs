using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media.Media3D;
using Microsoft.Kinect.Toolkit.Interaction;
using Reactive.Bindings;

namespace ImagePinchKinect
{
    public class AppModel
    {
        public InteractionTracker InteractionTracker { get; } = new InteractionTracker();

        public ReadOnlyReactiveProperty<InteractionHandPointer> PrimaryHand { get; }
        public ReadOnlyReactiveProperty<bool> IsTracked { get; }
        public ReadOnlyReactiveProperty<bool> IsInteractive { get; }
        public ReadOnlyReactiveProperty<bool> IsGripped { get; }

        public IObservable<IObservable<Vector3D>> GripDrag { get; }
        public ReactiveProperty<Vector3D> DraggedDelta { get; } = new ReactiveProperty<Vector3D>(new Vector3D());

        public AppModel()
        {
            PrimaryHand = InteractionTracker.UserInfo
                .Select(ToPrimaryHand)
                .ToReadOnlyReactiveProperty();
            IsTracked = PrimaryHand
                .Select(h => h?.IsTracked == true)
                .ToReadOnlyReactiveProperty();
            IsInteractive = PrimaryHand
                .Select(h => h?.IsInteractive == true)
                .ToReadOnlyReactiveProperty();
            IsGripped = PrimaryHand
                .Where(h => h == null || h.HandEventType != InteractionHandEventType.None)
                .Select(h => h != null && h.HandEventType == InteractionHandEventType.Grip)
                .ToReadOnlyReactiveProperty();

            // ドラッグは IsInteractive のときに開始します。
            // ドラッグは IsTracked である間は継続します。
            GripDrag = IsGripped
                .Where(b => b && IsInteractive.Value)
                .Select(_ => ToPoint3D(PrimaryHand.Value))
                .Select(p0 => PrimaryHand
                    .TakeWhile(_ => IsGripped.Value && IsTracked.Value)
                    .Select(_ => ToPoint3D(PrimaryHand.Value) - p0));
            GripDrag
                .Select(d => new { v0 = DraggedDelta.Value, d })
                .Subscribe(_ => _.d.Subscribe(v => DraggedDelta.Value = _.v0 + v));
        }

        InteractionHandPointer ToPrimaryHand(UserInfo u)
        {
            if (u == null) return null;

            if (PrimaryHand.Value == null)
                return u.HandPointers
                    .FirstOrDefault(h => h.IsPrimaryForUser);
            else
                return u.HandPointers
                    .Where(h => h.HandType == PrimaryHand.Value.HandType)
                    .FirstOrDefault(h => h.IsPrimaryForUser);
        }

        static Point3D ToPoint3D(InteractionHandPointer h) => new Point3D(h.RawX, h.RawY, h.RawZ);
    }
}
