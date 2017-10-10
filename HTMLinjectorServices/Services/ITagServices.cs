using System.Collections.Generic;

namespace HTMLinjectorServices
{
    public interface ITagServices
    {
        /// <summary>
        /// Finds all of the specfied tags in the specified text 
        /// </summary>
        /// <returns>A list of Tags found in the specified text</returns>
        /// <param name="searchText">The text to search for Tags</param>
        /// <param name="tagName">The name of the Tags to search for</param>
        /// <param name="hasContent">True when there is a Start and End tag</param>
        List<Tag> ExtractTags(string searchText, string tagName, bool hasContent);
    }
}
