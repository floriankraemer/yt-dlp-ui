// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Models;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class OptionItemViewModelTests
{
    [Fact]
    public void BoolValue_UpdatesValue()
    {
        var def = new YtDlpOptionDefinition
        {
            Flag = "--simulate",
            Section = "Verbosity",
            ValueType = "bool",
            Tooltip = "Simulate",
        };

        var item = new OptionItemViewModel(def, false);
        item.BoolValue = true;
        Assert.True(item.BoolValue);
        Assert.Equal(true, item.Value);
    }
}
