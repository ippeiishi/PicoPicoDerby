﻿using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(Selectable))]
    [ColorSynchronizer(typeof(Selectable), "Transition Pressed Color")]
    public sealed class SelectablePressedColorSynchronizer : ColorSynchronizer<Selectable>
    {
        protected internal override UnityEngine.Color GetValue()
        {
            return Component.colors.pressedColor;
        }

        protected internal override void SetValue(UnityEngine.Color value)
        {
            var colors = Component.colors;
            colors.pressedColor = value;
            Component.colors = colors;
        }
    }
}
