namespace Masticore.Queue
{
    /// <summary>
    /// Class defining default implementation for type determination
    /// </summary>
    public static class TypeNameUtils
    {
        /// <summary>
        /// Equivalent logic as <see cref="TypeNameForObject"/>, but for a class
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <returns></returns>
        public static string TypeNameForClass<TClass>()
        {
            return typeof(TClass).Name;
        }

        /// <summary>
        /// Equivalent logic as <see cref="TypeNameForClass{TClass}"/>, but for an object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string TypeNameForObject(object o)
        {
            return o.GetType().Name;
        }
    }
}
