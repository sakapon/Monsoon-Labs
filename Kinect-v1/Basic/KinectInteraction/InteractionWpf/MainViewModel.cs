using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Kinect.Toolkit.Interaction;
using Reactive.Bindings;

namespace InteractionWpf
{
    public class MainViewModel
    {
        public InteractionTracker InteractionTracker { get; } = new InteractionTracker();

        public ReadOnlyReactiveProperty<InteractionHandPointer> LeftHand { get; }
        public ReadOnlyReactiveProperty<InteractionHandPointer> RightHand { get; }

        public ReadOnlyReactiveProperty<string> LeftHandInfo { get; }
        public ReadOnlyReactiveProperty<string> RightHandInfo { get; }

        public ReadOnlyReactiveProperty<string> LeftInteractiveColor { get; }
        public ReadOnlyReactiveProperty<string> RightInteractiveColor { get; }

        public ReadOnlyReactiveProperty<bool> IsLeftGripped { get; }
        public ReadOnlyReactiveProperty<bool> IsRightGripped { get; }

        public ReadOnlyReactiveProperty<bool> IsLeftPressed { get; }
        public ReadOnlyReactiveProperty<bool> IsRightPressed { get; }

        public MainViewModel()
        {
            LeftHand = InteractionTracker.UserInfo
                .Select(u => u?.HandPointers.FirstOrDefault(h => h.HandType == InteractionHandType.Left))
                .ToReadOnlyReactiveProperty();
            RightHand = InteractionTracker.UserInfo
                .Select(u => u?.HandPointers.FirstOrDefault(h => h.HandType == InteractionHandType.Right))
                .ToReadOnlyReactiveProperty();

            LeftHandInfo = LeftHand.Select(ToString).ToReadOnlyReactiveProperty();
            RightHandInfo = RightHand.Select(ToString).ToReadOnlyReactiveProperty();

            LeftInteractiveColor = LeftHand.Select(hp => ToInteractiveColor(hp != null && hp.IsInteractive)).ToReadOnlyReactiveProperty();
            RightInteractiveColor = RightHand.Select(hp => ToInteractiveColor(hp != null && hp.IsInteractive)).ToReadOnlyReactiveProperty();

            IsLeftGripped = ToGripped(LeftHand);
            IsRightGripped = ToGripped(RightHand);

            IsLeftPressed = LeftHand.Select(ToPressed).ToReadOnlyReactiveProperty();
            IsRightPressed = RightHand.Select(ToPressed).ToReadOnlyReactiveProperty();
        }

        static string ToString(InteractionHandPointer hp)
        {
            if (hp == null) return "";

            return string.Join("\r\n", PropertyHelper.GetPropertyValues(hp)
                .Select(p => $"{p.Key}: {p.Value}"));
        }

        static string ToInteractiveColor(bool isInteractive) => isInteractive ? "#009900" : "#FF6600";

        static ReadOnlyReactiveProperty<bool> ToGripped(IObservable<InteractionHandPointer> hps) =>
            hps
                .Where(hp => hp == null || !hp.IsInteractive || hp.HandEventType != InteractionHandEventType.None)
                .Select(hp => hp != null && hp.IsInteractive && hp.HandEventType == InteractionHandEventType.Grip)
                .ToReadOnlyReactiveProperty();

        static bool ToPressed(InteractionHandPointer hp) => hp != null && hp.IsInteractive && hp.IsPressed;
    }

    public static class PropertyHelper
    {
        public static Dictionary<string, object> GetPropertyValues(object obj)
        {
            var properties = TypeDescriptor.GetProperties(obj);
            return properties
                .Cast<PropertyDescriptor>()
                .ToDictionary(p => p.Name, p => p.GetValue(obj));
        }
    }
}
