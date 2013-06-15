﻿using System;
using System.Windows.Forms;

namespace Battle_Script_Pro
{
    class FixedRichTextBox : RichTextBox
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!base.AutoWordSelection)
            {
                base.AutoWordSelection = true;
                base.AutoWordSelection = false;
            }
        }
    }
}
