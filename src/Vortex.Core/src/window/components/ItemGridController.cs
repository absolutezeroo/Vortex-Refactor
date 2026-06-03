// @see core/window/components/ItemGridController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ItemGridController.as
/// Grid layout controller. Treats grid as columns of ItemLists.
public class ItemGridController : ItemListController, IItemGridWindow
{
    private bool _verticalSpacingOverride;

    /// @see ItemGridController.as::ItemGridController (default)
    public ItemGridController() : base() { }

    /// @see ItemGridController.as::ItemGridController (name + rect)
    public ItemGridController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ItemGridController.as::ItemGridController (full AS3 11-param signature)
    public ItemGridController
    (
        string param1,
        uint param2,
        uint param3,
        uint param4,
        IWindowContext param5,
        Rect2 param6,
        IWindow? param7,
        Action<WindowEvent, IWindow>? param8 = null,
        IList<object>? param9 = null,
        IList<string>? param10 = null,
        uint param11 = 0, string param12 = ""
    ) : base(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12)
    {
        // @see ItemGridController.as — vertical grid if type != 54
        _horizontal = param2 != 54;
        _scaleToFitItems = true;
    }

    /// @see ItemGridController.as::get numGridItems
    public int NumGridItems
    {
        get
        {
            int count = 0;

            for (int i = 0;
                 i < NumListItems;
                 i++)
            {
                if (GetListItemAt(i) is ItemListController col)
                {
                    count += col.NumListItems;
                }
            }

            return count;
        }
    }

    /// @see ItemGridController.as::get numColumns
    public int NumColumns => NumListItems;

    /// @see ItemGridController.as::get numRows
    public int NumRows
    {
        get
        {
            int max = 0;

            for (int i = 0;
                 i < NumListItems;
                 i++)
            {
                if (GetListItemAt(i) is ItemListController col && col.NumListItems > max)
                {
                    max = col.NumListItems;
                }
            }

            return max;
        }
    }

    /// @see ItemGridController.as::get shouldRebuildGridOnResize
    public bool ShouldRebuildGridOnResize { get; set; } = true;

    /// @see ItemGridController.as::get containerResizeToColumns
    public bool ContainerResizeToColumns { get; set; }

    /// @see ItemGridController.as::get iterator
    public override object? Iterator()
    {
        return new ItemGridIterator(this);
    }

    /// @see ItemGridController.as::getGridItemAt
    public IWindow? GetGridItemAt(int index)
    {
        if (NumColumns == 0)
        {
            return null;
        }

        int col = index % NumColumns;
        int row = index / NumColumns;

        ItemListController? column = GetListItemAt(col) as ItemListController;

        return column?.GetListItemAt(row);
    }

    /// @see ItemGridController.as::getGridItemIndex
    public int GetGridItemIndex(IWindow item)
    {
        for (int col = 0;
             col < NumColumns;
             col++)
        {
            if (GetListItemAt(col) is not ItemListController column)
            {
                continue;
            }

            for (int row = 0;
                 row < column.NumListItems;
                 row++)
            {
                if (column.GetListItemAt(row) == item)
                {
                    return col + (row * NumColumns);
                }
            }
        }

        return -1;
    }

    private int _nextColumnIndex;

    /// @see ItemGridController.as::addGridItem — round-robin column distribution
    public IWindow? AddGridItem(IWindow item)
    {
        // @see ItemGridController.as — find or create column for new item
        ItemListController? targetColumn = null;

        if (NumColumns == 0)
        {
            targetColumn = CreateColumnForItem(item);
            _nextColumnIndex = 1;
        }
        else
        {
            // @see ItemGridController.as — round-robin: sequential index modulo column count
            int colIndex = _nextColumnIndex % NumColumns;

            if (GetListItemAt(colIndex) is ItemListController col)
            {
                targetColumn = col;
            }

            // Check if we should add a new column instead
            if (targetColumn is { NumListItems: > 0 })
            {
                float colWidth = item.width + Spacing;

                if (_scrollAreaWidth + colWidth <= width)
                {
                    targetColumn = CreateColumnForItem(item);
                }
            }

            _nextColumnIndex++;
        }

        targetColumn?.AddListItem(item);
        UpdateScrollAreaRegion();

        return item;
    }

    /// @see ItemGridController.as::removeGridItem
    public IWindow? RemoveGridItem(IWindow item)
    {
        for (int col = 0;
             col < NumColumns;
             col++)
        {
            if (GetListItemAt(col) is not ItemListController column)
            {
                continue;
            }

            for (int row = 0;
                 row < column.NumListItems;
                 row++)
            {
                if (column.GetListItemAt(row) != item)
                {
                    continue;
                }

                column.RemoveListItem(item);

                UpdateScrollAreaRegion();

                return item;
            }
        }

        return null;
    }

    /// @see ItemGridController.as::removeGridItems
    public void RemoveGridItems()
    {
        for (int col = 0;
             col < NumColumns;
             col++)
        {
            ItemListController? column = GetListItemAt(col) as ItemListController;

            column?.RemoveListItems();
        }

        UpdateScrollAreaRegion();
    }

    /// @see ItemGridController.as::destroyGridItems
    public void DestroyGridItems()
    {
        for (int col = 0;
             col < NumColumns;
             col++)
        {
            ItemListController? column = GetListItemAt(col) as ItemListController;

            column?.DestroyListItems();
        }

        DestroyListItems();
    }

    /// @see ItemGridController.as::rebuildGridStructure
    public void RebuildGridStructure()
    {
        // @see ItemGridController.as — extract all items, destroy columns, redistribute
        List<IWindow> items = new();

        for (int row = 0;
             row < NumRows;
             row++)
        {
            for (int col = 0;
                 col < NumColumns;
                 col++)
            {
                ItemListController? column = GetListItemAt(col) as ItemListController;
                IWindow? item = column?.GetListItemAt(row);

                if (item != null)
                {
                    items.Add(item);
                }
            }
        }

        // Remove all items from columns without destroying
        for (int col = 0;
             col < NumColumns;
             col++)
        {
            ItemListController? column = GetListItemAt(col) as ItemListController;

            column?.RemoveListItems();
        }

        DestroyListItems();

        // Re-add items
        foreach (IWindow item in items)
        {
            AddGridItem(item);
        }
    }

    /// @see ItemGridController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "vertical_spacing":
                    if (prop.value != null)
                    {
                        VerticalSpacing = Convert.ToInt32(prop.value);
                        _verticalSpacingOverride = true;
                    }
                    break;
                case "container_resize_to_columns":
                    if (prop.value is bool resize)
                    {
                        ContainerResizeToColumns = resize;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see ItemGridController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        bool result = base.Update(param1, param2);

        if (param2.type == WindowEvent.WE_RESIZED && ShouldRebuildGridOnResize)
        {
            RebuildGridStructure();
        }

        return result;
    }


    /// @see ItemGridController.as::addColumnForItem
    private ItemListController CreateColumnForItem(IWindow item)
    {
        ItemListController column = new()
        {
            AutoArrangeItems = true,
        };

        if (_verticalSpacingOverride)
        {
            column.Spacing = VerticalSpacing;
        }
        else
        {
            column.Spacing = Spacing;
        }

        AddListItem(column);

        return column;
    }

    /// @see ItemGridController.as::get/set verticalSpacing
    public int VerticalSpacing { get; set; }

    /// @see ItemGridController.as::addGridItemAt
    public virtual IWindow? AddGridItemAt(IWindow item, int index)
    {
        if (NumColumns == 0)
        {
            return AddGridItem(item);
        }

        index = Math.Min(NumGridItems, index);

        int col = index % NumColumns;
        int row = index / NumColumns;

        ItemListController? column = GetListItemAt(col) as ItemListController;

        if (column == null)
        {
            return AddGridItem(item);
        }

        column.AddListItemAt(item, row);
        UpdateScrollAreaRegion();

        return item;
    }

    /// @see ItemGridController.as::getGridItemByID
    public virtual IWindow? GetGridItemByID(uint id)
    {
        return GetListItemByID(id);
    }

    /// @see ItemGridController.as::getGridItemByName
    public virtual IWindow? GetGridItemByName(string name)
    {
        return GetListItemByName(name);
    }

    /// @see ItemGridController.as::getGridItemByTag
    public virtual IWindow? GetGridItemByTag(string tag)
    {
        return GetListItemByTag(tag);
    }

    /// @see ItemGridController.as::removeGridItemAt
    public virtual IWindow? RemoveGridItemAt(int index)
    {
        IWindow? item = GetGridItemAt(index);

        if (item != null)
        {
            RemoveGridItem(item);
        }

        return item;
    }

    /// @see ItemGridController.as::setGridItemIndex
    public virtual void SetGridItemIndex(IWindow item, int index)
    {
        SetListItemIndex(item, index);
    }

    /// @see ItemGridController.as::swapGridItems
    public virtual void SwapGridItems(IWindow item1, IWindow item2)
    {
        SwapListItems(item1, item2);
    }

    /// @see ItemGridController.as::swapGridItemsAt
    public virtual void SwapGridItemsAt(int index1, int index2)
    {
        SwapListItemsAt(index1, index2);
    }

    /// @see ItemGridController.as::populate
    public override void Populate(IList<IWindow> items)
    {
        RemoveGridItems();

        foreach (IWindow item in items)
        {
            AddGridItem(item);
        }
    }
}
