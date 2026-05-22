// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using System.Collections.ObjectModel;

namespace YtDlpUi.UI.ViewModels;

public sealed class OptionSectionViewModel
{
    public OptionSectionViewModel(string name, IReadOnlyList<OptionItemViewModel> options)
    {
        Name = name;
        Options = new ObservableCollection<OptionItemViewModel>(options);
    }

    public string Name { get; }
    public ObservableCollection<OptionItemViewModel> Options { get; }
}
