using ShomreiTorah.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;

namespace ShomreiTorah.Common.Tests {


	/// <summary>
	///This is a test class for ExtensionsTest and is intended
	///to contain all ExtensionsTest Unit Tests
	///</summary>
	[TestClass()]
	public class ExtensionsTest {
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

		/// <summary>
		///A test for ReportGaps
		///</summary>
		[TestMethod()]
		public void ReportGapsTest() {
			Assert.AreEqual(
@"1     : B
        2 null items
4     : E
—————————————————————————
5     : F
        1 null item
7     : H
—————————————————————————
7     : H
        3 null items
11    : L", new[] { "A", "B", null, null, "E", "F", null, "H", null, null, null, "L", "M" }.ReportGaps());
			Assert.AreEqual("", new[] { "A", "B", "C", "D", "E", "F", "G" }.ReportGaps());
		}

		#region ToHebrewString
		[TestMethod()]
		public void ToHebrewStringLetterTest() {
			Assert.AreEqual("א", 1.ToHebrewString(HebrewNumberFormat.Letter));
			Assert.AreEqual("ב", 2.ToHebrewString(HebrewNumberFormat.Letter));
			Assert.AreEqual("ג", 3.ToHebrewString(HebrewNumberFormat.Letter));
			Assert.AreEqual("י", 10.ToHebrewString(HebrewNumberFormat.Letter));
			Assert.AreEqual("יא", 11.ToHebrewString(HebrewNumberFormat.Letter));
			Assert.AreEqual("יב", 12.ToHebrewString(HebrewNumberFormat.Letter));

			Assert.AreEqual("ק", 100.ToHebrewString(HebrewNumberFormat.Letter));
			Assert.AreEqual("קא", 101.ToHebrewString(HebrewNumberFormat.Letter));
			Assert.AreEqual("קכ", 120.ToHebrewString(HebrewNumberFormat.Letter));
			Assert.AreEqual("קכא", 121.ToHebrewString(HebrewNumberFormat.Letter));
			Assert.AreEqual("תריג", 613.ToHebrewString(HebrewNumberFormat.Letter));
		}
		[TestMethod()]
		public void ToHebrewStringLetterTestQuoted() {
			Assert.AreEqual("א'", 1.ToHebrewString(HebrewNumberFormat.LetterQuoted));
			Assert.AreEqual("ב'", 2.ToHebrewString(HebrewNumberFormat.LetterQuoted));
			Assert.AreEqual("ג'", 3.ToHebrewString(HebrewNumberFormat.LetterQuoted));
			Assert.AreEqual("י'", 10.ToHebrewString(HebrewNumberFormat.LetterQuoted));
			Assert.AreEqual("י\"א", 11.ToHebrewString(HebrewNumberFormat.LetterQuoted));
			Assert.AreEqual("י\"ב", 12.ToHebrewString(HebrewNumberFormat.LetterQuoted));

			Assert.AreEqual("ק'", 100.ToHebrewString(HebrewNumberFormat.LetterQuoted));
			Assert.AreEqual("ק\"א", 101.ToHebrewString(HebrewNumberFormat.LetterQuoted));
			Assert.AreEqual("ק\"כ", 120.ToHebrewString(HebrewNumberFormat.LetterQuoted));
			Assert.AreEqual("קכ\"א", 121.ToHebrewString(HebrewNumberFormat.LetterQuoted));
			Assert.AreEqual("תרי\"ג", 613.ToHebrewString(HebrewNumberFormat.LetterQuoted));
		}
		[TestMethod()]
		public void ToHebrewStringLetterFullזכר() {
			Assert.AreEqual("אחד", 1.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שנים", 2.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שלשה", 3.ToHebrewString(HebrewNumberFormat.Fullזכר));

			Assert.AreEqual("עשרה", 10.ToHebrewString(HebrewNumberFormat.Fullזכר));

			Assert.AreEqual("אחד עשר", 11.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שנים עשר", 12.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שלשה עשר", 13.ToHebrewString(HebrewNumberFormat.Fullזכר));

			Assert.AreEqual("עשרים", 20.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שלשים", 30.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("ארבעים", 40.ToHebrewString(HebrewNumberFormat.Fullזכר));

			Assert.AreEqual("אחד ושבעים", 71.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שנים וששים", 62.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שלשה וחמשים", 53.ToHebrewString(HebrewNumberFormat.Fullזכר));

			Assert.AreEqual("מאה", 100.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("מאה ואחד ועשרים", 121.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("מאתים", 200.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("מאתים ושנים ועשרים", 222.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("מאתים ועשרים", 220.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("ארבע מאות", 400.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שש מאות ושלשה עשר", 613.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שבע מאות ותשעה ותשעים", 799.ToHebrewString(HebrewNumberFormat.Fullזכר));

			Assert.AreEqual("אלף", 1000.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שני אלפים", 2000.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("שלשה אלפים", 3000.ToHebrewString(HebrewNumberFormat.Fullזכר));
			Assert.AreEqual("ארבעה אלפים ושלש מאות ואחד ועשרים", 4321.ToHebrewString(HebrewNumberFormat.Fullזכר));

		}
		[TestMethod()]
		public void ToHebrewStringLetterFullנקבה() {
			Assert.AreEqual("אחת", 1.ToHebrewString(HebrewNumberFormat.Fullנקבה));
			Assert.AreEqual("שתים", 2.ToHebrewString(HebrewNumberFormat.Fullנקבה));
			Assert.AreEqual("שלש", 3.ToHebrewString(HebrewNumberFormat.Fullנקבה));

			Assert.AreEqual("עשר", 10.ToHebrewString(HebrewNumberFormat.Fullנקבה));

			Assert.AreEqual("אחת עשרה", 11.ToHebrewString(HebrewNumberFormat.Fullנקבה));
			Assert.AreEqual("שתים עשרה", 12.ToHebrewString(HebrewNumberFormat.Fullנקבה));
			Assert.AreEqual("שלש עשרה", 13.ToHebrewString(HebrewNumberFormat.Fullנקבה));
		}
		#endregion

		/// <summary>
		///A test for Stripנקודות
		///</summary>
		[TestMethod()]
		public void StripנקודותTest() {
			Assert.AreEqual(
				@"
א לדוד יהוה אורי וישעי ממי אירא
יהוה מעוז־חיי ממי אפחד׃
ב בקרב עלי מרעים לאכל את־בשרי
צרי ואיבי לי המה כשלו ונפלו׃
ג אם־תחנה עלי מחנה לא־יירא לבי
אם־תקום עלי מלחמה בזאת אני בוטח׃
ד אחת שאלתי מאת־יהוה אותה אבקש
שבתי בבית־יהוה כל־ימי חיי
לחזות בנעם־יהוה ולבקר בהיכלו׃
ה כי יצפנני בסכה ביום רעה
יסתרני בסתר אהלו בצור ירוממני׃
ו ועתה ירום ראשי על איבי סביבותי ואזבחה באהלו זבחי תרועה
אשירה ואזמרה ליהוה׃
ז שמע־יהוה קולי אקרא וחנני וענני׃
ח לך אמר לבי בקשו פני את־פניך יהוה אבקש׃
ט אל־תסתר פניך ממני אל תט־באף עבדך
עזרתי היית אל־תטשני ואל־תעזבני אלהי ישעי׃
י כי־אבי ואמי עזבוני ויהוה יאספני׃
יא הורני יהוה דרכך ונחני בארח מישור למען שוררי׃
יב אל־תתנני בנפש צרי כי קמו־בי עדי־שקר ויפח חמס׃
יג לולא האמנתי לראות בטוב־יהוה בארץ חיים׃
יד קוה אל־יהוה חזק ויאמץ לבך וקוה אל־יהוה׃",
				@"
א לְדָוִ֨ד יְהוָ֤ה אוֹרִ֣י וְ֭יִשְׁעִי מִמִּ֣י אִירָ֑א
יְהוָ֥ה מָֽעוֹז־חַ֝יַּ֗י מִמִּ֥י אֶפְחָֽד׃
ב בִּקְרֹ֤ב עָלַ֨י מְרֵעִים֮ לֶֽאֱכֹ֪ל אֶת־בְּשָׂ֫רִ֥י
צָרַ֣י וְאֹֽיְבַ֣י לִ֑י הֵ֖מָּה כָֽשְׁל֣וּ וְנָפָֽלוּ׃
ג אִם־תַּֽחֲנֶ֬ה עָלַ֨י מַֽחֲנֶה֮ לֹֽא־יִירָ֪א לִ֫בִּ֥י
אִם־תָּק֣וּם עָ֭לַי מִלְחָמָ֑ה בְּ֝זֹ֗את אֲנִ֣י בוֹטֵֽחַ׃
ד אַחַ֤ת שָׁאַ֣לְתִּי מֵֽאֵת־יְהוָה֮ אוֹתָ֪הּ אֲבַ֫קֵּ֥שׁ
שִׁבְתִּ֣י בְּבֵית־יְ֭הוָה כָּל־יְמֵ֣י חַיַּ֑י
לַֽחֲז֥וֹת בְּנֹֽעַם־יְ֝הוָ֗ה וּלְבַקֵּ֥ר בְּהֵֽיכָלֽוֹ׃
ה כִּ֤י יִצְפְּנֵ֨נִי בְּסֻכֹּה֮ בְּי֪וֹם רָ֫עָ֥ה
יַ֭סְתִּרֵנִי בְּסֵ֣תֶר אָֽהֳל֑וֹ בְּ֝צ֗וּר יְרֽוֹמְמֵֽנִי׃
ו וְעַתָּ֨ה יָר֪וּם רֹאשִׁ֡י עַ֤ל אֹֽיְבַ֬י סְֽבִיבוֹתַ֗י וְאֶזְבְּחָ֣ה בְ֭אָֽהֳלוֹ זִבְחֵ֣י תְרוּעָ֑ה
אָשִׁ֥ירָה וַֽ֝אֲזַמְּרָ֗ה לַֽיהוָֽה׃
ז שְׁמַע־יְהוָ֖ה קוֹלִ֥י אֶקְרָ֗א וְחָנֵּ֥נִי וַֽעֲנֵֽנִי׃
ח לְךָ֤ אָמַ֣ר לִ֭בִּי בַּקְּשׁ֣וּ פָנָ֑י אֶת־פָּנֶ֖יךָ יְהוָ֣ה אֲבַקֵּֽשׁ׃
ט אַל־תַּסְתֵּ֬ר פָּנֶ֨יךָ מִמֶּנִּי֮ אַ֥ל תַּט־בְּאַ֗ף עַ֫בְדֶּ֥ךָ
עֶזְרָתִ֥י הָיִ֑יתָ אַֽל־תִּטְּשֵׁ֥נִי וְאַל־תַּֽ֝עַזְבֵ֗נִי אֱלֹהֵ֥י יִשְׁעִֽי׃
י כִּֽי־אָבִ֣י וְאִמִּ֣י עֲזָב֑וּנִי וַֽיהוָ֣ה יַֽאַסְפֵֽנִי׃
יא ה֤וֹרֵ֥נִי יְהוָ֗ה דַּ֫רְכֶּ֥ךָ וּ֭נְחֵנִי בְּאֹ֣רַח מִישׁ֑וֹר לְ֝מַ֗עַן שֽׁוֹרְרָֽי׃
יב אַֽל־תִּ֭תְּנֵנִי בְּנֶ֣פֶשׁ צָרָ֑י כִּ֥י קָֽמוּ־בִ֥י עֵֽדֵי־שֶׁ֝֗קֶר וִיפֵ֥חַ חָמָֽס׃
יג לׄוּׄלֵ֗ׄאׄ הֶ֭אֱמַנְתִּי לִרְא֥וֹת בְּֽטוּב־יְהוָ֗ה בְּאֶ֣רֶץ חַיִּֽים׃
יד קַוֵּ֗ה אֶל־יְה֫וָ֥ה חֲ֭זַק וְיַֽאֲמֵ֣ץ לִבֶּ֑ךָ וְ֝קַוֵּ֗ה אֶל־יְהוָֽה׃".Stripנקודות()
			);
		}


		/// <summary>
		///A test for Has
		///</summary>
		[TestMethod()]
		public void HasTest() {
			Assert.IsTrue(Enumerable.Range(0, 4).Has(4));
			Assert.IsFalse(Enumerable.Range(0, 4).Has(5));

			Assert.IsTrue(new bool[] { true, true }.Has(2));
			Assert.IsFalse(new bool[] { true, true }.Has(3));

			Assert.IsTrue(Enumerable.Range(0, 0).Has(0));
		}

		/// <summary>
		///A test for IndexOf
		///</summary>
		[TestMethod()]
		public void IndexOfTest() {
			Assert.AreEqual(-1, Enumerable.Range(0, 12).IndexOf(i => i - 3 == 12));
			Assert.AreEqual(+9, Enumerable.Range(0, 12).IndexOf(i => i + 3 == 12));
		}

		/// <summary>
		///A test for Join
		///</summary>
		[TestMethod()]
		public void JoinTest() {
			Assert.AreEqual("a, a, a", Enumerable.Repeat("a", 3).Join(", "));
			Assert.AreEqual("1.2.3.4.5", Enumerable.Range(1, 5).Join(".", i => i.ToString()));
			Assert.AreEqual("2468", Enumerable.Range(1, 4).Join("", i => (i * 2).ToString()));
		}

		/// <summary>
		///A test for Mod
		///</summary>
		[TestMethod()]
		public void ModTest() {
			Assert.AreEqual(TimeSpan.FromDays(4), TimeSpan.FromDays(11).Mod(TimeSpan.FromDays(7)));
			Assert.AreEqual(TimeSpan.FromDays(4), TimeSpan.FromDays(32).Mod(TimeSpan.FromDays(7)));
			Assert.AreEqual(TimeSpan.Zero, TimeSpan.FromDays(32).Mod(TimeSpan.FromDays(32)));
			Assert.AreEqual(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(123).Mod(TimeSpan.FromMinutes(10)));
		}

		/// <summary>
		///A test for RoundUp
		///</summary>
		[TestMethod()]
		public void RoundUpTest() {
			Assert.AreEqual(TimeSpan.FromHours(1), TimeSpan.FromHours(1).RoundUp());
			Assert.AreEqual(TimeSpan.FromHours(1), TimeSpan.FromMinutes(56).RoundUp());
			Assert.AreEqual(TimeSpan.FromHours(1), TimeSpan.FromMinutes(2).RoundUp(TimeSpan.FromHours(1)));
		}

		/// <summary>
		///A test for RoundDown
		///</summary>
		[TestMethod()]
		public void RoundDownTest() {
			Assert.AreEqual(TimeSpan.FromHours(1), TimeSpan.FromHours(1).RoundDown());
			Assert.AreEqual(TimeSpan.FromHours(1), TimeSpan.FromMinutes(64).RoundDown());
			Assert.AreEqual(TimeSpan.FromHours(1), TimeSpan.FromMinutes(62).RoundDown(TimeSpan.FromHours(1)));
		}

		/// <summary>
		///A test for AppendJoin
		///</summary>
		[TestMethod()]
		public void AppendJoinTest() {
			var builder = new StringBuilder();

			builder.AppendJoin(Enumerable.Repeat("a", 4), "");
			Assert.AreEqual("aaaa", builder.ToString());
			builder.Append(" ");
			builder.AppendJoin(Enumerable.Range(0, 5).Select(i => (i * 2).ToString()), ";");
			Assert.AreEqual("aaaa 0;2;4;6;8", builder.ToString());
		}

		/// <summary>
		///A test for CopyTo
		///</summary>
		[TestMethod()]
		public void CopyToTest() {
			var rand = new Random();

			var sourceBytes = new byte[rand.Next(2048, 16384)];
			rand.NextBytes(sourceBytes);

			var from = new MemoryStream(sourceBytes);
			var to = new MemoryStream();

			Assert.AreEqual(sourceBytes.Length, Extensions.CopyTo(from, to));
			CollectionAssert.AreEqual(sourceBytes, to.ToArray());
		}

		/// <summary>
		///A test for GetSuffix
		///</summary>
		[TestMethod()]
		public void GetSuffixTest() {
			Assert.AreEqual("th", 0.GetSuffix());
			Assert.AreEqual("st", 1.GetSuffix());
			Assert.AreEqual("nd", 2.GetSuffix());
			Assert.AreEqual("rd", 3.GetSuffix());
			Assert.AreEqual("th", 4.GetSuffix());

			Assert.AreEqual("th", 10.GetSuffix());
			Assert.AreEqual("th", 11.GetSuffix());
			Assert.AreEqual("th", 12.GetSuffix());
			Assert.AreEqual("th", 13.GetSuffix());
			Assert.AreEqual("th", 14.GetSuffix());
			Assert.AreEqual("th", 20.GetSuffix());
			Assert.AreEqual("st", 21.GetSuffix());
			Assert.AreEqual("nd", 32.GetSuffix());
			Assert.AreEqual("rd", 43.GetSuffix());
			Assert.AreEqual("th", 1169.GetSuffix());
			Assert.AreEqual("th", 3270.GetSuffix());
		}

		/// <summary>
		///A test for IsEqualTo
		///</summary>
		[TestMethod()]
		public void IsEqualToTest() {
			var rand = new Random();

			var bytes = new byte[rand.Next(128, 16384)];
			rand.NextBytes(bytes);
			Assert.IsTrue(new MemoryStream(bytes).IsEqualTo(new MemoryStream(bytes)));

			var otherBytes = new byte[rand.Next(128, 16384)];
			rand.NextBytes(otherBytes);

			Assert.IsFalse(new MemoryStream(bytes).IsEqualTo(new MemoryStream(otherBytes)));
		}
	}
}
