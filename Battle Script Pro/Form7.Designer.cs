namespace Battle_Script_Pro
{
    partial class Form7
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpBoxComments = new System.Windows.Forms.GroupBox();
            this.rdBtnApostrophe = new System.Windows.Forms.RadioButton();
            this.rdBtnSemiColon = new System.Windows.Forms.RadioButton();
            this.rdBtnBackSlash = new System.Windows.Forms.RadioButton();
            this.rdBtnColon = new System.Windows.Forms.RadioButton();
            this.grpBoxDecMode = new System.Windows.Forms.GroupBox();
            this.rdBtnStrict = new System.Windows.Forms.RadioButton();
            this.rdBtnNormal = new System.Windows.Forms.RadioButton();
            this.rdBtnEnhanced = new System.Windows.Forms.RadioButton();
            this.grpBoxNumberDecMode = new System.Windows.Forms.GroupBox();
            this.rdBtnBinary = new System.Windows.Forms.RadioButton();
            this.rdBtnOctal = new System.Windows.Forms.RadioButton();
            this.rdBtnDecimal = new System.Windows.Forms.RadioButton();
            this.rdBtnHex = new System.Windows.Forms.RadioButton();
            this.rdBtnThornal = new System.Windows.Forms.RadioButton();
            this.grpBoxComments.SuspendLayout();
            this.grpBoxDecMode.SuspendLayout();
            this.grpBoxNumberDecMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpBoxComments
            // 
            this.grpBoxComments.Controls.Add(this.rdBtnApostrophe);
            this.grpBoxComments.Controls.Add(this.rdBtnSemiColon);
            this.grpBoxComments.Controls.Add(this.rdBtnBackSlash);
            this.grpBoxComments.Controls.Add(this.rdBtnColon);
            this.grpBoxComments.Location = new System.Drawing.Point(13, 13);
            this.grpBoxComments.Name = "grpBoxComments";
            this.grpBoxComments.Size = new System.Drawing.Size(137, 117);
            this.grpBoxComments.TabIndex = 0;
            this.grpBoxComments.TabStop = false;
            this.grpBoxComments.Text = "Comment Separator";
            // 
            // rdBtnApostrophe
            // 
            this.rdBtnApostrophe.AutoSize = true;
            this.rdBtnApostrophe.Location = new System.Drawing.Point(7, 92);
            this.rdBtnApostrophe.Name = "rdBtnApostrophe";
            this.rdBtnApostrophe.Size = new System.Drawing.Size(129, 17);
            this.rdBtnApostrophe.TabIndex = 3;
            this.rdBtnApostrophe.TabStop = true;
            this.rdBtnApostrophe.Text = "\'\' - Double Apostrophe";
            this.rdBtnApostrophe.UseVisualStyleBackColor = true;
            // 
            // rdBtnSemiColon
            // 
            this.rdBtnSemiColon.AutoSize = true;
            this.rdBtnSemiColon.Location = new System.Drawing.Point(7, 68);
            this.rdBtnSemiColon.Name = "rdBtnSemiColon";
            this.rdBtnSemiColon.Size = new System.Drawing.Size(31, 17);
            this.rdBtnSemiColon.TabIndex = 2;
            this.rdBtnSemiColon.TabStop = true;
            this.rdBtnSemiColon.Text = ";;";
            this.rdBtnSemiColon.UseVisualStyleBackColor = true;
            // 
            // rdBtnBackSlash
            // 
            this.rdBtnBackSlash.AutoSize = true;
            this.rdBtnBackSlash.Location = new System.Drawing.Point(7, 44);
            this.rdBtnBackSlash.Name = "rdBtnBackSlash";
            this.rdBtnBackSlash.Size = new System.Drawing.Size(35, 17);
            this.rdBtnBackSlash.TabIndex = 1;
            this.rdBtnBackSlash.TabStop = true;
            this.rdBtnBackSlash.Text = "//";
            this.rdBtnBackSlash.UseVisualStyleBackColor = true;
            // 
            // rdBtnColon
            // 
            this.rdBtnColon.AutoSize = true;
            this.rdBtnColon.Location = new System.Drawing.Point(7, 20);
            this.rdBtnColon.Name = "rdBtnColon";
            this.rdBtnColon.Size = new System.Drawing.Size(31, 17);
            this.rdBtnColon.TabIndex = 0;
            this.rdBtnColon.TabStop = true;
            this.rdBtnColon.Text = "::";
            this.rdBtnColon.UseVisualStyleBackColor = true;
            // 
            // grpBoxDecMode
            // 
            this.grpBoxDecMode.Controls.Add(this.rdBtnStrict);
            this.grpBoxDecMode.Controls.Add(this.rdBtnNormal);
            this.grpBoxDecMode.Controls.Add(this.rdBtnEnhanced);
            this.grpBoxDecMode.Location = new System.Drawing.Point(156, 13);
            this.grpBoxDecMode.Name = "grpBoxDecMode";
            this.grpBoxDecMode.Size = new System.Drawing.Size(126, 117);
            this.grpBoxDecMode.TabIndex = 1;
            this.grpBoxDecMode.TabStop = false;
            this.grpBoxDecMode.Text = "Decompiling Mode";
            // 
            // rdBtnStrict
            // 
            this.rdBtnStrict.AutoSize = true;
            this.rdBtnStrict.Location = new System.Drawing.Point(7, 68);
            this.rdBtnStrict.Name = "rdBtnStrict";
            this.rdBtnStrict.Size = new System.Drawing.Size(49, 17);
            this.rdBtnStrict.TabIndex = 2;
            this.rdBtnStrict.TabStop = true;
            this.rdBtnStrict.Text = "Strict";
            this.rdBtnStrict.UseVisualStyleBackColor = true;
            // 
            // rdBtnNormal
            // 
            this.rdBtnNormal.AutoSize = true;
            this.rdBtnNormal.Location = new System.Drawing.Point(7, 44);
            this.rdBtnNormal.Name = "rdBtnNormal";
            this.rdBtnNormal.Size = new System.Drawing.Size(58, 17);
            this.rdBtnNormal.TabIndex = 1;
            this.rdBtnNormal.TabStop = true;
            this.rdBtnNormal.Text = "Normal";
            this.rdBtnNormal.UseVisualStyleBackColor = true;
            // 
            // rdBtnEnhanced
            // 
            this.rdBtnEnhanced.AutoSize = true;
            this.rdBtnEnhanced.Location = new System.Drawing.Point(7, 20);
            this.rdBtnEnhanced.Name = "rdBtnEnhanced";
            this.rdBtnEnhanced.Size = new System.Drawing.Size(74, 17);
            this.rdBtnEnhanced.TabIndex = 0;
            this.rdBtnEnhanced.TabStop = true;
            this.rdBtnEnhanced.Text = "Enhanced";
            this.rdBtnEnhanced.UseVisualStyleBackColor = true;
            // 
            // grpBoxNumberDecMode
            // 
            this.grpBoxNumberDecMode.Controls.Add(this.rdBtnThornal);
            this.grpBoxNumberDecMode.Controls.Add(this.rdBtnHex);
            this.grpBoxNumberDecMode.Controls.Add(this.rdBtnDecimal);
            this.grpBoxNumberDecMode.Controls.Add(this.rdBtnOctal);
            this.grpBoxNumberDecMode.Controls.Add(this.rdBtnBinary);
            this.grpBoxNumberDecMode.Location = new System.Drawing.Point(13, 136);
            this.grpBoxNumberDecMode.Name = "grpBoxNumberDecMode";
            this.grpBoxNumberDecMode.Size = new System.Drawing.Size(269, 98);
            this.grpBoxNumberDecMode.TabIndex = 2;
            this.grpBoxNumberDecMode.TabStop = false;
            this.grpBoxNumberDecMode.Text = "Number Decompile Mode";
            // 
            // rdBtnBinary
            // 
            this.rdBtnBinary.AutoSize = true;
            this.rdBtnBinary.Location = new System.Drawing.Point(7, 19);
            this.rdBtnBinary.Name = "rdBtnBinary";
            this.rdBtnBinary.Size = new System.Drawing.Size(54, 17);
            this.rdBtnBinary.TabIndex = 0;
            this.rdBtnBinary.TabStop = true;
            this.rdBtnBinary.Text = "Binary";
            this.rdBtnBinary.UseVisualStyleBackColor = true;
            // 
            // rdBtnOctal
            // 
            this.rdBtnOctal.AutoSize = true;
            this.rdBtnOctal.Location = new System.Drawing.Point(7, 42);
            this.rdBtnOctal.Name = "rdBtnOctal";
            this.rdBtnOctal.Size = new System.Drawing.Size(50, 17);
            this.rdBtnOctal.TabIndex = 1;
            this.rdBtnOctal.TabStop = true;
            this.rdBtnOctal.Text = "Octal";
            this.rdBtnOctal.UseVisualStyleBackColor = true;
            // 
            // rdBtnDecimal
            // 
            this.rdBtnDecimal.AutoSize = true;
            this.rdBtnDecimal.Location = new System.Drawing.Point(6, 65);
            this.rdBtnDecimal.Name = "rdBtnDecimal";
            this.rdBtnDecimal.Size = new System.Drawing.Size(63, 17);
            this.rdBtnDecimal.TabIndex = 2;
            this.rdBtnDecimal.TabStop = true;
            this.rdBtnDecimal.Text = "Decimal";
            this.rdBtnDecimal.UseVisualStyleBackColor = true;
            // 
            // rdBtnHex
            // 
            this.rdBtnHex.AutoSize = true;
            this.rdBtnHex.Location = new System.Drawing.Point(150, 19);
            this.rdBtnHex.Name = "rdBtnHex";
            this.rdBtnHex.Size = new System.Drawing.Size(86, 17);
            this.rdBtnHex.TabIndex = 3;
            this.rdBtnHex.TabStop = true;
            this.rdBtnHex.Text = "Hexadecimal";
            this.rdBtnHex.UseVisualStyleBackColor = true;
            // 
            // rdBtnThornal
            // 
            this.rdBtnThornal.AutoSize = true;
            this.rdBtnThornal.Location = new System.Drawing.Point(150, 42);
            this.rdBtnThornal.Name = "rdBtnThornal";
            this.rdBtnThornal.Size = new System.Drawing.Size(61, 17);
            this.rdBtnThornal.TabIndex = 4;
            this.rdBtnThornal.TabStop = true;
            this.rdBtnThornal.Text = "Thornal";
            this.rdBtnThornal.UseVisualStyleBackColor = true;
            // 
            // Form7
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 243);
            this.Controls.Add(this.grpBoxNumberDecMode);
            this.Controls.Add(this.grpBoxDecMode);
            this.Controls.Add(this.grpBoxComments);
            this.Name = "Form7";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Form7_Load);
            this.grpBoxComments.ResumeLayout(false);
            this.grpBoxComments.PerformLayout();
            this.grpBoxDecMode.ResumeLayout(false);
            this.grpBoxDecMode.PerformLayout();
            this.grpBoxNumberDecMode.ResumeLayout(false);
            this.grpBoxNumberDecMode.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpBoxComments;
        private System.Windows.Forms.RadioButton rdBtnApostrophe;
        private System.Windows.Forms.RadioButton rdBtnSemiColon;
        private System.Windows.Forms.RadioButton rdBtnBackSlash;
        private System.Windows.Forms.RadioButton rdBtnColon;
        private System.Windows.Forms.GroupBox grpBoxDecMode;
        private System.Windows.Forms.RadioButton rdBtnStrict;
        private System.Windows.Forms.RadioButton rdBtnNormal;
        private System.Windows.Forms.RadioButton rdBtnEnhanced;
        private System.Windows.Forms.GroupBox grpBoxNumberDecMode;
        private System.Windows.Forms.RadioButton rdBtnBinary;
        private System.Windows.Forms.RadioButton rdBtnOctal;
        private System.Windows.Forms.RadioButton rdBtnDecimal;
        private System.Windows.Forms.RadioButton rdBtnHex;
        private System.Windows.Forms.RadioButton rdBtnThornal;
    }
}