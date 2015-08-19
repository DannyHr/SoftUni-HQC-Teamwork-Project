namespace RestSharp.Tests
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web.Script.Serialization;
    using RestSharp.Extensions;
    using Xunit;

    public class StringExtensionsTests
    {
        [Fact]
        public void UrlEncode_Should_Throw_An_Exception_On_Null_Input()
        {
            const string NullString = null;

            Assert.Throws<System.ArgumentNullException>(delegate { NullString.UrlEncode(); });
        }

        [Fact]
        public void UrlEncode_Returns_Correct_Length_When_Less_Than_Limit()
        {
            const int NumLessThanLimit = 32766;
            string stringWithLimitLength = new string('.', NumLessThanLimit);

            Assert.True(stringWithLimitLength.UrlEncode().Length == NumLessThanLimit);
        }

        [Fact]
        public void HasValue_Returns_False_On_Null_String()
        {
            const string NullString = null;

            Assert.False(StringExtensions.HasValue(NullString));
        }

        [Fact]
        public void RemoveUnderscoresAndDashes_Removes_Underscore_And_Dashes_From_String()
        {
            const string InputString = "This _is -_input--- -string with dashes____ and underscore_---_-.";
            const string ExpectedResult = "This is input string with dashes and underscore.";

            var actualResult = StringExtensions.RemoveUnderscoresAndDashes(InputString);

            Assert.Equal(ExpectedResult, actualResult);
        }

        [Fact]
        public void ParseJsonDate_Parses_JSonDate_To_DateTime()
        {
            const string JSonDate = @"""\/Date(1309421746929)\/""";
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            DateTime expectedDateTime = jsonSerializer.Deserialize<DateTime>(@"""\/Date(1309421746929)\/""");

            var culture = CultureInfo.InvariantCulture;
            var actualDateTime = StringExtensions.ParseJsonDate(JSonDate, culture);

            Assert.Equal(expectedDateTime, actualDateTime);
        }

        [Fact]
        public void RemoveSurroundingQuotes_Should_Remove_Leading_And_Trailing_Quotes()
        {
            const string InputString = "\"This is input string surrounded by quotes\"";
            const string ExpectedString = "This is input string surrounded by quotes";

            string actualResult = StringExtensions.RemoveSurroundingQuotes(InputString);

            Assert.Equal(ExpectedString, actualResult);
        }

        [Fact]
        public void RemoveSurroundingQuotes_Should_Not_Remove_Quotes_If_Doesnt_End_With_Quote()
        {
            const string InputString = "\"This is input string only starting with quotes and not ending with such";
            const string ExpectedString = "\"This is input string only starting with quotes and not ending with such";

            string actualResult = StringExtensions.RemoveSurroundingQuotes(InputString);

            Assert.Equal(ExpectedString, actualResult);
        }

        [Fact]
        public void ToPascalCase_Converts_String_To_PascalCase()
        {
            const string InputString = "This is__ string containing _underscores_._";
            const string ExpectedString = "ThisIsStringContainingUnderscores.";

            string actualResult = StringExtensions.ToPascalCase(InputString, true, CultureInfo.InvariantCulture);

            Assert.Equal(ExpectedString, actualResult);
        }

        [Fact]
        public void ToPascalCase_Converts_All_Upper_Case_String_To_PascalCase()
        {
            const string InputString = "THIS IS__ STRING WITH _ALL_ WORDS UPPERCASED ._";
            const string ExpectedString = "ThisIsStringWithAllWordsUppercased.";

            string actualResult = StringExtensions.ToPascalCase(InputString, true, CultureInfo.InvariantCulture);

            Assert.Equal(ExpectedString, actualResult);
        }

        [Fact]
        public void MakeInitialLowerCase_Converts_First_Letter_To_Lower_Case()
        {
            const string InputString = "UPPERCASE";
            const string ExpectedString = "uPPERCASE";

            string actualResult = StringExtensions.MakeInitialLowerCase(InputString);

            Assert.Equal(ExpectedString, actualResult);
        }

        [Fact]
        public void IsUpperCase_Should_Return_True_On_Whole_String_Upper_Case()
        {
            const string InputString = "UPPERCASE";

            var actualResult = StringExtensions.IsUpperCase(InputString);

            Assert.True(actualResult);
        }

        [Fact]
        public void IsUpperCase_Should_Return_False_On_Containing_Lower_Case()
        {
            const string InputString = "notUPPERCASE";

            var actualResult = StringExtensions.IsUpperCase(InputString);

            Assert.False(actualResult);
        }

        [Fact]
        public void AddUnderscores_Should_Place_Underscores_On_PascalCased_Word()
        {
            const string InputString = "PascalCasedWordToPutUnderscoresOn";
            const string ExpectedString = "Pascal_Cased_Word_To_Put_Underscores_On";

            var actualResult = StringExtensions.AddUnderscores(InputString);

            Assert.Equal(ExpectedString, actualResult);
        }

        [Fact]
        public void AddDashes_Should_Place_Dashes_On_PascalCased_Word()
        {
            const string InputString = "PascalCasedWordToPutUnderscoresOn";
            const string ExpectedString = "Pascal-Cased-Word-To-Put-Underscores-On";

            var actualResult = StringExtensions.AddDashes(InputString);

            Assert.Equal(ExpectedString, actualResult);
        }

        [Fact]
        public void AddSpaces_Should_Place_Spaces_On_PascalCased_Word()
        {
            const string InputString = "PascalCasedWordToPutUnderscoresOn";
            const string ExpectedString = "Pascal Cased Word To Put Underscores On";

            var actualResult = StringExtensions.AddSpaces(InputString);

            Assert.Equal(ExpectedString, actualResult);
        }

        [Fact]
        public void AddUnderscorePrefix_Should_Add_Underscore_Prefix_On_PascalCased_Word()
        {
            const string InputString = "PascalCasedWordToPutUnderscoresOn";
            const string ExpectedString = "_PascalCasedWordToPutUnderscoresOn";

            var actualResult = StringExtensions.AddUnderscorePrefix(InputString);

            Assert.Equal(ExpectedString, actualResult);
        }

        [Fact]
        public void GetNameVariants_Should_Return_Possible_Names()
        {
            const string InputString = "my name";
            const string ExpectedStringLowerCase = "my name";
            const string ExpectedStringCamelCase = "myName";
            const string ExpectedStringCamelCaseUnderscorePrefix = "_myName";
            const string ExpectedStringLowerCaseUnderscorePrefix = "_my name";
            const string ExpectedStringLowerCaseWithDashes = "my-name";

            var nameVariants = StringExtensions.GetNameVariants(InputString, CultureInfo.InvariantCulture);

            Assert.True(nameVariants.Contains(ExpectedStringLowerCase));
            Assert.True(nameVariants.Contains(ExpectedStringCamelCase));
            Assert.True(nameVariants.Contains(ExpectedStringCamelCaseUnderscorePrefix));
            Assert.True(nameVariants.Contains(ExpectedStringLowerCaseUnderscorePrefix));
            Assert.True(nameVariants.Contains(ExpectedStringLowerCaseWithDashes));
        }

        [Fact]
        public void Matches_Should_True_On_Matching_Regex_In_String()
        {
            const string InputString = "Input String That Have To Match Provided Regex Pattern";
            const string Pattern = @"\b[A-Z]\w+\b";

            var actualResult = StringExtensions.Matches(InputString, Pattern);

            Assert.True(actualResult);
        }
    }
}
