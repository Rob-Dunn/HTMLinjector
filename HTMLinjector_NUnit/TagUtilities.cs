using System;
using System.Collections.Generic;
using HTMLinjectorServices;
using NUnit.Framework;

namespace HTMLinjector_NUnit
{
    internal class TagUtilities
    {
        /// <summary>
        /// Checks that two Tag objects have identical properties
        /// </summary>
        /// <param name="expectedTag">Expected tag.</param>
        /// <param name="actualTag">Actual tag.</param>
        internal static void AreTagsSame(Tag expectedTag, Tag actualTag)
        {
            Assert.IsTrue((expectedTag == null) == (actualTag == null), "One Tag is null but the other isn't");

            Assert.AreEqual(expectedTag.Contents, actualTag.Contents);
            Assert.AreEqual(expectedTag.HasContent, actualTag.HasContent);
            Assert.AreEqual(expectedTag.Name, actualTag.Name);
            Assert.AreEqual(expectedTag.SearchText, actualTag.SearchText);

            TagUtilities.AreTagPositionsSame(expectedTag.StartTagPosition, actualTag.StartTagPosition);
            TagUtilities.AreTagPositionsSame(expectedTag.EndTagPosition, actualTag.EndTagPosition);

            TagUtilities.ArePropertiesSame(expectedTag.Properties, actualTag.Properties);
        }

        internal static void AreTagPositionsSame(TagPosition expectedTagPosition, TagPosition actualTagPosition)
        {
            if (expectedTagPosition == null && actualTagPosition == null) return;
            Assert.IsTrue((expectedTagPosition == null) == (actualTagPosition == null), "One TagPosition is null but the other isn't");

            Assert.AreEqual(expectedTagPosition.EndPosition, actualTagPosition.EndPosition);
            Assert.AreEqual(expectedTagPosition.StartEndPosition ,actualTagPosition.StartEndPosition);
            Assert.AreEqual(expectedTagPosition.StartPosition, actualTagPosition.StartPosition);
        }

        /// <summary>
        /// Checks that two lists of tag properties match
        /// </summary>
        /// <param name="expectedProperties">Expected properties.</param>
        /// <param name="actualProperties">Actual properties.</param>
        internal static void ArePropertiesSame(Dictionary<string, string> expectedProperties, Dictionary<string, string> actualProperties)
        {
            if (expectedProperties == null && actualProperties == null) return;

            Assert.AreEqual(expectedProperties.Count, actualProperties.Count);

            foreach(string key in expectedProperties.Keys)
            {
                string expectedPropertyValue = expectedProperties[key];

                string actualPropertyValue = null;
                actualProperties.TryGetValue(key, out actualPropertyValue);

                Assert.AreEqual(expectedPropertyValue, actualPropertyValue);
            }
        }

        /// <summary>
        /// Factory method for creating test Tags
        /// </summary>
        /// <returns>The test tag.</returns>
        /// <param name="tagName">Tag name.</param>
        /// <param name="searchText">Search text.</param>
        /// <param name="startTagStartPosition">Start tag start position.</param>
        /// <param name="startTagStartEndPosition">Start tag start end position.</param>
        /// <param name="startTagEndPosition">Start tag end position.</param>
        /// <param name="endTagStartPosition">End tag start position.</param>
        /// <param name="endTagStartEndPosition">End tag start end position.</param>
        /// <param name="endTagEndPosition">End tag end position.</param>
        /// <param name="content">Content.</param>
        /// <param name="properties">Properties.</param>
        internal static Tag CreateTestTag(
            string tagName, 
            string searchText, 
            int startTagStartPosition, 
            int startTagStartEndPosition, 
            int startTagEndPosition,
            int? endTagStartPosition = null,
            int? endTagStartEndPosition = null,
            int? endTagEndPosition = null,
            string content = null,
            Dictionary<string, string> properties = null
        )
        {
            TagPosition startTagPosition = new TagPosition() 
            { 
                StartPosition = startTagStartPosition, 
                StartEndPosition = startTagStartEndPosition, 
                EndPosition = startTagEndPosition 
            };

            // Create an end tag, if needed
            TagPosition endTagPosition = null;
            if (endTagStartPosition.HasValue && endTagStartEndPosition.HasValue && endTagEndPosition.HasValue)
            {
                endTagPosition = new TagPosition() 
                { 
                    StartPosition = endTagStartPosition.Value, 
                    StartEndPosition = endTagStartEndPosition.Value, 
                    EndPosition = endTagEndPosition.Value 
                };
            }

            return new Tag()
            {
                Contents = content,
                HasContent = content != null,
                Name = tagName,
                Properties = properties == null ? new Dictionary<string, string>() : properties,
                SearchText = searchText,
                StartTagPosition = startTagPosition,
                EndTagPosition = endTagPosition
            };
        }
    }
}
