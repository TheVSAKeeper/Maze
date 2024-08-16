﻿using System.Collections.ObjectModel;

namespace LabirintBlazorApp.Services;

public class InventoryService
{
    private readonly Inventory _inventory;

    public InventoryService()
    {
        _inventory = new Inventory();
        Stacks = new ObservableCollection<ItemStack>(_inventory.Items);
        ScoreItems = new ObservableCollection<ScoreItem>(_inventory.ScoreItems);
    }

    /// <inheritdoc cref="Inventory.Items" />
    public IEnumerable<Item> Items => _inventory.Items.Select(x => x.Item);

    public ObservableCollection<ItemStack> Stacks { get; }
    public ObservableCollection<ScoreItem> ScoreItems { get; }

    public void Clear()
    {
        _inventory.Clear();
    }
}
