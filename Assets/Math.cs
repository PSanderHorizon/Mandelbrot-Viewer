namespace Assets
{
    public static class Math
    {
        /// <summary>
        /// Linearly maps a value x in the range [a, b] to its equivalent position
        /// in the range [m, n]. For example 2 in [0, 10] becomes 14 in [10, 30].
        /// </summary>
        /// <param name="x">The initial value to map.</param>
        /// <param name="a">The start of the initial range.</param>
        /// <param name="b">The end of the initial range.</param>
        /// <param name="m">The start of the new range.</param>
        /// <param name="n">The end of the new range.</param>
        /// <returns></returns>
        public static double MapRange(double x, double a, double b, double m, double n)
            => (x - a) / (b - a) * (n - m) + m;
    }
}
