﻿using Labirint.Core.TileFeatures;

namespace Labirint.Core.Items;

public class Sand : ScoreItem
{
    private const int MinSize = 6;
    private const int MaxSize = 9;

    public Sand()
    {
        Name = "sand";
        DisplayName = "Песочек";

        CostPerItem = 100;

        SoundSettings = new SoundSettings(string.Empty, "score");
    }

    public override int CalculateCountInMaze(int width, int height, int density)
    {
        return (width + height) / 2;
    }

    public override WorldItem GetWorldItem(WorldItemParameters parameters)
    {
        WorldItem item = base.GetWorldItem(parameters);

        return new WorldItem(this, Image, Alignment.BottomCenter, parameters.Random.Random.Next(MinSize, MaxSize + 1) / 10d)
        {
            PickUp = TryPickUp,
            AfterPlace = item.AfterPlace
        };
    }

    protected override bool TryPickUp(WorldItem worldItem)
    {
        return Stack.TryAdd((int)Math.Floor(worldItem.DrawingSettings!.Scale * 10 % MinSize + 1));
    }
}
