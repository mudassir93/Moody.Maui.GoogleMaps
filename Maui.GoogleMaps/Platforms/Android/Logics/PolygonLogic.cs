using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Maui.GoogleMaps.Android;
using Maui.GoogleMaps.Android.Extensions;
using Microsoft.Maui.Platform;
using NativePolygon = Android.Gms.Maps.Model.Polygon;

namespace Maui.GoogleMaps.Logics.Android;

public class PolygonLogic : DefaultPolygonLogic<NativePolygon, GoogleMap>
{
    protected override IList<Polygon> GetItems(Map map)
    {
        return map.Polygons;
    }

    public override void Register(GoogleMap oldNativeMap, Map oldMap, GoogleMap newNativeMap, Map newMap, IElementHandler handler)
    {
        base.Register(oldNativeMap, oldMap, newNativeMap, newMap, handler);

        if (newNativeMap != null)
        {
            newNativeMap.PolygonClick += OnPolygonClick;
        }
    }

    public override void Unregister(GoogleMap nativeMap, Map map)
    {
        if (nativeMap != null)
        {
            nativeMap.PolygonClick -= OnPolygonClick;
        }

        base.Unregister(nativeMap, map);
    }

    protected override NativePolygon CreateNativeItem(Polygon outerItem)
    {
        var opts = new PolygonOptions();

        foreach (var p in outerItem.Positions)
        {
            opts.Add(new LatLng(p.Latitude, p.Longitude));
        }

        opts.InvokeStrokeWidth(outerItem.StrokeWidth * this.ScaledDensity); // TODO: convert from px to pt. Is this collect? (looks like same iOS Maps)
        opts.InvokeStrokeColor(outerItem.StrokeColor.ToPlatform());
        opts.InvokeFillColor(outerItem.FillColor.ToPlatform());
        opts.Clickable(outerItem.IsClickable);
        opts.InvokeZIndex(outerItem.ZIndex);

        foreach (var hole in outerItem.Holes)
        {
            opts.Holes.Add(hole.Select(x => x.ToLatLng()).ToJavaList());
        }

        var nativePolygon = NativeMap.AddPolygon(opts);

        // associate pin with marker for later lookup in event handlers
        outerItem.NativeObject = nativePolygon;
        outerItem.SetOnPositionsChanged((polygon, e) =>
        {
            var native = polygon.NativeObject as NativePolygon;
            native.Points = polygon.Positions.ToLatLngs();
        });

        outerItem.SetOnHolesChanged((polygon, e) =>
        {
            var native = polygon.NativeObject as NativePolygon;
            native.SetHoles((IList<IList<LatLng>>)polygon.Holes
                .Select(x => (IList<LatLng>)x.Select(y=>y.ToLatLng()).ToJavaList())
                            .ToJavaList());
        });

        return nativePolygon;
    }

    protected override NativePolygon DeleteNativeItem(Polygon outerItem)
    {
        outerItem.SetOnHolesChanged(null);
        outerItem.SetOnPositionsChanged(null);

        if (outerItem.NativeObject is not NativePolygon nativePolygon)
        {
            return null;
        }

        nativePolygon.Remove();
        outerItem.NativeObject = null;
        return nativePolygon;
    }

    void OnPolygonClick(object sender, GoogleMap.PolygonClickEventArgs e)
    {
        // clicked polyline
        var nativeItem = e.Polygon;

        // lookup pin
        var targetOuterItem = GetItems(Map).FirstOrDefault(
            outerItem => ((NativePolygon)outerItem.NativeObject).Id == nativeItem.Id);

        // only consider event handled if a handler is present.
        // Else allow default behavior of displaying an info window.
        targetOuterItem?.SendTap();
    }

    internal override void OnUpdateIsClickable(Polygon outerItem, NativePolygon nativeItem)
    {
        nativeItem.Clickable = outerItem.IsClickable;
    }

    internal override void OnUpdateStrokeColor(Polygon outerItem, NativePolygon nativeItem)
    {
        nativeItem.StrokeColor = outerItem.StrokeColor.ToPlatform();
    }

    internal override void OnUpdateStrokeWidth(Polygon outerItem, NativePolygon nativeItem)
    {
        // TODO: convert from px to pt. Is this collect? (looks like same iOS Maps)
        nativeItem.StrokeWidth = outerItem.StrokeWidth * this.ScaledDensity;
    }

    internal override void OnUpdateFillColor(Polygon outerItem, NativePolygon nativeItem)
    {
        nativeItem.FillColor = outerItem.FillColor.ToPlatform();
    }

    internal override void OnUpdateZIndex(Polygon outerItem, NativePolygon nativeItem)
    {
        nativeItem.ZIndex = outerItem.ZIndex;
    }
}