﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using uPalette.Editor.Core.Shared;
using uPalette.Editor.Foundation.EasyTreeView;
using uPalette.Runtime.Foundation.TinyRx;

namespace uPalette.Editor.Core.PaletteEditor
{
    internal abstract class PaletteEditorTreeView<T> : TreeViewBase
    {
        private const string DragType = "PaletteEditorTreeView";

        private readonly Dictionary<int, string> _columnIndexToThemeIdMap = new Dictionary<int, string>();
        private readonly Dictionary<string, int> _entryIdToItemIdMap = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _folderPathToItemIdMap = new Dictionary<string, int>();

        private readonly Subject<(PaletteEditorTreeViewEntryItem<T> item, int newIndex)> _itemIndexChangedSubject =
            new Subject<(PaletteEditorTreeViewEntryItem<T> item, int newIndex)>();

        private readonly Subject<Empty> _onAddThemeButtonClickedSubject = new Subject<Empty>();

        private readonly Subject<string> _onRemoveThemeButtonClickedSubject = new Subject<string>();

        private readonly Subject<(string themeId, string newName)> _onRenameThemeButtonClickedSubject =
            new Subject<(string themeId, string newName)>();

        private readonly Dictionary<string, string> _themes = new Dictionary<string, string>();

        [NonSerialized] private int _currentItemId;

        protected PaletteEditorTreeView(TreeViewState state) : base(state)
        {
            showAlternatingRowBackgrounds = true;

            rowHeight = EditorGUIUtility.singleLineHeight + 8;

            SetupColumnStates();
        }

        public char FolderDelimiter => UPaletteProjectSettings.instance.FolderDelimiter;

        public IObservable<(PaletteEditorTreeViewEntryItem<T> item, int newIndex)> ItemIndexChangedAsObservable =>
            _itemIndexChangedSubject;

        public bool FolderMode { get; private set; }

        public void AddThemeColumn(string id, string name)
        {
            _themes.Add(id, name);
            SetupColumnStates();
        }

        public void RemoveThemeColumn(string id)
        {
            _themes.Remove(id);
            SetupColumnStates();
        }

        public void ClearThemeColumns()
        {
            _themes.Clear();
            SetupColumnStates();
        }

        public void SetThemeName(string id, string name)
        {
            _themes[id] = name;
            SetupColumnStates();
        }

        public PaletteEditorTreeViewEntryItem<T> AddItem(string entryId, string name, Dictionary<string, T> values)
        {
            var entryItem = new PaletteEditorTreeViewEntryItem<T>(entryId, name, values)
            {
                id = _currentItemId++
            };
            _entryIdToItemIdMap.Add(entryId, entryItem.id);
            AddItemAndSetParent(entryItem, -1);
            SetHierarchy(entryItem.id);
            return entryItem;
        }

        /// <summary>
        ///     Set the item as a child item of the appropriate parent item. If there is no parent item, create it.
        /// </summary>
        /// <param name="entryItemId"></param>
        public void SetHierarchy(int entryItemId)
        {
            var entryItem = (PaletteEditorTreeViewEntryItem<T>)GetItem(entryItemId);

            if (FolderMode)
            {
                var name = entryItem.Name.Value;
                var hasFolder = name.Contains(FolderDelimiter);
                var folderPath = hasFolder
                    ? name.Substring(0, name.LastIndexOf(FolderDelimiter))
                    : "";

                // Create folders if needed
                if (hasFolder)
                {
                    var folderNames = folderPath.Split(FolderDelimiter);

                    var currentFolderPath = "";
                    foreach (var folderName in folderNames)
                    {
                        var parentFolderId = string.IsNullOrEmpty(currentFolderPath)
                            ? -1
                            : _folderPathToItemIdMap[currentFolderPath];
                        if (!string.IsNullOrEmpty(currentFolderPath))
                            currentFolderPath += FolderDelimiter;
                        currentFolderPath += folderName;

                        if (!_folderPathToItemIdMap.ContainsKey(currentFolderPath))
                        {
                            var folderItem = new PaletteEditorTreeViewFolderItem(currentFolderPath)
                            {
                                id = _currentItemId++,
                                displayName = folderName
                            };
                            _folderPathToItemIdMap.Add(currentFolderPath, folderItem.id);
                            AddItemAndSetParent(folderItem, parentFolderId, false);
                            SetItemIndexByName(folderItem);
                        }
                    }
                }

                // Parenting
                var folderItemId = string.IsNullOrEmpty(folderPath) ? -1 : _folderPathToItemIdMap[folderPath];
                SetParent(entryItemId, folderItemId);

                // Remove empty folders
                RemoveEmptyFolders();

                // Set display name
                var nonFolderName = hasFolder
                    ? name.Substring(name.LastIndexOf(FolderDelimiter) + 1)
                    : name;
                entryItem.displayName = nonFolderName;
            }
            else
            {
                // Parenting
                SetParent(entryItemId, -1);

                // Set display name
                entryItem.displayName = entryItem.Name.Value;
            }
        }

        private void RemoveEmptyFolders()
        {
            // Process in order of long folder path (from the end folder)
            var orderedFolderPaths = _folderPathToItemIdMap.Keys
                .OrderByDescending(x => x.Length)
                .ToArray();

            var removeTargetFolders = new List<(string path, int itemId)>();
            foreach (var folderPath in orderedFolderPaths)
            {
                var folderItemId = _folderPathToItemIdMap[folderPath];
                var folderItem = GetItem(folderItemId);

                // if folder has no children, remove it.
                if (!folderItem.hasChildren)
                {
                    removeTargetFolders.Add((folderPath, folderItemId));
                    continue;
                }

                // if all children are remove target, remove it.
                var allChildrenAreRemoveTarget = folderItem.children
                    .All(x => removeTargetFolders.Any(y => y.itemId == x.id));
                if (allChildrenAreRemoveTarget)
                    removeTargetFolders.Add((folderPath, folderItemId));
            }

            foreach (var removeTargetFolder in removeTargetFolders)
            {
                var removeTargetFolderItemId = removeTargetFolder.itemId;
                _folderPathToItemIdMap.Remove(removeTargetFolder.path);
                RemoveItem(removeTargetFolderItemId, false);
            }
        }

        public void SetFolderMode(bool folderMode, bool reload = true)
        {
            if (FolderMode == folderMode)
                return;

            FolderMode = folderMode;

            // Remove all folder items
            foreach (var folderItemId in _folderPathToItemIdMap.Values)
                RemoveItem(folderItemId, false);
            _folderPathToItemIdMap.Clear();

            // Build hierarchy
            foreach (var entryItemId in _entryIdToItemIdMap.Values)
                SetHierarchy(entryItemId);

            if (FolderMode)
                OrderItemsByName(RootItem, true);

            if (reload)
                Reload();
        }

        public void RemoveItem(string entryId, bool invokeCallback = true)
        {
            var id = _entryIdToItemIdMap[entryId];
            var item = GetItem(id);
            _entryIdToItemIdMap.Remove(entryId);
            RemoveItem(id, invokeCallback);
            RemoveParentItemsIfEmpty(item);
        }

        private void RemoveParentItemsIfEmpty(TreeViewItem item)
        {
            var parent = item.parent;
            if (parent.id != -1 && !parent.hasChildren)
            {
                var folderItem = (PaletteEditorTreeViewFolderItem)parent;
                _folderPathToItemIdMap.Remove(folderItem.FolderPath);
                RemoveItem(parent.id, false);
                RemoveParentItemsIfEmpty(parent);
            }
        }

        protected override bool CanRename(TreeViewItem item)
        {
            // Rename is not supported for folder.
            if (item is PaletteEditorTreeViewFolderItem)
                return false;

            // Set displayName to full name during rename.
            if (FolderMode)
                item.displayName = ((PaletteEditorTreeViewEntryItem<T>)item).Name.Value;

            return true;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            var item = (PaletteEditorTreeViewEntryItem<T>)GetItem(args.itemID);
            if (args.acceptedRename)
                item.SetName(args.newName, true);

            // Set displayName to non-folder name after rename.
            if (FolderMode)
            {
                var name = item.Name.Value;
                var hasFolder = name.Contains(FolderDelimiter);
                var nonFolderName = hasFolder
                    ? name.Substring(name.LastIndexOf(FolderDelimiter) + 1)
                    : name;
                item.displayName = nonFolderName;
            }
        }

        protected override void CellGUI(int columnIndex, Rect cellRect, RowGUIArgs args)
        {
            cellRect.height -= 4;
            cellRect.y += 2;
            if (args.item is PaletteEditorTreeViewFolderItem)
            {
                args.rowRect.yMin += 5;
                DefaultRowGUI(args);
            }
            else
            {
                var item = (PaletteEditorTreeViewEntryItem<T>)args.item;
                var columnCount = ColumnStates.Length;
                if (columnIndex == 0)
                {
                    args.rowRect.yMin += 5;
                    DefaultRowGUI(args);
                }
                else if (columnIndex == columnCount - 1)
                {
                    if (GUI.Button(cellRect, new GUIContent("Apply"))) item.OnApplyButtonClicked();
                }
                else
                {
                    var themeId = _columnIndexToThemeIdMap[columnIndex];
                    // For Gradient, use SetValueAndNotify to notify the change.
                    using (var ccs = new EditorGUI.ChangeCheckScope())
                    {
                        var newValue = DrawValueField(cellRect, item.Values[themeId].Value);
                        if (ccs.changed)
                            item.Values[themeId].SetValueAndNotify(newValue);
                    }
                }
            }
        }

        protected abstract T DrawValueField(Rect rect, T value);

        protected override IOrderedEnumerable<TreeViewItem> OrderItems(
            IList<TreeViewItem> items,
            int keyColumnIndex,
            bool ascending
        )
        {
            string KeySelector(TreeViewItem x)
            {
                return GetText((PaletteEditorTreeViewEntryItem<T>)x, keyColumnIndex);
            }

            return ascending
                ? items.OrderBy(KeySelector, Comparer<string>.Create(EditorUtility.NaturalCompare))
                : items.OrderByDescending(KeySelector, Comparer<string>.Create(EditorUtility.NaturalCompare));
        }

        protected override string GetTextForSearch(TreeViewItem item, int columnIndex)
        {
            return GetText((PaletteEditorTreeViewEntryItem<T>)item, columnIndex);
        }

        private static string GetText(PaletteEditorTreeViewEntryItem<T> entryItem, int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                    return entryItem.Name.Value;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            // Index is not supported in folder mode because items are sorted by name.
            if (FolderMode)
                return false;

            return string.IsNullOrEmpty(searchString);
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            // Index is not supported in folder mode because items are sorted by name.
            if (FolderMode)
                return;

            var selections = args.draggedItemIDs;
            if (selections.Count <= 0) return;

            var items = GetRows()
                .Where(i => selections.Contains(i.id))
                .Select(x => (PaletteEditorTreeViewEntryItem<T>)x)
                .ToArray();

            if (items.Length <= 0) return;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(DragType, items);
            DragAndDrop.StartDrag(items.Length > 1 ? "<Multiple>" : items[0].displayName);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            // Index is not supported in folder mode because items are sorted by name.
            if (FolderMode)
                return DragAndDropVisualMode.None;

            if (args.performDrop)
            {
                var data = DragAndDrop.GetGenericData(DragType);
                var items = (PaletteEditorTreeViewEntryItem<T>[])data;

                if (items == null || items.Length <= 0) return DragAndDropVisualMode.None;

                switch (args.dragAndDropPosition)
                {
                    case DragAndDropPosition.BetweenItems:
                        var afterIndex = args.insertAtIndex;
                        foreach (var item in items)
                        {
                            var itemIndex = RootItem.children.IndexOf(item);
                            if (itemIndex < afterIndex) afterIndex--;

                            SetItemIndex(item, afterIndex, true);
                            afterIndex++;
                        }

                        SetSelection(items.Select(x => x.id).ToArray());

                        Reload();
                        break;
                    case DragAndDropPosition.UponItem:
                    case DragAndDropPosition.OutsideItems:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return DragAndDropVisualMode.Move;
        }

        /// <summary>
        ///     Sort items by specifying index
        /// </summary>
        /// <param name="entryItem"></param>
        /// <param name="index"></param>
        /// <param name="notify"></param>
        public void SetItemIndex(PaletteEditorTreeViewEntryItem<T> entryItem, int index, bool notify)
        {
            var children = RootItem.children;
            var itemIndex = RootItem.children.IndexOf(entryItem);
            children.RemoveAt(itemIndex);
            children.Insert(index, entryItem);
            if (notify)
                _itemIndexChangedSubject.OnNext((entryItem, index));
        }

        /// <summary>
        ///     Sort items according to name
        /// </summary>
        /// <param name="item"></param>
        public void SetItemIndexByName(TreeViewItem item)
        {
            var children = item.parent.children;
            var orderedChildren = children
                .OrderBy(x => x.displayName, Comparer<string>.Create(EditorUtility.NaturalCompare))
                .ToArray();
            var index = Array.IndexOf(orderedChildren, item);

            var itemIndex = item.parent.children.IndexOf(item);
            children.RemoveAt(itemIndex);
            children.Insert(index, item);
        }

        /// <summary>
        ///     Sort all items under <see cref="parent" /> according to name
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="recursive"></param>
        public void OrderItemsByName(TreeViewItem parent, bool recursive)
        {
            if (parent.children == null)
                return;

            var children = parent.children;
            var orderedChildren = children
                .OrderBy(x => x.displayName, Comparer<string>.Create(EditorUtility.NaturalCompare))
                .ToArray();

            children.Clear();
            foreach (var child in orderedChildren)
            {
                children.Add(child);
                if (recursive)
                    OrderItemsByName(child, true);
            }
        }

        private void SetupColumnStates()
        {
            var columnIndex = 0;
            _columnIndexToThemeIdMap.Clear();
            var columns = new List<MultiColumnHeaderState.Column>();
            var nameColumn = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 130,
                minWidth = 50,
                autoResize = false,
                allowToggleVisibility = false
            };
            columns.Add(nameColumn);
            columnIndex++;
            foreach (var theme in _themes)
            {
                var themeId = theme.Key;
                var themeName = theme.Value;
                var valueColumn = new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(themeName),
                    headerTextAlignment = TextAlignment.Center,
                    canSort = false,
                    width = 150,
                    minWidth = 50,
                    autoResize = false,
                    allowToggleVisibility = false
                };
                _columnIndexToThemeIdMap.Add(columnIndex, themeId);
                columns.Add(valueColumn);
                columnIndex++;
            }

            var applyColumn = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Apply to Selected"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                width = 120,
                minWidth = 50,
                autoResize = false,
                allowToggleVisibility = false
            };
            columns.Add(applyColumn);
            ColumnStates = columns.ToArray();
        }
    }
}