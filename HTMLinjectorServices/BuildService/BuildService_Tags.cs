using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HTMLinjectorServices.Models;

namespace HTMLinjectorServices
{
    public partial class BuildService
    {
        /// <summary>
        /// Finds all of the specfied tags in the specified text 
        /// </summary>
        /// <returns>A list of Tags found in the specified text</returns>
        /// <param name="searchText">The text to search for Tags</param>
        /// <param name="tagName">The name of the Tags to search for</param>
        /// <param name="hasContent">True when there is a Start and End tag</param>
        /// <param name="tags">The </param>
        private List<Tag> ExtractTags(string searchText, string tagName, bool hasContent)
        {
            List<Tag> tags = new List<Tag>(); 
            int startPosition = 0;

            while (true)
            {
                Tag tag = new Tag
                {
                    Name = tagName,
                    SearchText = searchText,
                    HasContent = hasContent
                };

                // See if we can find the tag
                this.ExtractTag(tag, startPosition);
                if (tag.StartTagPosition == null) break; // No more tags

                // If there is a start and end Tag we'll need to extract the Tag contents
                if (tag.HasContent)
                {
                    this.ExtractTagContent(tag);
                }

                // Successful
                tags.Add(tag);

                // Start looking AFTER the last tag we found
                startPosition = tag.HasContent ? tag.EndTagPosition.EndPosition : tag.StartTagPosition.EndPosition;
            }

            return tags;
        }

        /// <summary>
        /// Attempts to extract the the next tag, starting at the specified position
        /// </summary>
        /// <returns>True if a Tag was found</returns>
        /// <param name="tag">The Tag to find</param>
        /// <param name="currentPosition">The position to start looking at</param>
        private void ExtractTag(Tag tag, int currentPosition)
        {
            // Find the tag
            TagPosition startTagPosition = this.FindTagPosition(tag.SearchText, currentPosition, tag.Name + (tag.HasContent ? "_START" : ""));
            if (startTagPosition == null) return; // Tag not found

            tag.StartTagPosition = startTagPosition;

            // Extract the Tag values
            this.GetTagValues(tag);
        }

        /// <summary>
        /// Looks for the location of the next tag, starting at the specified position 
        /// </summary>
        /// <returns>The position details of the next tag</returns>
        /// <param name="searchText">The text to search for tags</param>
        /// <param name="startPosition">The position within the text to start search for tags</param></param>
        /// <param name="tagName">The name of the tag to search for</param>
        private TagPosition FindTagPosition(string searchText, int startPosition, string tagName)
        {
            TagPosition tagPosition = null;

            string startTagText = "<!-- " + tagName + " ";
            int tagStartStartIndex = searchText.IndexOf(startTagText, startPosition, StringComparison.OrdinalIgnoreCase);
            if (tagStartStartIndex == -1) return null; // tag not found

            string startTagEndText = " -->";
            int tagStartEndIndex = searchText.IndexOf(startTagEndText, tagStartStartIndex, StringComparison.OrdinalIgnoreCase);
            if (tagStartEndIndex == -1) this.BuildHandler.SignalBuildError(tagName + " tag not closed");

            tagPosition = new TagPosition();
            tagPosition.StartPosition = tagStartStartIndex;
            tagPosition.StartEndPosition = tagStartStartIndex + startTagText.Length;
            tagPosition.EndPosition = tagStartEndIndex + startTagEndText.Length;

            return tagPosition;
        }

        /// <summary>
        /// Locate the end tag and extract everything inbetween the start and end tags 
        /// </summary>
        /// <param name="tag">The target tag</param>
        private void ExtractTagContent(Tag tag)
        {
            // Find the end tag
            string endTagName = tag.Name + "_END";

            TagPosition endTagPosition = this.FindTagPosition(tag.SearchText, tag.StartTagPosition.EndPosition, endTagName);

            if (endTagPosition == null) this.BuildHandler.SignalBuildError("Missing end tag");

            tag.EndTagPosition = endTagPosition;

            // Get all the text inbetween the start and end tags - that's the content
            int contentLength = tag.EndTagPosition.StartPosition - tag.StartTagPosition.EndPosition;
            tag.Contents = tag.SearchText.Substring(tag.StartTagPosition.EndPosition, contentLength);
        }

        /// <summary>
        /// Extract the Tag properties
        /// </summary>
        /// <param name="tag">The target tag</param>
        private void GetTagValues(Tag tag)
        {
            // Get tag key value pairs
            int position = tag.StartTagPosition.StartEndPosition;
            this.GetNextKeyValuePair(tag, ref position);
        }

        /// <summary>
        /// Extract the next key value pair from the specified Tag at the specified starting position
        /// </summary>
        /// <param name="tag">The Tag containing the key value pairs</param>
        /// <param name="position">The position to start searching from</param>
        private void GetNextKeyValuePair(Tag tag, ref int position)
        {
            while (true)
            {
                // We're potentially at the start of the next value or key/value pair
                // We need to figure out if we've got to the end of the tag or if there's a value or a key/value pair
                int nextEqualsIndex = tag.SearchText.IndexOf("=", position, StringComparison.OrdinalIgnoreCase);

                // Any more values or value key pairs?
                if (nextEqualsIndex == -1 || nextEqualsIndex > tag.StartTagPosition.EndPosition)
                {
                    // Finished processing values
                    break;
                }

                if (nextEqualsIndex != -1)
                {
                    // Next character is an equals.  It has to be a key value pair
                    this.GetTagKeyValuePair(tag, nextEqualsIndex, ref position);
                }
            }
        }

        /// <summary>
        /// Extract the specified key value pair
        /// </summary>
        /// <param name="tag">The Tag containing the key value pair</param>
        /// <param name="nextEqualsIndex">The location of the equals that seperates the key and value</param>
        /// <param name="position">The position of the key value pair</param>
        private void GetTagKeyValuePair(Tag tag, int nextEqualsIndex, ref int position)
        {
            // Get the key
            int keyLength = nextEqualsIndex - position;
            string key = tag.SearchText.Substring(position, keyLength).Trim();

            // Get the value
            int firstSingleQuoteIndex = tag.SearchText.IndexOf("'", nextEqualsIndex, StringComparison.OrdinalIgnoreCase);
            if (firstSingleQuoteIndex == -1) this.BuildHandler.SignalBuildError("Missing first single quote");

            int secondSingleQuoteIndex = tag.SearchText.IndexOf("'", firstSingleQuoteIndex + 1, StringComparison.OrdinalIgnoreCase);
            if (secondSingleQuoteIndex == -1) this.BuildHandler.SignalBuildError("Missing second single quote");

            int valueLength = secondSingleQuoteIndex - firstSingleQuoteIndex - 1;
            string value = tag.SearchText.Substring(firstSingleQuoteIndex + 1, valueLength);

            // Add it to the tags values
            tag.Properties.Add(key, value);

            // Move the position along
            position = secondSingleQuoteIndex + 1;
        }
    }
}
