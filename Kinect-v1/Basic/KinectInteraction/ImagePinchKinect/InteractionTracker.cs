using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;
using Reactive.Bindings;

namespace ImagePinchKinect
{
    public class InteractionTracker
    {
        public ReactiveProperty<UserInfo[]> UserInfoes { get; } = new ReactiveProperty<UserInfo[]>();
        public ReadOnlyReactiveProperty<UserInfo> UserInfo { get; }

        KinectSensor Sensor;
        InteractionStream InteractionStream;

        public InteractionTracker()
        {
            UserInfo = UserInfoes.Select(ToFirstUserInfo).ToReadOnlyReactiveProperty();

            if (DispatcherHelper.IsInDesignMode) return;
            if (!KinectSensor.KinectSensors.Any()) return;

            Sensor = KinectSensor.KinectSensors[0];
            Sensor.AllFramesReady += AllFramesReady;

            InteractionStream = new InteractionStream(Sensor, new DummyClient());
            InteractionStream.InteractionFrameReady += InteractionFrameReady;

            Sensor.DepthStream.Enable();
            Sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            Sensor.SkeletonStream.Enable();
            Sensor.Start();
        }

        void AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    InteractionStream.ProcessDepth(frame.GetRawPixelData(), frame.Timestamp);
                }
            }

            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    var skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
                    InteractionStream.ProcessSkeleton(skeletons, Sensor.AccelerometerGetCurrentReading(), frame.Timestamp);
                }
            }
        }

        void InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (var frame = e.OpenInteractionFrame())
            {
                if (frame == null) return;

                var userInfoes = new UserInfo[InteractionFrame.UserInfoArrayLength];
                frame.CopyInteractionDataTo(userInfoes);
                UserInfoes.Value = userInfoes;
            }
        }

        UserInfo ToFirstUserInfo(UserInfo[] us) =>
            UserInfo.Value == null ?
            us.FirstOrDefault(u => u.SkeletonTrackingId != 0) :
            us.FirstOrDefault(u => u.SkeletonTrackingId == UserInfo.Value.SkeletonTrackingId);
    }

    public class DummyClient : IInteractionClient
    {
        static readonly InteractionInfo DefaultInteractionInfo = new InteractionInfo();

        public InteractionInfo GetInteractionInfoAtLocation(int skeletonTrackingId, InteractionHandType handType, double x, double y)
        {
            return DefaultInteractionInfo;
        }
    }
}
