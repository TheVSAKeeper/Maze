﻿using Labirint.Core.TileFeatures;

namespace Labirint.Core.Items.Base;

public abstract class Item
{
    public abstract string Name { get; }
    public abstract string DisplayName { get; }

    public string Icon => $"/images/items/{Name}-icon.png";
    public string Image => $"/images/items/{Name}.png";

    public ItemStack Stack { get; protected init; } = null!;

    public virtual ControlSettings? ControlSettings => null;
    public virtual SoundSettings? SoundSettings => null;

    public virtual bool TryUse(Position position, Direction? direction, Labyrinth labyrinth)
    {
        if (Stack.TryRemove(1) == false)
        {
            return false;
        }

        AfterUse(position, direction, labyrinth);
        return true;
    }

    public virtual WorldItem GetWorldItem(WorldItemParameters parameters)
    {
        return new WorldItem(this, Image, Alignment.Center, 0.9)
        {
            PickUp = TryPickUp,
            AfterPlace = AfterPlace
        };
    }

    public abstract int CalculateCountInMaze(int width, int height, int density);

    public IEnumerable<WorldItem> GetItemsForPlace(WorldItemParameters parameters)
    {
        int requiredCount = CalculateCountInMaze(parameters.Width, parameters.Height, parameters.Density);

        for (int i = 0; i < requiredCount; i++)
        {
            yield return GetWorldItem(parameters);
        }
    }

    protected virtual bool TryPickUp(WorldItem worldItem)
    {
        if (Stack.TryAdd(1) == false)
        {
            return false;
        }

        AfterPickUp(worldItem);
        return true;
    }

    protected virtual void AfterUse(Position position, Direction? direction, Labyrinth labyrinth)
    {
    }

    protected virtual void AfterPickUp(WorldItem worldItem)
    {
    }

    protected virtual void AfterPlace(Position position, Labyrinth labyrinth)
    {
        Stack.InMazeCount++;
    }
}
