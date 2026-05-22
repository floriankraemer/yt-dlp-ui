// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Models;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class OptionItemViewModelParsingTests
{
    [Fact]
    public void StringValue_ParsesIntegerOption()
    {
        var item = new OptionItemViewModel(
            new YtDlpOptionDefinition
            {
                Flag = "--retries",
                Section = "Download",
                ValueType = "int",
                Tooltip = "Retries",
            },
            10);

        item.StringValue = "5";
        Assert.Equal(5, item.Value);
    }
}
