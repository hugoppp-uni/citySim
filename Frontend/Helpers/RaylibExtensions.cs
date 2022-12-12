using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Raylib_CsLo.Raylib;
using Raylib_CsLo;

namespace CitySim.Frontend.Helpers
{
    internal class RaylibExtensions
    {
        public static void MyDrawRect(float left, float top, float right, float bottom, Color color)
        {
            DrawRectangleRec(new(left, top, right - left, bottom - top), color);
        }

        public static void MyDrawRoundedRect(float left, float top, float right, float bottom, float radius, Color color)
        {
            float width = right - left;
            float height = bottom - top;

            //raylib decided to handle roundness in a pretty annoying way
            float roundness = radius / (Math.Min(width, height) / 2);

            DrawRectangleRounded(new(left, top, width, height), roundness, 6, color);
        }

        public enum GuiControlProperty
        {
            BORDER_COLOR_NORMAL = 0,
            BASE_COLOR_NORMAL,
            TEXT_COLOR_NORMAL,
            BORDER_COLOR_FOCUSED,
            BASE_COLOR_FOCUSED,
            TEXT_COLOR_FOCUSED,
            BORDER_COLOR_PRESSED,
            BASE_COLOR_PRESSED,
            TEXT_COLOR_PRESSED,
            BORDER_COLOR_DISABLED,
            BASE_COLOR_DISABLED,
            TEXT_COLOR_DISABLED,
            BORDER_WIDTH,
            TEXT_PADDING,
            TEXT_ALIGNMENT,
            RESERVED,

            TEXT_SIZE = 16,             // Text size (glyphs max height)
            TEXT_SPACING,               // Text spacing between glyphs
            LINE_COLOR,                 // Line control color
            BACKGROUND_COLOR
        }

        public enum GuiControl
        {
            // Default -> populates to all controls when set
            DEFAULT = 0,
            // Basic controls
            LABEL,          // Used also for: LABELBUTTON
            BUTTON,
            TOGGLE,         // Used also for: TOGGLEGROUP
            SLIDER,         // Used also for: SLIDERBAR
            PROGRESSBAR,
            CHECKBOX,
            COMBOBOX,
            DROPDOWNBOX,
            TEXTBOX,        // Used also for: TEXTBOXMULTI
            VALUEBOX,
            SPINNER,        // Uses: BUTTON, VALUEBOX
            LISTVIEW,
            COLORPICKER,
            SCROLLBAR,
            STATUSBAR
        }
    }
}
