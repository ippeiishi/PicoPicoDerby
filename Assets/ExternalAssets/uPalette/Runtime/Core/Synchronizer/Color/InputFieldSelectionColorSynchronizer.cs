﻿using UnityEngine;
using UnityEngine.UI;

namespace uPalette.Runtime.Core.Synchronizer.Color
{
    [RequireComponent(typeof(InputField))]
    [ColorSynchronizer(typeof(InputField), "Selection Color")]
    public sealed class InputFieldSelectionColorSynchronizer : ColorSynchronizer<InputField>
    {
        protected internal override UnityEngine.Color GetValue()
        {
            return Component.selectionColor;
        }

        protected internal override void SetValue(UnityEngine.Color value)
        {
            Component.selectionColor = value;
        }
    }
}
