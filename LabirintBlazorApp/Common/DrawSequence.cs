using System.Diagnostics.CodeAnalysis;

namespace LabirintBlazorApp.Common;

public class DrawSequence
{
    public enum CommandType
    {
        BeginPath = 0,
        MoveTo = 1,
        LineTo = 2,
        Stroke = 3,
        DrawImage = 4,
        StrokeStyle = 5,
        LineWidth = 6,
        ClearRect = 7
    }

    private readonly List<Command> _commands = [];

    public int Count => _commands.Count;

    public void StrokeStyle(string color)
    {
        _commands.Add(new Command(CommandType.StrokeStyle)
        {
            Color = color
        });
    }

    public void LineWidth(double width)
    {
        _commands.Add(new Command(CommandType.LineWidth)
        {
            Width = width
        });
    }

    public void BeginPath()
    {
        _commands.Add(new Command(CommandType.BeginPath));
    }

    public void MoveTo(double x, double y)
    {
        _commands.Add(new Command(CommandType.MoveTo)
        {
            X = x,
            Y = y
        });
    }

    public void LineTo(double x, double y)
    {
        _commands.Add(new Command(CommandType.LineTo)
        {
            X = x,
            Y = y
        });
    }

    public void Stroke()
    {
        _commands.Add(new Command(CommandType.Stroke));
    }

    public void DrawImage(string source, double left, double top, double width, double height)
    {
        _commands.Add(new Command(CommandType.DrawImage)
        {
            X = left,
            Y = top,
            Source = source,
            Width = width,
            Height = height
        });
    }

    public void ClearRect(double x, double y, double width, double height)
    {
        _commands.Add(new Command(CommandType.DrawImage)
        {
            X = x,
            Y = y,
            Width = width,
            Height = height
        });
    }

    public List<Command> ToList()
    {
        return [.._commands];
    }

    [method: SetsRequiredMembers]
    public class Command(CommandType type)
    {
        public required CommandType Type { get; set; } = type;
        public double X { get; set; }
        public double Y { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
