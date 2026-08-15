using Labirint.Web.Common.Seeding;

namespace Labirint.Web.Tests;

[TestFixture]
public class SeedSourceTests
{
    [TestCase("test", 2117040481)]
    [TestCase("лабиринт", 1551532478)]
    public void TextSeedIsHashedTest(string userSeed, int expectedSeed)
    {
        Assert.That(SeedSource.GenerateSeed(userSeed), Is.EqualTo(expectedSeed));
    }

    [TestCase("")]
    [TestCase("test")]
    [TestCase("Zj9V4CPCUYKAg")]
    [TestCase("очень длинное зерно из нескольких слов")]
    public void GeneratedSeedIsNotNegativeTest(string userSeed)
    {
        Assert.That(SeedSource.GenerateSeed(userSeed), Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void NumericSeedIsUsedAsIsTest()
    {
        SeedSource source = new()
        {
            UserSeed = "12345",
        };

        source.Reload();

        Assert.That(source.CurrentSeed, Is.EqualTo(12345));
    }

    [Test]
    public void TooLongNumericSeedFallsBackToHashTest()
    {
        const string userSeed = "99999999999999999999";

        SeedSource source = new()
        {
            UserSeed = userSeed,
        };

        source.Reload();

        Assert.That(source.CurrentSeed, Is.EqualTo(SeedSource.GenerateSeed(userSeed)));
    }

    [Test]
    public void ForcedReloadDropsUserSeedTest()
    {
        SeedSource source = new()
        {
            UserSeed = "12345",
        };

        source.Reload(true);

        Assert.Multiple(() =>
        {
            Assert.That(source.UserSeed, Is.Null);
            Assert.That(source.CurrentSeed, Is.Not.EqualTo(12345));
        });
    }

    [Test]
    public void SameSeedGivesSameSequenceTest()
    {
        SeedSource source = new()
        {
            UserSeed = "лабиринт",
        };

        source.Reload();
        var first = Enumerable.Range(0, 10).Select(_ => source.Generator.Next(1000)).ToArray();

        source.Repeat();
        var second = Enumerable.Range(0, 10).Select(_ => source.Generator.Next(1000)).ToArray();

        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void ResetSeedRequiresGenerationTest()
    {
        SeedSource source = new()
        {
            UserSeed = "12345",
        };

        source.Reload();
        source.ResetSeed();

        Assert.That(source.IsGenerateRequired, Is.True);
    }

    [Test]
    public void RepeatAfterResetAppliesUserSeedAgainTest()
    {
        SeedSource source = new()
        {
            UserSeed = "12345",
        };

        source.Reload();
        source.ResetSeed();
        source.Repeat();

        Assert.Multiple(() =>
        {
            Assert.That(source.IsGenerateRequired, Is.False);
            Assert.That(source.CurrentSeed, Is.EqualTo(12345));
        });
    }

    [Test]
    public void RepeatAfterResetWithoutUserSeedGeneratesNewSeedTest()
    {
        SeedSource source = new();

        source.ResetSeed();
        source.Repeat();

        Assert.Multiple(() =>
        {
            Assert.That(source.IsGenerateRequired, Is.False);
            Assert.That(source.UserSeed, Is.Null);
        });
    }
}
