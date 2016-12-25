//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;

using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    public class WeatherMakerDayNightCycleScript : MonoBehaviour
    {
        [Tooltip("The camera the day/night cycle is running in.")]
        public Camera Camera;

        [Tooltip("The sun light to use for the day night cycle.")]
        public Light Sun;

        [Tooltip("Intensity of the sun light at full intensity. " +
            "Change this value to set the maximum intensity of the sun. Do not modify the sun light object intensity directly.")]
        public float SunFullIntensity = 1.0f;

        [Range(-100000, 100000.0f)]
        [Tooltip("The speed of the cycle. Set to 0 to freeze the cycle and manually control it. At a speed of 1, the cycle is in real-time. " +
            "A speed of 100 is 100 times faster than normal. Negative numbers run the cycle backwards.")]
        public float Speed = 10.0f;

        [Tooltip("The current time of day in seconds (local time).")]
        public float TimeOfDay = SecondsPerDay * 0.5f; // high noon default time of day

        [Tooltip("The year for simulating the sun position - this is not changed automatically and must be set and updated by you. " +
            "The calculation is only correct for dates in the range March 1 1900 to February 28 2100.")]
        public int Year = 2000;

        [Tooltip("The month for simulating the sun position - this is not changed automatically and must be set and updated by you.")]
        public int Month = 9;

        [Tooltip("The day for simulating the sun position - this is not changed automatically and must be set and updated by you.")]
        public int Day = 21;

        [Tooltip("Offset for the time zone of the lat / lon in seconds. You must calculate this based on the lat/lon you are providing and the year/month/day.")]
        public int TimeZoneOffsetSeconds = 21600;

        [Range(-90.0f, 90.0f)]
        [Tooltip("The latitude in degrees on the planet that the camera is at - 90 (north pole) to -90 (south pole)")]
        public double Latitude = 40.7608; // salt lake city latitude

        [Range(-180.0f, 180.0f)]
        [Tooltip("The longitude in degrees on the planet that the camera is at. -180 to 180.")]
        public double Longitude = -111.8910; // salt lake city longitude

        [Tooltip("The amount of degrees your planet is tilted - Earth is about 23.439f")]
        public float AxisTilt = 23.439f;

        [Tooltip("Begin fading out the sun when it's dot product vs. the down vector becomes less than or equal to this value.")]
        public float SunDotFadeThreshold = -0.3f;

        [Tooltip("Disable the sun when it's dot product vs. the down vector becomes less than or equal to this value.")]
        public float SunDotDisableThreshold = -0.4f;

        /// <summary>
        /// The normal vector of the sun, pointing towards 0,0,0
        /// </summary>
        public Vector3 SunNormal { get; private set; }

        /// <summary>
        /// Number of seconds per day
        /// </summary>
        public const float SecondsPerDay = 86400.0f;

        /// <summary>
        /// Time of day at high noon
        /// </summary>
        public const float HighNoonTimeOfDay = SecondsPerDay * 0.5f;

        /// <summary>
        /// Number of seconds in one degree
        /// </summary>
        public const float SecondsForOneDegree = SecondsPerDay / 360.0f;

        /// <summary>
        /// Calculate the position of the sun
        /// </summary>
        /// <param name="dateTime">DateTime - must be in UTC format</param>
        /// <param name="latitude">Latitude in decimal degrees</param>
        /// <param name="longitude">Longitude in decimal degrees</param>
        /// <param name="axisTilt">Degrees tilt of the planet axis - earth is about 23.439</param>
        /// <returns>Unit vector pointing out towards the sun</returns>
        public static Vector3 CalculateSunPosition(DateTime dateTime, double latitude, double longitude, double axisTilt)
        {
            // dateTime should already be UTC format
            double d = (dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / dayMs) + jDiff;
            double e = degreesToRadians * axisTilt; // obliquity of the Earth
            double m = SolarMeanAnomaly(d);
            double l = EclipticLongitude(m);
            double dec = Declination(e, l, 0);
            double ra = RightAscension(e, l, 0);
            double lw = -degreesToRadians * longitude;
            double phi = degreesToRadians * latitude;
            double h = SiderealTime(d, lw) - ra;
            double azimuth = Azimuth(h, phi, dec);
            double altitude = Altitude(h, phi, dec);

            float y = (float)Math.Sin(altitude);
            float hyp = (float)Math.Cos(altitude);
            float z = hyp * (float)Math.Cos(azimuth);
            float x = hyp * (float)Math.Sin(azimuth);
            return new Vector3(x, y, z);
        }

        private const double degreesToRadians = Math.PI / 180.0;
        private const double dayMs = 1000.0 * 60.0 * 60.0 * 24.0;
        private const double j1970 = 2440587.5;
        private const double j2000 = 2451545.0;
        private const double jDiff = (j1970 - j2000);

        private float lastTimeOfDay = float.MinValue;

        private static double RightAscension(double e, double l, double b)
        {
            return Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l));
        }

        private static double Declination(double e, double l, double b)
        {
            return Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l));
        }

        private static double Azimuth(double h, double phi, double dec)
        {
            return Math.Atan2(Math.Sin(h), Math.Cos(h) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi));
        }

        private static double Altitude(double h, double phi, double dec)
        {
            return Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(h));
        }

        private static double SiderealTime(double d, double lw)
        {
            return degreesToRadians * (280.16 + 360.9856235 * d) - lw;
        }

        private static double SolarMeanAnomaly(double d)
        {
            return degreesToRadians * (357.5291 + 0.98560028 * d);
        }

        private static double EclipticLongitude(double m)
        {
            double c = degreesToRadians * (1.9148 * Math.Sin(m) + 0.02 * Math.Sin(2.0 * m) + 0.0003 * Math.Sin(3.0 * m)); // equation of center
            double p = degreesToRadians * 102.9372; // perihelion of the Earth
            return m + c + p + Math.PI;
        }

        private static double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return (2 * Math.PI) + angleInRadians;
            }
            else if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians - (2 * Math.PI);
            }
            else
            {
                return angleInRadians;
            }
        }

        private void DoDayNightCycle()
        {
            if (Speed != 0.0f)
            {
                TimeOfDay += (Speed * Time.deltaTime);
                if (TimeOfDay >= SecondsPerDay)
                {
                    TimeOfDay -= SecondsPerDay;
                }
            }

            if (lastTimeOfDay != TimeOfDay)
            {
                if (Camera.orthographic)
                {
                    Sun.transform.rotation = Quaternion.AngleAxis(180.0f + ((TimeOfDay / SecondsPerDay) * 360.0f), Vector3.right);
                }
                else
                {
                    // position the sun far out at the edge of the sky sphere using solar calculations

                    // convert local time of day to UTC time of day - quick and dirty calculation
                    double offsetSeconds = TimeZoneOffsetSeconds;// 3600.0 * (Math.Sign(Longitude) * Longitude * 24.0 / 360.0);
                    TimeSpan t = TimeSpan.FromSeconds(TimeOfDay + offsetSeconds);
                    DateTime dt = new DateTime(Year, Month, Day, 0, 0, 0, DateTimeKind.Utc) + t;
                    SunNormal = -CalculateSunPosition(dt, Latitude, Longitude, AxisTilt);

                    // position and scale the sun
                    Sun.transform.position = Camera.transform.position - SunNormal;
                    Sun.transform.LookAt(Camera.transform.position, Vector3.up);
                    Sun.transform.position = (Sun.transform.forward * Camera.farClipPlane * -0.8f);

                    float dot = Vector3.Dot(Sun.transform.forward, Vector3.down);
                    Sun.enabled = (dot > SunDotDisableThreshold);
                    if (dot <= SunDotFadeThreshold && dot >= SunDotDisableThreshold)
                    {
                        Debug.Assert(SunDotDisableThreshold <= SunDotFadeThreshold, "SunDotDisableThreshold should be less than or equal to SunDotFadeThreshold");

                        float range = (SunDotFadeThreshold - SunDotDisableThreshold);
                        if (TimeOfDay < HighNoonTimeOfDay)
                        {
                            Sun.intensity = Mathf.Lerp(0.0f, SunFullIntensity, (dot - SunDotDisableThreshold) / range);
                        }
                        else
                        {
                            Sun.intensity = Mathf.Lerp(SunFullIntensity, 0.0f, (SunDotFadeThreshold - dot) / range);
                        }
                    }
                    else
                    {
                        Sun.intensity = SunFullIntensity;
                    }
                }
                lastTimeOfDay = TimeOfDay;
            }
        }

        private void Start()
        {
            DoDayNightCycle();
        }

        private void Update()
        {
            DoDayNightCycle();
        }
    }
}

// resources:
// https://en.wikipedia.org/wiki/Position_of_the_Sun
// http://stackoverflow.com/questions/8708048/position-of-the-sun-given-time-of-day-latitude-and-longitude
// http://www.grasshopper3d.com/forum/topics/solar-calculation-plugin
// http://guideving.blogspot.nl/2010/08/sun-position-in-c.html
// https://github.com/mourner/suncalc
// http://stackoverflow.com/questions/1058342/rough-estimate-of-the-time-offset-from-gmt-from-latitude-longitude
// http://www.stjarnhimlen.se/comp/tutorial.html
// http://www.suncalc.net/#/40.7608,-111.891,12/2000.09.21/12:46
// http://www.suncalc.net/scripts/suncalc.js

// a secondary calculation algorithm that is slightly less performant
/*
// Number of days from J2000.0.  
// double d = 367 * dateTime.Year - (7 * (dateTime.Year + ((dateTime.Month + 9) / 12))) / 4 + (275 * dateTime.Month) / 9 + dateTime.Day - 730530;
double julianDate = 367 * dateTime.Year -
    (int)((7.0 / 4.0) * (dateTime.Year +
    (int)((dateTime.Month + 9.0) / 12.0))) +
    (int)((275.0 * dateTime.Month) / 9.0) +
    dateTime.Day - 730531.5;

double julianCenturies = julianDate / 36525.0;

// Sidereal Time  
double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;

double siderealTimeUT = siderealTimeHours +
    (366.2422 / 365.2422) * (double)dateTime.TimeOfDay.TotalHours;

double siderealTime = siderealTimeUT * 15 + longitude;
latitude *= degreesToRadians;

// Refine to number of days (fractional) to specific time.  
julianDate += (double)dateTime.TimeOfDay.TotalHours / 24.0;
julianCenturies = julianDate / 36525.0;

// Solar Coordinates  
double meanLongitude = CorrectAngle(degreesToRadians *
    (280.466 + 36000.77 * julianCenturies));

double meanAnomaly = CorrectAngle(degreesToRadians *
    (357.529 + 35999.05 * julianCenturies));

double equationOfCenter = degreesToRadians * ((1.915 - 0.005 * julianCenturies) *
    Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

double elipticalLongitude =
    CorrectAngle(meanLongitude + equationOfCenter);

double obliquity = (axisTilt - 0.013 * julianCenturies) * degreesToRadians;

// Right Ascension  
double rightAscension = Math.Atan2(
    Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
    Math.Cos(elipticalLongitude));

double declination = Math.Asin(
    Math.Sin(rightAscension) * Math.Sin(obliquity));

// Horizontal Coordinates  
double hourAngle = CorrectAngle(siderealTime * degreesToRadians) - rightAscension;

if (hourAngle > Math.PI)
{
    hourAngle -= 2 * Math.PI;
}

double altitude = Math.Asin(Math.Sin(latitude) *
    Math.Sin(declination) + Math.Cos(latitude) *
    Math.Cos(declination) * Math.Cos(hourAngle));

// Nominator and denominator for calculating Azimuth  
// angle. Needed to test which quadrant the angle is in.  
double aziNom = -Math.Sin(hourAngle);
double aziDenom =
    Math.Tan(declination) * Math.Cos(latitude) -
    Math.Sin(latitude) * Math.Cos(hourAngle);

double azimuth = Math.Atan(aziNom / aziDenom);

if (aziDenom < 0) // In 2nd or 3rd quadrant  
{
    azimuth += Math.PI;
}
else if (aziNom < 0) // In 4th quadrant  
{
    azimuth += 2 * Math.PI;
}
*/
