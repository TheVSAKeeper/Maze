using Bunit;
using Labirint.Web.Components.Base;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Labirint.Web.Tests;

[TestFixture]
public class RenderComponentTests
{
    private BunitContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _context = new();
        _context.Services.AddLogging();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public void FirstRenderDrawsOnceTest()
    {
        var component = _context.Render<ProbeComponent>().Instance;

        Assert.Multiple(() =>
        {
            Assert.That(component.FirstRenderCount, Is.EqualTo(1));
            Assert.That(component.DrawCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ForceRenderDrawsAgainTest()
    {
        var rendered = _context.Render<ProbeComponent>();
        var component = rendered.Instance;
        var markupCount = component.MarkupCount;

        await rendered.InvokeAsync(component.ForceRenderAsync);

        Assert.Multiple(() =>
        {
            Assert.That(component.DrawCount, Is.EqualTo(2));
            Assert.That(component.MarkupCount, Is.EqualTo(markupCount + 1));
        });
    }

    [Test]
    public async Task StateHasChangedWithoutForceIsSkippedTest()
    {
        var rendered = _context.Render<ProbeComponent>();
        var component = rendered.Instance;
        var markupCount = component.MarkupCount;

        await rendered.InvokeAsync(component.RequestStateHasChanged);

        Assert.That(component.MarkupCount, Is.EqualTo(markupCount));
    }

    [Test]
    public async Task OverlappingForceRendersKeepBothFramesTest()
    {
        var rendered = _context.Render<ProbeComponent>();
        var component = rendered.Instance;
        var markupCount = component.MarkupCount;

        await rendered.InvokeAsync(async () =>
        {
            await component.ForceRenderAsync();
            await component.ForceRenderAsync();
        });

        Assert.That(component.MarkupCount, Is.EqualTo(markupCount + 2));
    }

    private sealed class ProbeComponent : RenderComponent
    {
        public int DrawCount { get; private set; }
        public int FirstRenderCount { get; private set; }
        public int MarkupCount { get; private set; }

        public void RequestStateHasChanged()
        {
            StateHasChanged();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            MarkupCount++;
        }

        protected override Task OnRenderAsyncInner()
        {
            DrawCount++;
            return Task.CompletedTask;
        }

        protected override Task OnFirstRenderAsyncInner()
        {
            FirstRenderCount++;
            return Task.CompletedTask;
        }
    }
}
