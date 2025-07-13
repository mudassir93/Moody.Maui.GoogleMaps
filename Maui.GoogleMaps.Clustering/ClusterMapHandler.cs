using Maui.GoogleMaps.Handlers;

namespace Maui.GoogleMaps.Clustering
{
    public partial class ClusterMapHandler : MapHandler
    {
        public static PropertyMapper<ClusteredMap, ClusterMapHandler> ClusterMapMapper = new(MapMapper)
        {
#if ANDROID || IOS
            [nameof(IClusteredMap.GeoJson)] = MapGeoJson,
#endif
        };
        public ClusterMapHandler() : base(ClusterMapMapper)
        {

        }
    }
}
