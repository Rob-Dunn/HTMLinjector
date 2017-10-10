using NUnit.Framework;
using HTMLinjectorServices;
using System.Collections.Generic;

namespace HTMLinjector_NUnit
{
    [TestFixture()]
    public class ExtractTagsPropertiesTests
    {
        [Test()]
        public void OneTagWithOnePropertyMissingQuotes()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name property0= --> and some more text";
            string tagName = "a_tag_name";

            BuildException buildException = Assert.Throws<BuildException>(() => { tagServices.ExtractTags(searchText, tagName, false); });

            // Check result
            Assert.AreEqual("Missing first single quote", buildException.Message);
        }

        [Test()]
        public void OneTagWithOnePropertyMissingSecondQuote()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name property0=' --> and some more text";
            string tagName = "a_tag_name";

            BuildException buildException = Assert.Throws<BuildException>(() => { tagServices.ExtractTags(searchText, tagName, false); });

            // Check result
            Assert.AreEqual("Missing second single quote", buildException.Message);
        }

        [Test()]
        public void OneTagWithOneEmptyProperty()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name property0='' --> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Dictionary<string, string> expectedProperties = new Dictionary<string, string>() { { "property0", "" } };
            Tag expectedTag = TagUtilities.CreateTestTag(tagName, searchText, 15, 31, 47, null, null, null, null, expectedProperties);

            Assert.True(1 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag, actualTags[0]);
        }

        [Test()]
        public void OneTagWithOneProperty()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name property0='some value' --> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Dictionary<string, string> expectedProperties = new Dictionary<string, string>() { { "property0", "some value" } };
            Tag expectedTag = TagUtilities.CreateTestTag(tagName, searchText, 15, 31, 57, null, null, null, null, expectedProperties);

            Assert.True(1 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag, actualTags[0]);
        }

        [Test()]
        public void OneTagWithTwoPropertiesNoSpace()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name property0='some value'property1='another value' --> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Dictionary<string, string> expectedProperties = new Dictionary<string, string>() { { "property0", "some value" }, { "property1", "another value" } };
            Tag expectedTag = TagUtilities.CreateTestTag(tagName, searchText, 15, 31, 82, null, null, null, null, expectedProperties);

            Assert.True(1 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag, actualTags[0]);
        }

        [Test()]
        public void OneTagWithTwoProperties()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name property0='some value' property1='another value' --> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Dictionary<string, string> expectedProperties = new Dictionary<string, string>() { { "property0", "some value" }, { "property1", "another value" } };
            Tag expectedTag = TagUtilities.CreateTestTag(tagName, searchText, 15, 31, 83, null, null, null, null, expectedProperties);

            Assert.True(1 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag, actualTags[0]);
        }

        [Test()]
        public void TwoTagsOnePropertyTwoProperties()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                                                                                                                       111111111111111111111111111111111111111111111111111
            //                             111111111122222222223333333333444444444455555555556666666666777777777788888888889999999999000000000011111111112222222222333333333344444444445
            //                   0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
            string searchText = "some test text <!-- a_tag_name tag0property0='some value' --> <!-- a_tag_name tag1Property0='another value' tag1Property1='yet another value' --> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Dictionary<string, string> expectedTag0Properties = new Dictionary<string, string>() { { "tag0property0", "some value" } };
            Tag expectedTag0 = TagUtilities.CreateTestTag(tagName, searchText, 15, 31, 61, null, null, null, null, expectedTag0Properties);

            Dictionary<string, string> expectedTag1Properties = new Dictionary<string, string>() { { "tag1Property0", "another value" }, { "tag1Property1", "yet another value" } };
            Tag expectedTag1 = TagUtilities.CreateTestTag(tagName, searchText, 62, 78, 145, null, null, null, null, expectedTag1Properties);

            Assert.True(2 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag0, actualTags[0]);
            TagUtilities.AreTagsSame(expectedTag1, actualTags[1]);
        }

        [Test()]
        public void TwoTagsFirstWithContentAndPropertySecondTwoProperties()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                                                                                                                       11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111112222222222222222222222222222222
            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999999999900000000001111111111222222222233333333334444444444555555555566666666667777777777888888888899999999990000000000111111111122222222223
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
            string searchText = "some test text <!-- a_tag_name_START tag0property0='some value' -->some content<!-- a_tag_name_END -->  <!-- a_tag_name_START tag1Property0='another value' tag1Property1='yet another value' --> more content <!-- a_tag_name_END --> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, true);

            // Check result
            Dictionary<string, string> expectedTag0Properties = new Dictionary<string, string>() { { "tag0property0", "some value" } };
            Tag expectedTag0 = TagUtilities.CreateTestTag(tagName, searchText, 15, 37, 67, 79, 99, 102, "some content", expectedTag0Properties);

            Dictionary<string, string> expectedTag1Properties = new Dictionary<string, string>() { { "tag1Property0", "another value" }, { "tag1Property1", "yet another value" } };
            Tag expectedTag1 = TagUtilities.CreateTestTag(tagName, searchText, 104, 126, 193, 207, 227, 230, " more content ", expectedTag1Properties);

            Assert.True(2 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag0, actualTags[0]);
            TagUtilities.AreTagsSame(expectedTag1, actualTags[1]);
        }
    }
}