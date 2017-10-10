using NUnit.Framework;
using HTMLinjectorServices;
using System.Collections.Generic;

namespace HTMLinjector_NUnit
{
    [TestFixture()]
    public class ExtractTagsWithContentTests
    {
        [Test()]
        public void EmptyString()
        {
            TagServices tagServices = new TagServices(new BuildHandler());
            List<Tag> actualTags = tagServices.ExtractTags("", "a_tag_name", true);

            // Check result
            Assert.IsEmpty(actualTags);
        }

        [Test()]
        public void NoTags()
        {
            TagServices tagServices = new TagServices(new BuildHandler());
            List<Tag> actualTags = tagServices.ExtractTags("some test text", "a_tag_name", true);

            // Check result
            Assert.IsEmpty(actualTags);
        }

        [Test()]
        public void OneTagNotClosed()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name_START -->some content<!-- a_tag_name_ZEND --> and some more text";
            string tagName = "a_tag_name";

            BuildException buildException = Assert.Throws<BuildException>(() => { tagServices.ExtractTags(searchText, tagName, true); });

            // Check result
            Assert.AreEqual("Missing end tag", buildException.Message);
        }

        [Test()]
        public void OneTagEmptyContent()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name_START --><!-- a_tag_name_END --> and some more text";
            string tagName = "a_tag_name";
            string content = "";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, true);

            // Check result
            Tag expectedTag = TagUtilities.CreateTestTag(tagName, searchText, 15, 37, 40, 40, 60, 63, content);

            Assert.True(1 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag, actualTags[0]);
        }

        [Test()]
        public void OneTagWithContent()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name_START -->some content<!-- a_tag_name_END --> and some more text";
            string tagName = "a_tag_name";
            string content = "some content";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, true);

            // Check result
            Tag expectedTag = TagUtilities.CreateTestTag(tagName, searchText, 15, 37, 40, 52, 72, 75, content);

            Assert.True(1 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag, actualTags[0]);
        }

        [Test()]
        public void TwoTagsWithContent()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                                                                                                                       111111111111111111111111111111111111111111111111111
            //                             111111111122222222223333333333444444444455555555556666666666777777777788888888889999999999000000000011111111112222222222333333333344444444445
            //                   0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
            string searchText = "some test text <!-- a_tag_name_START -->some content<!-- a_tag_name_END --> text <!-- a_tag_name_START -->more content<!-- a_tag_name_END --> more text";
            string tagName = "a_tag_name";
            string tag0Content = "some content";
            string tag1Content = "more content";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, true);

            // Check result
            Tag expectedTag0 = TagUtilities.CreateTestTag(tagName, searchText, 15, 37, 40, 52, 72, 75, tag0Content);
            Tag expectedTag1 = TagUtilities.CreateTestTag(tagName, searchText, 81, 103, 106, 118, 138, 141, tag1Content);

            Assert.True(2 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag0, actualTags[0]);
            TagUtilities.AreTagsSame(expectedTag1, actualTags[1]);
        }
    }
}
