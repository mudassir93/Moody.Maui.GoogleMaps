using CoreLocation;
using AddressBookUI;

namespace Maui.GoogleMaps.iOS;

internal class GeocoderBackend
{
    public static void Register()
    {
        Geocoder.GetPositionsForAddressAsyncFunc = GetPositionsForAddressAsync;
        Geocoder.GetAddressesForPositionFuncAsync = GetAddressesForPositionAsync;
    }

    static Task<IEnumerable<string>> GetAddressesForPositionAsync(Position position)
    {
        var location = new CLLocation(position.Latitude, position.Longitude);
        var geocoder = new CLGeocoder();
        var source = new TaskCompletionSource<IEnumerable<string>>();
        geocoder.ReverseGeocodeLocation(location, (placemarks, error) =>
        {
            placemarks ??= Array.Empty<CLPlacemark>();
            var addresses = placemarks.Select(p => ABAddressFormatting.ToString(p.AddressDictionary, false));
            source.SetResult(addresses);
        });
        return source.Task;
    }

    static Task<IEnumerable<Position>> GetPositionsForAddressAsync(string address)
    {
        var geocoder = new CLGeocoder();
        var source = new TaskCompletionSource<IEnumerable<Position>>();
        geocoder.GeocodeAddress(address, (placemarks, error) =>
        {
            placemarks ??= Array.Empty<CLPlacemark>();
            var positions = placemarks.Select(p => new Position(p.Location.Coordinate.Latitude, p.Location.Coordinate.Longitude));
            source.SetResult(positions);
        });
        return source.Task;
    }
}