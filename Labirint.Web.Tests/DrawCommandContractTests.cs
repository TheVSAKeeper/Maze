using System.Reflection;
using System.Text.RegularExpressions;

namespace Labirint.Web.Tests;

[TestFixture]
public class DrawCommandContractTests
{
    private const string ScriptFileName = "canvasHelper.js";

    [Test]
    public void CommandCodesMatchScriptTest()
    {
        var script = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, ScriptFileName));
        var block = Regex.Match(script, @"const commandTypes = \{(?<body>[^}]*)\}");

        Assert.That(block.Success, Is.True, $"В {ScriptFileName} не найден объект commandTypes");

        var scriptCodes = Regex.Matches(block.Groups["body"].Value, @"(?<code>\d+)\s*:\s*'(?<name>\w+)'")
            .ToDictionary(match => int.Parse(match.Groups["code"].Value), match => match.Groups["name"].Value);

        var expectedCodes = typeof(DrawSequence.Command)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field is { IsLiteral: true } && field.FieldType == typeof(int))
            .ToDictionary(field => (int)field.GetRawConstantValue()!, field => ToCamelCase(field.Name));

        Assert.That(scriptCodes, Is.EqualTo(expectedCodes));
    }

    private static string ToCamelCase(string name)
    {
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
