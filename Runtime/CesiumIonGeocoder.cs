using Reinterop;
using System.Collections;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    /**
     * @brief The supported types of requests to geocoding API.
     */
    public enum CesiumIonGeocoderRequestType
    {
        /**
         * @brief Perform a full search from a complete query.
         */
        Search,

        /**
         * @brief Perform a quick search based on partial input, such as while a user
         * is typing.
         * The search results may be less accurate or exhaustive than using {@link GeocoderRequestType::Search}.
         */
        Autocomplete
    }

    /**
     * @brief The supported providers that can be accessed through ion's geocoder
     * API.
     */
    public enum CesiumIonGeocoderProviderType
    {
        /**
         * @brief Google geocoder, for use with Google data.
         */
        Google,

        /**
         * @brief Bing geocoder, for use with Bing data.
         */
        Bing,

        /**
         * @brief Use the default geocoder as set on the server. Used when neither
         * Bing or Google data is used.
         */
        Default
    };

    public class CesiumIonGeocoderFeature
    {
        /**
         * @brief The user-friendly display name of this feature.
         */
        public string displayName;

        /**
         * @brief The Longitude-Latitude-Height coordinates representing this feature.
         *
         * If the geocoder service returned a bounding box for this result, this will
         * return the bounding box. If the geocoder service returned a coordinate for
         * this result, this will return a zero-width rectangle at that coordinate.
         */
        public double3 positionLlh;
    }


    /**
     * @brief Attribution information for a query to a geocoder service.
     */
    public class CesiumIonGeocoderAttribution
    {
        /**
         * @brief An HTML string containing the necessary attribution information.
         */
        public string html;

        /**
         * @brief If true, the credit should be visible in the main credit container.
         * Otherwise, it can appear in a popover.
         */
        public bool showOnScreen;
    };

    /**
     * @brief The result of making a request to a geocoder service.
     */
    public class CesiumIonGeocoderResult
    {
        /**
         * @brief Any necessary attributions for this geocoder result.
         */
        public CesiumIonGeocoderAttribution[] attributions;

        /**
         * @brief The features obtained from this geocoder service, if any.
         */
        public CesiumIonGeocoderFeature[] features;
    };

    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumIonGeocoderImpl", "CesiumIonGeocoderImpl.h")]
    public partial class CesiumIonGeocoder
    {
        public partial Task<CesiumIonGeocoderResult> Geocode(
            CesiumIonServer ionServer,
            string ionToken,
            CesiumIonGeocoderProviderType providerType,
            CesiumIonGeocoderRequestType requestType,
            string query
       );
    }
}
