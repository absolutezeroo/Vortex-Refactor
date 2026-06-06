// @see core/window/components/ItemListController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Iterators;
using Vortex.Core.Window.Theme;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ItemListController.as
public class ItemListController : WindowController, IScrollableWindow, IItemListWindow
{
	protected float _scrollH;
	protected float _scrollV;
	protected float _scrollAreaWidth;
	protected float _scrollAreaHeight;
	protected bool _horizontal;
	protected int _spacing;
	protected bool _autoArrangeItems;
	protected bool _scaleToFitItems;
	protected bool _resizeOnItemUpdate;
	protected bool _isPartOfGridWindow;
	protected float _scrollStepH = 25f;
	protected float _scrollStepV = 25f;
	/// @see ItemListController.as — internal container child that holds all list items
	protected IWindow? _container;
	private bool _updatingLayout;
	private bool _isDragging;
	private float _dragStartX;
	private float _dragStartY;
	private float _dragStartScrollH;
	private float _dragStartScrollV;

	/// @see ItemListController.as::ItemListController (default)
	public ItemListController() : base() { }

	/// @see ItemListController.as::ItemListController (name + rect)
	public ItemListController(string param1, Rect2 param2) : base(param1, param2) { }

	/// @see ItemListController.as::ItemListController (full AS3 11-param signature)
	public ItemListController
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
		// @see ItemListController.as — determine orientation from type
		_horizontal = param2 == 51;

		// @see ItemListController.as — read defaults from theme
		try
		{
			IWindowFactory? factory = param5.GetWindowFactory();

			IThemeManager? themeManager = factory?.GetThemeManager();
			IPropertyMap? propDefaults = themeManager?.GetPropertyDefaults(param3);

			if (propDefaults?.GetValue("spacing")?.value is int spacingVal)
			{
				_spacing = spacingVal;
			}

			if (propDefaults?.GetValue("auto_arrange_items")?.value is bool autoArrange)
			{
				_autoArrangeItems = autoArrange;
			}

			if (propDefaults?.GetValue("scale_to_fit_items")?.value is bool scaleToFit)
			{
				_scaleToFitItems = scaleToFit;
			}

			if (propDefaults?.GetValue("resize_on_item_update")?.value is bool resizeOnUpdate)
			{
				_resizeOnItemUpdate = resizeOnUpdate;
			}
		}
		catch
		{
			// Fallback: keep defaults
		}

		// @see ItemListController.as — create internal scroll container
		_container = param5.Create(
			"_CONTAINER", "", 4, 0, 0x10,
			new Rect2(0, 0, width, height),
			null, this, 0, null, "",
			new List<string> { TAG_INTERNAL, TAG_EXCLUDE }
		);
	}


	/// @see ItemListController.as::get spacing
	public int Spacing
	{
		get => _spacing;
		set
		{
			_spacing = value;
			UpdateScrollAreaRegion();
		}
	}

	/// @see ItemListController.as::get scrollH
	public float ItemScrollH
	{
		get => _scrollH;
		set => SetScrollH(value);
	}

	/// @see ItemListController.as::get scrollV
	public float ItemScrollV
	{
		get => _scrollV;
		set => SetScrollV(value);
	}

	/// @see ItemListController.as::get maxScrollH
	public float MaxScrollHValue => Math.Max(0, _scrollAreaWidth - width);

	/// @see ItemListController.as::get maxScrollV
	public float MaxScrollVValue => Math.Max(0, _scrollAreaHeight - height);

	/// @see ItemListController.as::get numListItems — delegates to _container
	public int NumListItems => _container?.numChildren ?? 0;

	/// @see ItemListController.as::get visibleRegion
	public Rect2 ItemVisibleRegion => new(_scrollH * MaxScrollHValue, _scrollV * MaxScrollVValue, width, height);

	/// @see ItemListController.as::get scrollableRegion
	public Rect2 ItemScrollableRegion => new(0, 0, _scrollAreaWidth, _scrollAreaHeight);

	/// @see ItemListController.as::get scrollStepH
	public float ItemScrollStepH
	{
		get => _scrollStepH;
		set => _scrollStepH = value;
	}

	/// @see ItemListController.as::get scrollStepV
	public float ItemScrollStepV
	{
		get => _scrollStepV;
		set => _scrollStepV = value;
	}

	/// @see ItemListController.as::get autoArrangeItems
	public bool AutoArrangeItems
	{
		get => _autoArrangeItems;
		set
		{
			_autoArrangeItems = value;
			UpdateScrollAreaRegion();
		}
	}

	/// @see ItemListController.as::get scaleToFitItems
	public bool ScaleToFitItems
	{
		get => _scaleToFitItems;
		set
		{
			_scaleToFitItems = value;
			UpdateScrollAreaRegion();
		}
	}

	/// @see ItemListController.as::get resizeOnItemUpdate
	public bool ResizeOnItemUpdate
	{
		get => _resizeOnItemUpdate;
		set => _resizeOnItemUpdate = value;
	}

	/// @see ItemListController.as::get isPartOfGridWindow
	public bool IsPartOfGridWindow
	{
		get => _isPartOfGridWindow;
		set => _isPartOfGridWindow = value;
	}

	/// @see ItemListController.as::get iterator — uses _container
	public virtual object? Iterator()
	{
		return new ItemListIterator(_container ?? this);
	}

	/// @see ItemListController.as::addListItem — delegates to _container
	public virtual IWindow? AddListItem(IWindow item)
	{
		if (_container == null)
		{
			return item;
		}

		if (_autoArrangeItems)
		{
			if (_horizontal)
			{
				item.x = _scrollAreaWidth;
				_scrollAreaWidth += item.width + _spacing;
			}
			else
			{
				item.y = _scrollAreaHeight;
				_scrollAreaHeight += item.height + _spacing;
			}
		}

		_container.AddChild(item);

		return item;
	}

	/// @see ItemListController.as::addListItemAt — delegates to _container
	public virtual IWindow? AddListItemAt(IWindow item, int index)
	{
		if (_container == null)
		{
			return item;
		}

		if (index < 0 || index > _container.numChildren)
		{
			index = _container.numChildren;
		}

		_container.AddChildAt(item, index);
		UpdateScrollAreaRegion();

		return item;
	}

	/// @see ItemListController.as::getListItemAt — delegates to _container
	public virtual IWindow? GetListItemAt(int index)
	{
		if (_container == null || index < 0 || index >= _container.numChildren)
		{
			return null;
		}

		return _container.GetChildAt(index);
	}

	/// @see ItemListController.as::removeListItem — delegates to _container
	public virtual IWindow? RemoveListItem(IWindow item)
	{
		_container?.RemoveChild(item);
		UpdateScrollAreaRegion();

		return item;
	}

	/// @see ItemListController.as::removeListItems — delegates to _container
	public virtual void RemoveListItems()
	{
		if (_container != null)
		{
			while (_container.numChildren > 0)
			{
				_container.RemoveChildAt(_container.numChildren - 1);
			}
		}

		_scrollAreaWidth = 0;
		_scrollAreaHeight = 0;
	}

	/// @see ItemListController.as::destroyListItems — delegates to _container
	public virtual void DestroyListItems()
	{
		if (_container != null)
		{
			while (_container.numChildren > 0)
			{
				IWindow? child = _container.GetChildAt(_container.numChildren - 1);
				_container.RemoveChildAt(_container.numChildren - 1);
				child?.Destroy();
			}
		}

		_scrollAreaWidth = 0;
		_scrollAreaHeight = 0;
	}

	/// @see ItemListController.as::set properties
	public override void ApplyProperties(PropertyStruct[] properties)
	{
		foreach (PropertyStruct prop in properties)
		{
			switch (prop.key)
			{
				case "spacing":
					if (prop.value != null)
					{
						_spacing = Convert.ToInt32(prop.value);
					}
					break;
				case "auto_arrange_items":
					if (prop.value is bool autoArr)
					{
						_autoArrangeItems = autoArr;
					}
					break;
				case "scale_to_fit_items":
					if (prop.value is bool scale)
					{
						_scaleToFitItems = scale;
					}
					break;
				case "resize_on_item_update":
					if (prop.value is bool resize)
					{
						_resizeOnItemUpdate = resize;
					}
					break;
			}
		}

		base.ApplyProperties(properties);
	}

	/// @see ItemListController.as::setScrollH — repositions _container + dispatches WE_SCROLL
	protected virtual void SetScrollH(float value)
	{
		_scrollH = Math.Clamp(value, 0f, 1f);
		if (_container != null)
		{
			_container.x = -(_scrollH * MaxScrollHValue);
		}
		_context?.Invalidate(this, new Rect2(0, 0, width, height), Class3655.REDRAW);
		NotifyEventListeners(new WindowEvent(WindowEvent.WE_SCROLL, this, null));
	}

	/// @see ItemListController.as::setScrollV — repositions _container + dispatches WE_SCROLL
	protected virtual void SetScrollV(float value)
	{
		_scrollV = Math.Clamp(value, 0f, 1f);
		if (_container != null)
		{
			_container.y = -(_scrollV * MaxScrollVValue);
		}
		_context?.Invalidate(this, new Rect2(0, 0, width, height), Class3655.REDRAW);
		NotifyEventListeners(new WindowEvent(WindowEvent.WE_SCROLL, this, null));
	}

	/// @see ItemListController.as::updateScrollAreaRegion — iterates _container children
	protected virtual void UpdateScrollAreaRegion()
	{
		if (_updatingLayout)
		{
			return;
		}

		_updatingLayout = true;

		_scrollAreaWidth = 0;
		_scrollAreaHeight = 0;

		int itemCount = _container?.numChildren ?? 0;

		for (int i = 0; i < itemCount; i++)
		{
			IWindow? child = _container?.GetChildAt(i);

			if (child == null)
			{
				continue;
			}

			if (_horizontal)
			{
				if (_autoArrangeItems)
				{
					child.x = _scrollAreaWidth;
				}

				_scrollAreaWidth += child.width;

				if (i < itemCount - 1)
				{
					_scrollAreaWidth += _spacing;
				}

				if (_scaleToFitItems && child.height > _scrollAreaHeight)
				{
					_scrollAreaHeight = child.height;
				}
			}
			else
			{
				if (_autoArrangeItems)
				{
					child.y = _scrollAreaHeight;
				}

				_scrollAreaHeight += child.height;

				if (i < itemCount - 1)
				{
					_scrollAreaHeight += _spacing;
				}

				if (_scaleToFitItems && child.width > _scrollAreaWidth)
				{
					_scrollAreaWidth = child.width;
				}
			}
		}

		switch (_horizontal)
		{
			case false when !_scaleToFitItems:
				_scrollAreaWidth = width;
				break;
			case true when !_scaleToFitItems:
				_scrollAreaHeight = height;
				break;
		}

		_updatingLayout = false;
	}

	/// @see ItemListController.as::update — handles events from self and _container
	public override bool Update(WindowController param1, WindowEvent param2)
	{
		// @see ItemListController.as — handle container child events
		// @see ItemListController.as — handle container child events
		if (_container != null && ReferenceEquals(param2.related, _container))
		{
			switch (param2.type)
			{
				case WindowEvent.WE_CHILD_REMOVED:
				case WindowEvent.WE_CHILD_RESIZED:
				case WindowEvent.WE_CHILD_RELOCATED:
					UpdateScrollAreaRegion();
					if (_resizeOnItemUpdate)
					{
						NotifyEventListeners(new WindowEvent(WindowEvent.WE_RESIZED, this, null));
					}
					return true;
			}
		}

		if (param2.type == WindowEvent.WE_RESIZED)
		{
			UpdateScrollAreaRegion();
		}
		else if (param1 == this)
		{
			Process(param2);
		}

		return base.Update(param1, param2);
	}

	/// @see ItemListController.as::process
	private void Process(WindowEvent param1)
	{
		if (param1 is not WindowMouseEvent mouseEvt)
		{
			return;
		}

		switch (mouseEvt.type)
		{
			case WindowMouseEvent.WHEEL:
				ScrollWithWheel(mouseEvt.delta);
				break;
			case WindowMouseEvent.DOWN:
				_isDragging = true;
				_dragStartX = mouseEvt.localX;
				_dragStartY = mouseEvt.localY;
				_dragStartScrollH = _scrollH;
				_dragStartScrollV = _scrollV;
				break;
			case WindowMouseEvent.MOVE when _isDragging:
			{
				float deltaX = mouseEvt.localX - _dragStartX;
				float deltaY = mouseEvt.localY - _dragStartY;

				if (_horizontal && MaxScrollHValue > 0)
				{
					SetScrollH(_dragStartScrollH - (deltaX / MaxScrollHValue));
				}
				else if (!_horizontal && MaxScrollVValue > 0)
				{
					SetScrollV(_dragStartScrollV - (deltaY / MaxScrollVValue));
				}

				break;
			}
			case WindowMouseEvent.UP:
				_isDragging = false;
				break;
		}
	}

	float IScrollableWindow.ScrollH { get => _scrollH; set => SetScrollH(value); }

	float IScrollableWindow.ScrollV { get => _scrollV; set => SetScrollV(value); }

	float IScrollableWindow.ScrollStepH { get => _scrollStepH; set => _scrollStepH = value; }

	float IScrollableWindow.ScrollStepV { get => _scrollStepV; set => _scrollStepV = value; }

	float IScrollableWindow.MaxScrollH => MaxScrollHValue;

	float IScrollableWindow.MaxScrollV => MaxScrollVValue;

	Rect2 IScrollableWindow.VisibleRegion => ItemVisibleRegion;

	Rect2 IScrollableWindow.ScrollableRegion => ItemScrollableRegion;

	/// @see ItemListController.as::get scrollableWindow
	public virtual IWindow? ScrollableWindow => this;

	/// @see ItemListController.as::set disableAutodrag
	public bool DisableAutodrag { set { /* Deferred */ } }

	/// @see ItemListController.as::getListItemByID — delegates to _container
	public virtual IWindow? GetListItemByID(uint id)
	{
		return _container?.GetChildByID((int)id);
	}

	/// @see ItemListController.as::getListItemByName — delegates to _container
	public virtual IWindow? GetListItemByName(string name)
	{
		return _container?.FindChildByName(name);
	}

	/// @see ItemListController.as::getListItemByTag — delegates to _container
	public virtual IWindow? GetListItemByTag(string tag)
	{
		return _container?.FindChildByTag(tag);
	}

	/// @see ItemListController.as::getListItemIndex — delegates to _container
	public virtual int GetListItemIndex(IWindow item)
	{
		return _container?.GetChildIndex(item) ?? -1;
	}

	/// @see ItemListController.as::removeListItemAt
	public virtual IWindow? RemoveListItemAt(int index)
	{
		IWindow? item = GetListItemAt(index);

		if (item != null)
		{
			RemoveListItem(item);
		}

		return item;
	}

	/// @see ItemListController.as::setListItemIndex — delegates to _container
	public virtual void SetListItemIndex(IWindow item, int index)
	{
		_container?.SetChildIndex(item, index);
		UpdateScrollAreaRegion();
	}

	/// @see ItemListController.as::swapListItems — delegates to _container
	public virtual void SwapListItems(IWindow item1, IWindow item2)
	{
		if (_container == null)
		{
			return;
		}

		int idx1 = _container.GetChildIndex(item1);
		int idx2 = _container.GetChildIndex(item2);

		if (idx1 >= 0 && idx2 >= 0)
		{
			_container.SetChildIndex(item1, idx2);
			_container.SetChildIndex(item2, idx1);
			UpdateScrollAreaRegion();
		}
	}

	/// @see ItemListController.as::groupListItemsWithID — delegates to _container
	public virtual void GroupListItemsWithId(uint id, List<IWindow> results)
	{
		_container?.GroupChildrenWithID(id, results, -1);
	}

	/// @see ItemListController.as::groupListItemsWithTag — delegates to _container
	public virtual void GroupListItemsWithTag(string tag, List<IWindow> results)
	{
		_container?.GroupChildrenWithTag(tag, results, -1);
	}

	/// @see ItemListController.as::swapListItemsAt
	public virtual void SwapListItemsAt(int index1, int index2)
	{
		IWindow? item1 = GetListItemAt(index1);
		IWindow? item2 = GetListItemAt(index2);

		if (item1 != null && item2 != null)
		{
			SwapListItems(item1, item2);
		}
	}

	/// @see ItemListController.as::arrangeListItems
	public virtual void ArrangeListItems()
	{
		UpdateScrollAreaRegion();
	}

	/// @see ItemListController.as::populate
	public virtual void Populate(IList<IWindow> items)
	{
		RemoveListItems();

		foreach (IWindow item in items)
		{
			AddListItem(item);
		}
	}

	/// @see ItemListController.as::stopDragging
	public virtual void StopDragging()
	{
		_isDragging = false;
	}

	/// @see ItemListController.as::scrollWithWheel
	/// Applies immediate scroll by delta, normalized to step/max ratio.
	public virtual void ScrollWithWheel(float delta)
	{
		if (_horizontal)
		{
			float maxH = MaxScrollHValue;

			if (maxH > 0)
			{
				float step = delta * _scrollStepH / maxH;
				SetScrollH(_scrollH - step);
			}
		}
		else
		{
			float maxV = MaxScrollVValue;

			if (maxV > 0)
			{
				float step = delta * _scrollStepV / maxV;
				SetScrollV(_scrollV - step);
			}
		}
	}
}
