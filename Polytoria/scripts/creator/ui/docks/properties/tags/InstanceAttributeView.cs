// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Godot;
using Polytoria.Datamodel;
using Polytoria.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Polytoria.Creator.UI;

public partial class InstanceAttributeView : Control
{
	private const string AttrLabelPath = "res://scenes/creator/tags/attr_label.tscn";
	public List<Instance> Targets = [];
	[Export] private Control _container = null!;
	[Export] private LineEdit _newNameEdit = null!;
	[Export] private LineEdit _newValueEdit = null!;
	[Export] private OptionButton _typeEdit = null!;
	[Export] private Button _plusButton = null!;
	[Export] private Control _attrLayout = null!;
	[Export] private Control _blankLayout = null!;

	private string _searchFilter = "";

	public override void _Ready()
	{
		base._Ready();

		_plusButton.Pressed += AddNew;
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		Clear();
	}

	private void OnSearch(string newText)
	{
		_searchFilter = newText;
		RefreshDisplay();
	}

	public void AddNew()
	{
		if (Targets.Count == 0) return;
		if (string.IsNullOrEmpty(_newNameEdit.Text)) return;

		foreach (Instance instance in Targets)
		{
			instance.SetAttribute(_newNameEdit.Text, _newValueEdit.Text);
		}

		_newNameEdit.Text = "";
		_newValueEdit.Text = "";
		Show(Targets);
	}

	public void Clear()
	{
		_blankLayout.Visible = true;
		_attrLayout.Visible = false;

		foreach (Node item in _container.GetChildren())
		{
			item.QueueFree();
		}
	}

	private void RefreshDisplay()
	{
		foreach (Node child in _container.GetChildren())
		{
			if (child is not TagLabel label) continue;
			bool matchesSearch = string.IsNullOrEmpty(_searchFilter) ||
								 label.Text.Contains(_searchFilter, StringComparison.CurrentCultureIgnoreCase);
			label.Visible = matchesSearch;
		}
	}

	public void Show(List<Instance> instances)
	{
		Clear();
		Targets = instances;

		if (Targets.Count == 0) return;

		_blankLayout.Visible = false;
		_attrLayout.Visible = true;

		HashSet<KeyValuePair<string, object>> all = [];
		foreach (var atrr in Targets.SelectMany(instance => instance.Attributes))
		{
			all.Add(atrr);
		}

		foreach (KeyValuePair<string, object> attribute in all)
		{
			AttributeLabel label = Globals.CreateInstanceFromScene<AttributeLabel>(AttrLabelPath);
			label.AttrName = attribute.Key;
			label.Value = (string)attribute.Value;

			bool allHave = Targets.All(inst => inst.Attributes.Contains(attribute));
			if (!allHave)
			{
				label.Modulate = new Color(1, 1, 1, 0.5f);
			}

			label.DeleteRequested += () =>
			{
				// Remove from all instances that have it (null removes it)
				foreach (Instance instance in Targets)
				{
					instance.SetAttribute(_newNameEdit.Text, null);
				}
				Show(Targets);
			};

			_container.AddChild(label);
		}
	}
}
