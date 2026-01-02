using System;

namespace Masticore.Tests
{
    /// <summary>
    /// Contains testing constants
    /// </summary>
    public static class TestUtils
    {
        /// <summary>
        /// Flag indicating whether to pass empty tests.
        /// </summary>
        public const bool PassEmptyTests = true;

        /// <summary>
        /// Used to mark tests as incomplete or not implemented.
        /// </summary>
        public static void TestNotFullyImplemented()
        {
            if (!PassEmptyTests)
            {
                throw new NotImplementedException("Test is not implemented");
            }
        }

        public static int GetExpectedLastPageItemCount(int totalItemCount, int itemsToTake)
        {
            int result = (totalItemCount) % itemsToTake;
            result = result == 0 ? itemsToTake : result;
            return result;
        }

        public static int GetExpectedPageCount(int totalItemCount, int itemsToTake)
        {
            int expLastPageItemCount = GetExpectedLastPageItemCount(totalItemCount, itemsToTake);
            int result = (totalItemCount / itemsToTake) + (expLastPageItemCount == 0 ? 0 : 1);
            return result;
        }

    }
}