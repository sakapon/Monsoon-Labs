using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Reactive.Bindings;

namespace ShapeWpf
{
    public class AppModel
    {
        const string PolygonPointsString = "100,200 100,100 0,0 100,100 0,100 100,0 0,100 0,200";
        const string BaseRandomPointsString = "100,200 100,100 100,100 0,100 0,100 0,200";

        public PointCollection PolygonPoints { get; } = PointCollection.Parse(PolygonPointsString);

        public ReactiveProperty<TipPoints> RandomTips { get; } = new ReactiveProperty<TipPoints>(new TipPoints { X1 = 0, X2 = 100 });
        public ReadOnlyReactiveProperty<PointCollection> RandomPoints { get; }

        public AppModel()
        {
            Observable.Interval(TimeSpan.FromSeconds(1 / 60.0))
                .Select(_ => NextTips(RandomTips.Value))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(t => RandomTips.Value = t);
            RandomPoints = RandomTips.Select(CreateRandomPoints).ToReadOnlyReactiveProperty();
        }

        static PointCollection CreateRandomPoints(TipPoints tips)
        {
            var points = PointCollection.Parse(BaseRandomPointsString);
            points.Insert(2, new Point(tips.X1, 0));
            points.Insert(5, new Point(tips.X2, 0));
            return points;
        }

        static readonly Random Random = new Random();

        static TipPoints NextTips(TipPoints tips)
        {
            var d1 = Random.Next(-3, 4);
            var d2 = Random.Next(-3, 4);
            return new TipPoints { X1 = tips.X1 + d1, X2 = tips.X2 + d2 };
        }
    }

    public struct TipPoints
    {
        public int X1 { get; set; }
        public int X2 { get; set; }
    }
}
