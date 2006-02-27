namespace Jayrock
{
    #region Imports

    using System;
    using System.Reflection;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
	public class TestInternetDate
	{
        [ Test ]
        public void CannotBeCreated()
        {
            try
            {
                Activator.CreateInstance(typeof(InternetDate), true);
                Assert.Fail();
            }
            catch (TargetInvocationException e)
            {
                Assert.IsTrue(e.InnerException is NotSupportedException);
            }
        }

        [ Test ]
        public void ParseMinimal()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:00 GMT", "26 Feb 2006 21:52 GMT");
        }

        [ Test ]
        public void ParseWithSeconds()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "26 Feb 2006 21:52:20 GMT");
        }

        [ Test ]
        public void ParseWithDayOfWeekNoSeconds()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:00 GMT", "Sun, 26 Feb 2006 21:52 GMT");
        }

        [ Test ]
        public void ParseComplete()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 21:52:20 GMT");
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotParseNull()
        {
            InternetDate.Parse(null);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotParseEmpty()
        {
            InternetDate.Parse(string.Empty);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotParseBelowMinimumLength()
        {
            InternetDate.Parse("1234567890");
        }

        [ Test ]
        public void ParseCET()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 22:52:20 +0100");
        }

        [ Test ]
        public void ParseIndiaTime()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Mon, 27 Feb 2006 03:22:20 +0530");
        }

        [ Test ]
        public void ParsePST()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 13:52:20 -0800");
        }

        [ Test ]
        public void ParseNorthAmericanZones()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 17:52:20 EDT");
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 16:52:20 EST");
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 16:52:20 CDT");
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 15:52:20 CST");
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 15:52:20 MDT");
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 14:52:20 MST");
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 14:52:20 PDT");
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 13:52:20 PST");
        }

        [ Test ]
        public void ParseIndiaTimeAssumingPositiveOffset()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Mon, 27 Feb 2006 03:22:20 0530");
        }

        [ Test ]
        public void Roundtrip()
        {
            AssertParse("Sun, 26 Feb 2006 21:52:20 GMT", "Sun, 26 Feb 2006 21:52:20 GMT");
        }

        [ Test, ExpectedException(typeof(FormatException)) ]
        public void EmptyTimeZone()
        {
            InternetDate.Parse("Sun, 26 Feb 2006 21:52:20 ");
        }

        [ Test, ExpectedException(typeof(FormatException)) ]
        public void MissingTimeZone()
        {
            InternetDate.Parse("Sun, 26 Feb 2006 21:52:20");
        }

        [ Test, ExpectedException(typeof(FormatException)) ]
        public void BadLocalDifferential()
        {
            InternetDate.Parse("Sun, 26 Feb 2006 21:52:20 HHMM");
        }

        [ Test, ExpectedException(typeof(FormatException)) ]
        public void WrongLocalDifferentialLength()
        {
            InternetDate.Parse("Sun, 26 Feb 2006 22:52:20 100");
        }

        [ Test, ExpectedException(typeof(FormatException)) ]
        public void NoTimeZoneDelimiter()
        {
            InternetDate.Parse("Sun,26-Feb-2006T22:52:20+0100");
        }

        private static void AssertParse(string expected, string input)
        {
            DateTime time = InternetDate.Parse(input);
            Assert.AreEqual(expected, time.ToUniversalTime().ToString("r"), "Input = " + input);
        }
	}
}
