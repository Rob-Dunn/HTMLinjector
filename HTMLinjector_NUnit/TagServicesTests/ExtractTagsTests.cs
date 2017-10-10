using NUnit.Framework;
using HTMLinjectorServices;
using System.Collections.Generic;

namespace HTMLinjector_NUnit
{
    [TestFixture()]
    public class ExtractTagsTests
    {
        [Test()]
        public void EmptyString()
        {
            TagServices tagServices = new TagServices(new BuildHandler());
            List<Tag> actualTags = tagServices.ExtractTags("", "a_tag_name", false);

            // Check result
            Assert.IsEmpty(actualTags);
        }

        [Test()]
        public void NoTags()
        {
            TagServices tagServices = new TagServices(new BuildHandler());
            List<Tag> actualTags = tagServices.ExtractTags("some test text", "a_tag_name", false);

            // Check result
            Assert.IsEmpty(actualTags);
        }

        [Test()]
        public void OneTagNoSpaceBetweenCommentAndTagName()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             1111111111222222222233333333334444444444555
            //                   01234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!--a_tag_name--> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Assert.True(0 == actualTags.Count);
        }

        [Test()]
        public void OneTagMissingCommentEnd()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             1111111111222222222233333333334444444444555
            //                   01234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name and some more text";
            string tagName = "a_tag_name";

            BuildException buildException = Assert.Throws<BuildException>(() => { tagServices.ExtractTags(searchText, tagName, false); });

            // Check result
            Assert.AreEqual(tagName + " tag not closed", buildException.Message);
        }

        [Test()]
        public void OneTag()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             1111111111222222222233333333334444444444555
            //                   01234567890123456789012345678901234567890123456789012
            string searchText = "some test text <!-- a_tag_name --> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Tag expectedTag = TagUtilities.CreateTestTag(tagName, searchText, 15, 31, 34);

            Assert.True(1 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag, actualTags[0]);
        }

        [Test()]
        public void OneTagStart()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             1111111111222222222233333333334444444444555
            //                   01234567890123456789012345678901234567890123456789012
            string searchText = "<!-- a_tag_name --> some test text and some more text\", \"a_tag_name";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Tag expectedTag = TagUtilities.CreateTestTag(tagName, searchText, 0, 16, 19);

            Assert.True(1 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag, actualTags[0]);
        }

        [Test()]
        public void OneTagEnd()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             1111111111222222222233333333334444444444555
            //                   01234567890123456789012345678901234567890123456789012
            string searchText = "some test text and some more text<!-- a_tag_name -->\", \"a_tag_name";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Tag expectedTag = TagUtilities.CreateTestTag(tagName, searchText, 33, 49, 52);

            Assert.True(1 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag, actualTags[0]);
        }

        [Test()]
        public void TwoTags()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             1111111111222222222233333333334444444444555555555566666666667
            //                   01234567890123456789012345678901234567890123456789012345678901234567890
            string searchText = "some test text <!-- a_tag_name --><!-- a_tag_name --> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Tag expectedTag0 = TagUtilities.CreateTestTag(tagName, searchText, 15, 31, 34);
            Tag expectedTag1 = TagUtilities.CreateTestTag(tagName, searchText, 34, 50, 53);

            Assert.True(2 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag0, actualTags[0]);
            TagUtilities.AreTagsSame(expectedTag1, actualTags[1]);
        }

        [Test()]
        public void TwoTagsStartAndEnd()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             1111111111222222222233333333334444444444555555555566666666667
            //                   01234567890123456789012345678901234567890123456789012345678901234567890
            string searchText = "<!-- a_tag_name -->some test text and some more text<!-- a_tag_name -->";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Tag expectedTag0 = TagUtilities.CreateTestTag(tagName, searchText, 0, 16, 19);
            Tag expectedTag1 = TagUtilities.CreateTestTag(tagName, searchText, 52, 68, 71);

            Assert.True(2 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag0, actualTags[0]);
            TagUtilities.AreTagsSame(expectedTag1, actualTags[1]);
        }

        [Test()]
        public void ThreeTags()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "some search text <!-- a_tag_name --><!-- a_tag_name --><!-- a_tag_name --> and some more text";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Tag expectedTag0 = TagUtilities.CreateTestTag(tagName, searchText, 17, 33, 36);
            Tag expectedTag1 = TagUtilities.CreateTestTag(tagName, searchText, 36, 52, 55);
            Tag expectedTag2 = TagUtilities.CreateTestTag(tagName, searchText, 55, 71, 74);

            Assert.True(3 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag0, actualTags[0]);
            TagUtilities.AreTagsSame(expectedTag1, actualTags[1]);
            TagUtilities.AreTagsSame(expectedTag2, actualTags[2]);
        }

        [Test()]
        public void ThreeTagsStartMiddleEnd()
        {
            TagServices tagServices = new TagServices(new BuildHandler());

            //                             11111111112222222222333333333344444444445555555555666666666677777777778888888888999
            //                   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012
            string searchText = "<!-- a_tag_name -->some search text<!-- a_tag_name --> and some more text<!-- a_tag_name -->";
            string tagName = "a_tag_name";

            List<Tag> actualTags = tagServices.ExtractTags(searchText, tagName, false);

            // Check result
            Tag expectedTag0 = TagUtilities.CreateTestTag(tagName, searchText, 0, 16, 19);
            Tag expectedTag1 = TagUtilities.CreateTestTag(tagName, searchText, 35, 51, 54);
            Tag expectedTag2 = TagUtilities.CreateTestTag(tagName, searchText, 73, 89, 92);

            Assert.True(3 == actualTags.Count);
            TagUtilities.AreTagsSame(expectedTag0, actualTags[0]);
            TagUtilities.AreTagsSame(expectedTag1, actualTags[1]);
            TagUtilities.AreTagsSame(expectedTag2, actualTags[2]);
        }
    }
}
