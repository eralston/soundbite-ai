using Masticore;

namespace AutoMapper
{
    /// <summary>
    /// Utility extensions for <see cref="IMapper"/>
    /// </summary>
    public static class IMapperExtensions
    {
        /// <summary>
        /// Automapper throws exceptions when the source object is null.  This extension method is
        /// responsible for returning a null reference if the source object is null.
        /// </summary>
        /// <typeparam name="TDestination">.NET type of the result.</typeparam>
        /// <param name="mapper">Reference to the mapper.</param>
        /// <param name="source">Source object to map to the target type.</param>
        /// <remarks>ONLY WORKS IF THE QUERY HAS TRACKING TURNED ON</remarks>
        /// <returns>a mapped instance if the source object is provided, otherwise <c>null</c>.</returns>
        public static TDestination MapSafe<TDestination>(this IMapper mapper, object source)
            where TDestination : class
        {
            Validator.ArgNotNull(nameof(mapper), mapper);
            return source == null ? null : mapper.Map<TDestination>(source);
        }

        /// <summary>
        /// Allows for all the mapping functionality of the <see cref="MapSafe"/> extension method
        /// but also provides a parameter array where you can pass ad-hoc LINQ projection references
        /// into the method. Unchained LINQ queries do not have an Include option like chained 
        /// queries do, so this tricks the statement into loading child data.  This is useful when
        /// mapping operations use properties internally but they are otherwise not specified in the
        /// LINQ query itself.
        /// </summary>        
        /// <remarks>
        /// ONLY WORKS IF THE QUERY HAS TRACKING TURNED ON
        /// Properties in a chain MUST be referenced invididually.  In other words, passing a single
        /// parameter like (Obj.Property.SubProperty) will not work, but passing two parameters like
        /// (Obj.Property, Obj.Property.SubProperty) will work.
        /// </remarks>
        /// <typeparam name="TDestination">.NET type of the result.</typeparam>
        /// <param name="mapper">Reference to the mapper.</param>
        /// <param name="source">Source object to map to the target type.</param>
        /// <param name="_">Ad-hoc references that are disregarded within this method.</param>
        /// <returns>a mapped instance if the source object is provided, otherwise <c>null</c>.</returns>
        public static TDestination MapSafeWith<TDestination>(this IMapper mapper, object source, params object[] _)
            where TDestination : class
        {
            Validator.ArgNotNull(nameof(mapper), mapper);
            return source == null ? null : mapper.Map<TDestination>(source);
        }
    }
}