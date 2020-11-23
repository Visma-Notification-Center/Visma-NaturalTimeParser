using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Pathoschild.NaturalTimeParser.Parser;
using Pathoschild.NaturalTimeParser.Parser.Plugins;
using Pathoschild.NaturalTimeParser.Parser.Tokenization;

namespace Pathoschild.NaturalTimeParser.Tests.Plugins
{
	/// <summary>Asserts that the <see cref="ArithmeticTimePlugin"/> supports all valid scenarios.</summary>
	[TestFixture]
	public class ArithmeticTimePluginTests
	{
		/*********
		** Unit tests
		*********/
		/***
		** Tokenize
		***/
		[Test(Description = "Assert that standard GNU relative time units are correctly tokenized.")]
		[TestCase("42 sec", ExpectedResult = "[Seconds:42]")]
		[TestCase("42 secs", ExpectedResult = "[Seconds:42]")]
		[TestCase("42 seconds", ExpectedResult = "[Seconds:42]")]
		[TestCase("42 seconds", ExpectedResult = "[Seconds:42]")]
		[TestCase("42 min", ExpectedResult = "[Minutes:42]")]
		[TestCase("42 mins", ExpectedResult = "[Minutes:42]")]
		[TestCase("42 minute", ExpectedResult = "[Minutes:42]")]
		[TestCase("42 minutes", ExpectedResult = "[Minutes:42]")]
		[TestCase("42 hour", ExpectedResult = "[Hours:42]")]
		[TestCase("42 hours", ExpectedResult = "[Hours:42]")]
		[TestCase("42 day", ExpectedResult = "[Days:42]")]
		[TestCase("42 days", ExpectedResult = "[Days:42]")]
		[TestCase("42 week", ExpectedResult = "[Weeks:42]")]
		[TestCase("42 weeks", ExpectedResult = "[Weeks:42]")]
		[TestCase("42 fortnight", ExpectedResult = "[Fortnights:42]")]
		[TestCase("42 fortnights", ExpectedResult = "[Fortnights:42]")]
		[TestCase("42 month", ExpectedResult = "[Months:42]")]
		[TestCase("42 months", ExpectedResult = "[Months:42]")]
		[TestCase("42 year", ExpectedResult = "[Years:42]")]
		[TestCase("42 years", ExpectedResult = "[Years:42]")]
		public string Tokenize_SupportsStandardUnits(string format)
		{
			return this.Tokenize(new ArithmeticTimePlugin(), format);
		}

		[Test(Description = "Assert that all the standard GNU formats are correctly tokenized. This includes keywords like 'ago', and optional signs and multipliers.")]
		[TestCase("years", ExpectedResult = "[Years:1]")]
		[TestCase("+years", ExpectedResult = "[Years:1]")]
		[TestCase("-years", ExpectedResult = "[Years:-1]")]
		[TestCase("years ago", ExpectedResult = "[Years:-1]")]
		[TestCase("+years ago", ExpectedResult = "[Years:-1]")]
		[TestCase("-years ago", ExpectedResult = "[Years:1]")]
		[TestCase("15 years", ExpectedResult = "[Years:15]")]
		[TestCase("+15 years", ExpectedResult = "[Years:15]")]
		[TestCase("-15 years", ExpectedResult = "[Years:-15]")]
		[TestCase("15 years ago", ExpectedResult = "[Years:-15]")]
		[TestCase("+15 years ago", ExpectedResult = "[Years:-15]")]
		[TestCase("-15 years ago", ExpectedResult = "[Years:15]")]
		[TestCase("    -15     years    ago", ExpectedResult = "[Years:15]")]
		public string Tokenize_SupportsStandardFormats(string format)
		{
			return this.Tokenize(new ArithmeticTimePlugin(), format);
		}

		[Test(Description = "Assert that invalid formats are ignored by the plugin. It should return no tokens since it could not parse them.")]
		[TestCase("invalid format", ExpectedResult = "")]
		[TestCase("15 eggs ago", ExpectedResult = "")]
		[TestCase("four eggs ago", ExpectedResult = "")]
		[TestCase("four eggs ago 15 days ago", ExpectedResult = "")]
		public string Tokenize_IgnoresInvalidFormats(string format)
		{
			return this.Tokenize(new ArithmeticTimePlugin(), format);
		}

		[TestCase(null)]
		public void Tokenize_IgnoresInvalidFormatsApplyNull_ShouldThrow(string format)
		{
			Assert.That(() => this.Tokenize(new ArithmeticTimePlugin(), format), Throws.TypeOf<ArgumentNullException>());
		}

		[Test(Description = "Assert that time units are case-insensitive (so '5 months' is identical to '5 MONTHS').")]
		[TestCase("42 FORTNIGHTS", ExpectedResult = "[Fortnights:42]")]
		[TestCase("42 MoNth", ExpectedResult = "[Months:42]")]
		public string Tokenize_IsCaseInsensitive(string format)
		{
			return this.Tokenize(new ArithmeticTimePlugin(), format);
		}

		[Test(Description = "Assert that time units are correctly tokenized when they're chained together.")]
		[TestCase("15 years 3 months 2 hours", ExpectedResult = "[Years:15][Months:3][Hours:2]")]
		[TestCase("15 years -12 months 2 fortnights 3 weeks -17 days ago -hours 2 minutes secs", ExpectedResult = "[Years:15][Months:-12][Fortnights:2][Weeks:3][Days:17][Hours:-1][Minutes:2][Seconds:1]")]
		[TestCase("15 years -months +months ago -2 fortnights 3 weeks -17 days ago -hours 2 minutes secs", ExpectedResult = "[Years:15][Months:-1][Months:-1][Fortnights:-2][Weeks:3][Days:17][Hours:-1][Minutes:2][Seconds:1]")]
		public string Tokenize_CanChainUnits(string format)
		{
			return this.Tokenize(new ArithmeticTimePlugin(), format);
		}

		[Test(Description = "Assert that the units can be localized.")]
		[TestCase("42 secondes", ExpectedResult = "[Seconds:42]")]
		[TestCase("42 minutes", ExpectedResult = "[Minutes:42]")]
		[TestCase("42 heures", ExpectedResult = "[Hours:42]")]
		[TestCase("42 jours", ExpectedResult = "[Days:42]")]
		[TestCase("42 semaines", ExpectedResult = "[Weeks:42]")]
		[TestCase("42 mois", ExpectedResult = "[Months:42]")]
		[TestCase("42 ans", ExpectedResult = "[Years:42]")]
		[TestCase("42 années", ExpectedResult = "[Years:42]")]
		public string Tokenize_CanLocalizeUnits(string format)
		{
			ArithmeticTimePlugin plugin = new ArithmeticTimePlugin();
			plugin.SupportedUnits["seconde"] = RelativeTimeUnit.Seconds;
			plugin.SupportedUnits["heure"] = RelativeTimeUnit.Hours;
			plugin.SupportedUnits["jour"] = RelativeTimeUnit.Days;
			plugin.SupportedUnits["semaine"] = RelativeTimeUnit.Weeks;
			plugin.SupportedUnits["mois"] = RelativeTimeUnit.Months;
			plugin.SupportedUnits["an"] = RelativeTimeUnit.Years;
			plugin.SupportedUnits["année"] = RelativeTimeUnit.Years;
			return this.Tokenize(plugin, format);
		}

		/***
		** Apply
		***/
		[Test(Description = "Assert that tokens for supported time units are correctly applied to a date.")]
		[TestCase(42, RelativeTimeUnit.Seconds, ExpectedResult = "2000-01-01T00:00:42")]
		[TestCase(42, RelativeTimeUnit.Minutes, ExpectedResult = "2000-01-01T00:42:00")]
		[TestCase(42, RelativeTimeUnit.Hours, ExpectedResult = "2000-01-02T18:00:00")]
		[TestCase(42, RelativeTimeUnit.Days, ExpectedResult = "2000-02-12T00:00:00")]
		[TestCase(42, RelativeTimeUnit.Weeks, ExpectedResult = "2000-10-21T00:00:00")]
		[TestCase(42, RelativeTimeUnit.Fortnights, ExpectedResult = "2001-08-11T00:00:00")]
		[TestCase(42, RelativeTimeUnit.Months, ExpectedResult = "2003-07-01T00:00:00")]
		[TestCase(42, RelativeTimeUnit.Years, ExpectedResult = "2042-01-01T00:00:00")]
		public string Apply_SupportsStandardUnits(int value, RelativeTimeUnit unit)
		{
			return this.TryApply(value, unit);
		}

		[Test(Description = "Assert that tokens for negative multipliers of supported time units are correctly applied to a date.")]
		[TestCase(-42, RelativeTimeUnit.Seconds, ExpectedResult = "1999-12-31T23:59:18")]
		[TestCase(-42, RelativeTimeUnit.Minutes, ExpectedResult = "1999-12-31T23:18:00")]
		[TestCase(-42, RelativeTimeUnit.Hours, ExpectedResult = "1999-12-30T06:00:00")]
		[TestCase(-42, RelativeTimeUnit.Days, ExpectedResult = "1999-11-20T00:00:00")]
		[TestCase(-42, RelativeTimeUnit.Weeks, ExpectedResult = "1999-03-13T00:00:00")]
		[TestCase(-42, RelativeTimeUnit.Fortnights, ExpectedResult = "1998-05-23T00:00:00")]
		[TestCase(-42, RelativeTimeUnit.Months, ExpectedResult = "1996-07-01T00:00:00")]
		[TestCase(-42, RelativeTimeUnit.Years, ExpectedResult = "1958-01-01T00:00:00")]
		public string Apply_SupportsNegativeUnits(int value, RelativeTimeUnit unit)
		{
			return this.TryApply(value, unit);

		}

		[Test(Description = "Assert that tokens for zero multipliers of supported time units are equivalent to the original date.")]
		[TestCase(0, RelativeTimeUnit.Seconds, ExpectedResult = "2000-01-01T00:00:00")]
		[TestCase(0, RelativeTimeUnit.Minutes, ExpectedResult = "2000-01-01T00:00:00")]
		[TestCase(0, RelativeTimeUnit.Hours, ExpectedResult = "2000-01-01T00:00:00")]
		[TestCase(0, RelativeTimeUnit.Days, ExpectedResult = "2000-01-01T00:00:00")]
		[TestCase(0, RelativeTimeUnit.Weeks, ExpectedResult = "2000-01-01T00:00:00")]
		[TestCase(0, RelativeTimeUnit.Fortnights, ExpectedResult = "2000-01-01T00:00:00")]
		[TestCase(0, RelativeTimeUnit.Months, ExpectedResult = "2000-01-01T00:00:00")]
		[TestCase(0, RelativeTimeUnit.Years, ExpectedResult = "2000-01-01T00:00:00")]
		public string Apply_SupportsZeroUnits(int value, RelativeTimeUnit unit)
		{
			return this.TryApply(value, unit);
		}

		[Test(Description = "Assert that  for supported time units throw on Unknown relative time unit ")]
		[TestCase(42, RelativeTimeUnit.Unknown)]
		[TestCase(-42, RelativeTimeUnit.Unknown)]
		[TestCase(0, RelativeTimeUnit.Unknown)]
		public void Apply_UnknownRelatimeTimeUnit_ShouldThrow(int value, RelativeTimeUnit unit)
		{
			Assert.That(() => this.TryApply(value, unit), Throws.TypeOf<FormatException>());

		}

		/*********
		** Protected methods
		*********/
		/// <summary>Tokenize a date format using a new <see cref="ArithmeticTimePlugin"/> instance, and return a string representation of the resulting tokens.</summary>
		/// <param name="plugin">The plugin which to tokenize the string.</param>
		/// <param name="format">The relative time format.</param>
		protected string Tokenize(ArithmeticTimePlugin plugin, string format)
		{
			IEnumerable<TimeToken> tokens = plugin.Tokenize(format);
			return TestHelpers.GetRepresentation(tokens);
		}

		/// <summary>Apply a relative time token to the a UTC date for 2000-01-01 using a new <see cref="ArithmeticTimePlugin"/> instance, and return a string representation of the resulting date.</summary>
		/// <param name="value">The relative time multiplier.</param>
		/// <param name="unit">The relative time unit.</param>
		protected string TryApply(int value, RelativeTimeUnit unit)
		{
			DateTime date = new DateTime(2000, 1, 1);
			TimeToken token = new TimeToken(ArithmeticTimePlugin.Key, null, value.ToString(CultureInfo.InvariantCulture), unit);
			DateTime? result = new ArithmeticTimePlugin().TryApply(token, date);
			return TestHelpers.GetRepresentation(result);
		}
	}
}
