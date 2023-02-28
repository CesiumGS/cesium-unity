using System;

namespace CesiumForUnity
{
    internal class Helpers
    {
        public static string ToString<T>(T value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Produces an angle in the range -Pi <= angle <= Pi which is equivalent to the provided angle.
        /// </summary>
        /// <param name="angle">The angle in radians.</param>
        /// <returns>The angle in the range [`-Math.PI`, `Math.PI`].</returns>
        public static double NegativePiToPi(double angle)
        {
            if (angle >= -Math.PI && angle <= Math.PI)
            { 
                // Early exit if the input is already inside the range. This avoids
                // unnecessary math which could introduce floating point error.
                return angle;
            }
            return Helpers.ZeroToTwoPi(angle + Math.PI) - Math.PI;
        }

        /// <summary>
        /// Produces an angle in the range -180 <= angle <= 180 which is equivalent to the provided angle.
        /// </summary>
        /// <param name="angle">The angle in degrees.</param>
        /// <returns>The angle in the range [-180, 180].</returns>
        public static double Negative180To180(double angle)
        {
            if (angle >= -180.0 && angle <= 180.0)
            {
                // Early exit if the input is already inside the range. This avoids
                // unnecessary math which could introduce floating point error.
                return angle;
            }
            return Helpers.ZeroTo360(angle + 180.0) - 180.0;
        }

        /// <summary>
        /// Produces an angle in the range 0 <= angle <= 2Pi which is equivalent to the provided angle.
        /// </summary>
        /// <param name="angle">The angle in radians.</param>
        /// <returns>The angle in the range [0, `Math::TwoPi`]</returns>
        public static double ZeroToTwoPi(double angle)
        {
            if (angle >= 0 && angle <= 2.0 * Math.PI)
            {
                // Early exit if the input is already inside the range. This avoids
                // unnecessary math which could introduce floating point error.
                return angle;
            }
            double mod = Helpers.Mod(angle, 2.0 * Math.PI);
            if (Math.Abs(mod) < 1.0e-14 && Math.Abs(angle) > 1.0e-14)
            {
                return 2.0 * Math.PI;
            }
            return mod;
        }

        /// <summary>
        /// Produces an angle in the range 0 <= angle <= 360 which is equivalent to the provided angle.
        /// </summary>
        /// <param name="angle">The angle in degrees.</param>
        /// <returns>The angle in the range [0, 360]</returns>
        public static double ZeroTo360(double angle)
        {
            if (angle >= 0 && angle <= 360.0)
            {
                // Early exit if the input is already inside the range. This avoids
                // unnecessary math which could introduce floating point error.
                return angle;
            }
            double mod = Helpers.Mod(angle, 360.0);
            if (Math.Abs(mod) < 1.0e-14 && Math.Abs(angle) > 1.0e-14)
            {
                return 360.0;
            }
            return mod;
        }

        /// <summary>
        /// The modulo operation that also works for negative dividends.
        /// </summary>
        /// <param name="m">The dividend.</param>
        /// <param name="n">The divisor.</param>
        /// <returns>The remainder.</returns>
        public static double Mod(double m, double n)
        {
            if (Math.Sign(m) == Math.Sign(n) && Math.Abs(m) < Math.Abs(n)) {
              // Early exit if the input does not need to be modded. This avoids
              // unnecessary math which could introduce floating point error.
              return m;
            }
            return ((m % n) + n) % n;
        }
    }
}
