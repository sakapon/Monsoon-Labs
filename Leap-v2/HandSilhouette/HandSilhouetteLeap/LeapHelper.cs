using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using LVector = Leap.Vector;

namespace HandSilhouetteLeap
{
    public static class LeapHelper
    {
        public static Vector3D ToVector3D(this LVector v)
        {
            if (v == null) throw new ArgumentNullException();
            return new Vector3D(v.x, v.y, v.z);
        }

        public static Point3D ToPoint3D(this LVector v)
        {
            if (v == null) throw new ArgumentNullException();
            return new Point3D(v.x, v.y, v.z);
        }
    }

    public static class VectorHelper
    {
        public static Point ToPoint(this Point3D p) =>
            new Point(p.X, p.Y);

        public static Point3D Floor(this Point3D p) =>
            new Point3D(Math.Floor(p.X), Math.Floor(p.Y), Math.Floor(p.Z));

        public static double ToRadian(double degree) => degree * Math.PI / 180.0;
        public static double FromRadian(double radian) => radian * 180.0 / Math.PI;
    }

    public class InclinableMapper
    {
        public Vector3D RealScreenCenter { get; set; } = new Vector3D(0, 250, 0);
        public Vector3D VirtualScreenCenter { get; set; } = new Vector3D(960, 540, 0);
        public double AngleDegrees { get; set; } = 0;
        public double PixelsPerMillimeter { get; set; } = 5;

        /// <summary>
        /// Leap Motion Controller の座標系におけるベクトルをスクリーンの座標系に変換します。
        /// </summary>
        /// <param name="p">Leap Motion Controller の座標系における座標。</param>
        /// <returns>スクリーンの座標系における座標。</returns>
        public Point3D ToScreenPosition(Point3D p)
        {
            var m = new Matrix3D();

            m.Translate(-RealScreenCenter);
            m.Rotate(new Quaternion(new Vector3D(1, 0, 0), AngleDegrees));
            m.Scale(new Vector3D(PixelsPerMillimeter, -PixelsPerMillimeter, PixelsPerMillimeter));
            m.Translate(VirtualScreenCenter);

            return p * m;
        }
    }
}
