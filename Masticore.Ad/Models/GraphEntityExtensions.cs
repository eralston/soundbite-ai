namespace Masticore.Ad
{
    /// <summary>
    /// Utility Methods for working with Graph entities
    /// </summary>
    public static class GraphEntityExtensions
    {
        /// <summary>
        /// Returns true if <see cref="GraphBase.Type"/> has a value and it matches the group type; otherwise, returns null for Type missing or false for definite mismatch
        /// </summary>
        /// <param name="graphObj"></param>
        /// <returns></returns>
        public static bool? IsUser(this GraphBase graphObj)
        {
            if (graphObj.Type == null)
            {
                return null;
            }

            return graphObj.Type == GraphUser.UserDataType;
        }

        /// <summary>
        /// Returns true if <see cref="GraphBase.Type"/> has a value and it matches the user type; otherwise, returns null for Type missing or false for definite mismatch
        /// </summary>
        /// <param name="graphObj"></param>
        /// <returns></returns>
        public static bool? IsGroup(this GraphBase graphObj)
        {
            if (graphObj.Type == null)
            {
                return null;
            }

            return graphObj.Type == GraphGroup.GroupDataType;
        }
    }
}