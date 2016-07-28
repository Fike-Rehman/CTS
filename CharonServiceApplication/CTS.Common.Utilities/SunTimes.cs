using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CTS.Common.Utilities
{
    /// <summary>
    /// Provides Sunset and sunrise times for the given location coordinates. If Location
    /// coordinate properties are not set prior to calling the GetSunTimes methods, Sunrise
    /// and Sunset times for Minneapolis are returned by default. The class utilizes
    /// http://sunrise-sunset.org/api . 
    /// </summary>
    public static class SunTimes
    {
        // By default we use Minneapolis coordinates
        public static double Latitude { get; set; } = 44.9799700;

        public static double Longitude { get; set; } = -93.2638400;

        /// <summary>
        /// This methods returns todays Sunrise and Sunset times for the location coordinates
        /// set earlier. The times returned are converted to the local time zone where the application is running.
        /// It utilizes sunrise-sunset API (http://sunrise-sunset.org/api). If the API doesn't return a valid
        /// JSON response, this will throw an exception. 
        /// </summary>
        /// <param name="sunriseTime"></param>
        /// <param name="sunsetTime"></param>
        public static void GetSunTimes(out DateTime sunriseTime, out DateTime sunsetTime)
        {
            sunriseTime = default(DateTime);
            sunsetTime = default(DateTime);

            using (var client = new WebClient())
            {
                try
                {
                    var url = "http://api.sunrise-sunset.org/json" + $"?lat={Latitude}&lng={Longitude}";

                    var responsestring = client.DownloadString(url);

                    JToken token = JObject.Parse(responsestring);

                    var sunrise = (DateTime)token.SelectToken("results").SelectToken("sunrise");

                    var sunset = (DateTime)token.SelectToken("results").SelectToken("sunset");

                    // These times are in UTC. We must convert and return times in local time
                    // zones taking into consideration the daylight savings time.
                    var localTimezone = TimeZone.CurrentTimeZone;

                    var currentOffset = localTimezone.GetUtcOffset(DateTime.Now);

                    sunriseTime = sunrise + currentOffset;
                    sunsetTime = sunset + currentOffset;

                    if(sunsetTime.Date < DateTime.Now.Date)
                    {
                        // Add a day because we are looking for the upcoming sunset time.
                        sunsetTime = sunset + currentOffset + new TimeSpan(1, 0, 0, 0);  
                    }
                    

                }
                catch (JsonReaderException)
                {
                    throw new Exception("Failed to get a valid Sunrise/Sunset Time values");

                }
                catch (Exception)
                {
                    throw new Exception("Failed to get a valid Sunrise/Sunset Time values");
                }
            }
        }

        /// <summary>
        /// This methods returns todays Sunrise and Sunset times for the location coordinates
        /// set earlier. The times returned are Universal Coordinated Times.
        /// The method utilizes sunrise-sunset API (http://sunrise-sunset.org/api). If the API doesn't return a valid
        /// JSON response, this will throw an exception.
        /// </summary>
        /// <param name="sunriseTime"></param>
        /// <param name="sunsetTime"></param>
        public static void GetSunTimesUTC(out DateTime sunriseTime, out DateTime sunsetTime)
        {
            sunriseTime = default(DateTime);
            sunsetTime = default(DateTime);
           
            using (var client = new WebClient())
            {
                try
                {
                    var url = "http://api.sunrise-sunset.org/json" + $"?lat={Latitude}&lng={Longitude}";

                    var responsestring = client.DownloadString(url);

                    JToken token = JObject.Parse(responsestring);

                    sunriseTime = (DateTime) token.SelectToken("results").SelectToken("sunrise");

                    sunsetTime = (DateTime) token.SelectToken("results").SelectToken("sunset");

                }
                catch (JsonReaderException)
                {
                    throw new Exception("Failed to get a valid Sunrise/Sunset Time values");

                }
                catch (Exception)
                {
                    throw new Exception("Failed to get a valid Sunrise/Sunset Time values");
                }              
            }
        }
    }
}
