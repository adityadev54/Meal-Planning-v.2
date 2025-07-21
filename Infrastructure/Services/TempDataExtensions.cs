using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Meal_Planning.Infrastructure.Services
{
    public static class TempDataExtensions
    {
        /// <summary>
        /// Read a value from TempData without marking it for deletion
        /// </summary>
        /// <param name="tempData">The TempData dictionary</param>
        /// <param name="key">The key to look up</param>
        /// <returns>The stored object or null if not found</returns>
        public static object? Peek(this ITempDataDictionary tempData, string key)
        {
            tempData.TryGetValue(key, out object? value);
            return value;
        }
    }
}
