using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;
using System.Xml;
using System.Net;
using System.Text.RegularExpressions;

namespace Battle_Script_Pro
{
    public partial class Form1 : Form
    {
        Dictionary<string, uint> baseBattleRamLocations;
        Dictionary<string, Command> commands;
        Dictionary<string, SuperCommand> superCommands;
        Dictionary<string, uint> userDefinitions;
        Dictionary<DynamicPointer, bool> pointerNames;
        Dictionary<string, string> dynamicStrings;
        Dictionary<string, uint> keywords;
        List<int> decompiledOffsets;
        List<RichTextBox> scripts;
        int lastInsertedScriptLocation;
        int lastInsertedScriptLength;
        bool scriptInserted;
        int freeSpaceLocation;
        List<bool> unsaved;
        List<int> newScriptNames;
        bool dynamicLocationSet;
        bool insideHashOrgBracket;
        bool firstTime;
        int location;
        string selectedROMPath;
        List<string> currentlyOpenBS;
        uint moveEffectTableLocation;
        Form4 openFindForm;
        Form5 openFARForm;
        byte freeSpaceByte;
        Color rtbBackColour = Color.White;
        Color rtbForeColour = Color.Black;
        Font rtbFont = new Font("Arial", 10.0f, FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        FontStyle rtbFontStyle = FontStyle.Regular;
        string rtbFontName = "Arial";
        string commentString = "::";
        int decompileMode = 1;
        int numberDecompileMode = 3;
        Dictionary<string, bool> headersNeeded;
        Dictionary<uint, string> pokemonExport;
        Dictionary<uint, string> stdExport;
        Dictionary<uint, string> typeExport;
        Dictionary<uint, string> movesExport;
        Dictionary<uint, string> itemsExport;
        Dictionary<uint, string> abilitiesExport;
        Dictionary<uint, string> bankExport;
        bool specialOneTriggered = false;
        string specialOneType = "";
        uint moveDataTableLocation;

        public Form1()
        {
            InitializeComponent();
            this.MaximumSize = this.MinimumSize = this.Size;
            SetupTool();
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form_KeyDown);
        }

        public Form1(string fileName)
        {
            InitializeComponent();
            this.MaximumSize = this.MinimumSize = this.Size;
            SetupTool();
            if (Path.GetExtension(fileName).ToUpper().Equals(".BS") || Path.GetExtension(fileName).ToUpper().Equals(".BSH"))
            {
                scripts[tabControl1.SelectedIndex].Lines = File.ReadAllLines(fileName);
            }
            else
            {
                MessageBox.Show("This file format is unsupported through drag and drop.");
            }
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form_KeyDown);
        }

        public Form1(int offset, string fileName)
        {
            InitializeComponent();
            this.MaximumSize = this.MinimumSize = this.Size;
            SetupTool();
            if (File.Exists(fileName))
            {
                if (Path.GetExtension(fileName).ToUpper().Equals(".GBA"))
                {
                    byte[] rom = File.ReadAllBytes(fileName);
                    headersNeeded = new Dictionary<string, bool>();
                    headersNeeded.Add("abilities", false);
                    headersNeeded.Add("moves", false);
                    headersNeeded.Add("pokemon", false);
                    headersNeeded.Add("items", false);
                    List<string> toReturn = DecompileScript(location, File.ReadAllBytes(selectedROMPath)).ToList();
                    bool insertReturn = false;
                    int countLines = 0;
                    foreach (string s in headersNeeded.Keys)
                    {
                        if (headersNeeded[s])
                        {
                            toReturn.Insert(0, "#include " + s + ".bsh");
                            countLines++;
                            insertReturn = true;
                        }
                    }
                    if (insertReturn)
                    {
                        toReturn.Insert(countLines, "");
                    }
                    scripts[tabControl1.SelectedIndex].Lines = toReturn.ToArray();
                    headersNeeded.Clear();
                }
                else
                {
                    MessageBox.Show("This file format is unsupported.");
                }
                this.KeyPreview = true;
                this.KeyDown += new KeyEventHandler(Form_KeyDown);
            }
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case (Keys.S):
                        {
                            saveToolStripMenuItem_Click(sender, e);
                            break;
                        }
                    case (Keys.N):
                        {
                            CreateNewTab(null);
                            break;
                        }
                    case (Keys.W):
                        {
                            CloseScript();
                            break;
                        }
                    case (Keys.O):
                        {
                            openToolStripMenuItem_Click(sender, e);
                            break;
                        }
                    case (Keys.F):
                        {
                            Find();
                            break;
                        }
                    case (Keys.H):
                        {
                            FindAndReplace();
                            break;
                        }
                    case (Keys.G):
                        {
                            Goto();
                            break;
                        }
                    case (Keys.D):
                        {
                            btnDebug_Click(sender, e);
                            break;
                        }
                    default:
                        break;
                }
            }
            else
            {
                if (e.KeyCode == Keys.F1)
                {
                    commandGuideToolStripMenuItem_Click(sender, e);
                }
                else if (e.KeyCode == Keys.F2)
                {
                    OpenWebsite();
                }
                else if (e.KeyCode == Keys.F3)
                {
                    OpenDataLocation();
                }
                else if (e.KeyCode == Keys.F12)
                {
                    OpenSettingsForm();
                }
                else if (e.KeyCode == Keys.F4)
                {
                    downloadCleanINIToolStripMenuItem_Click(sender, e);
                }
                else if (e.KeyCode == Keys.F5)
                {
                    updateDatabaseToolStripMenuItem_Click(sender, e);
                }
            }
        }

        private void CreateNewTab(string name)
        {
            int number = 1;
            if (name == null)
            {
                while (newScriptNames.Contains(number))
                {
                    number++;
                }
            }
            else
            {
                number = 0;
            }
            TabPage page;
            if (name == null)
            {
                page = new TabPage("Script " + number.ToString());
            }
            else
            {
                page = new TabPage(Path.GetFileNameWithoutExtension(name));
            }
            FixedRichTextBox rtb;
            LineNumbersControlForRichTextBox.LineNumbersForRichText lineNos;
            CreateRTBAndNumbersList(out rtb, out lineNos);
            page.Controls.Add(rtb);
            page.Controls.Add(lineNos);
            tabControl1.TabPages.Add(page);
            unsaved.Add(false);
            currentlyOpenBS.Add(name);
            newScriptNames.Add(number);
            tabControl1.SelectedIndex = tabControl1.TabPages.IndexOf(page);
        }

        private void Goto()
        {
            Form3 newForm = new Form3();
            newForm.ShowDialog();
            int number = newForm.LineNumber;
            bool success = newForm.Success;
            if (success)
            {
                int j = 0;
                string text = scripts[tabControl1.SelectedIndex].Text;
                if (number > scripts[tabControl1.SelectedIndex].Lines.Length)
                {
                    number = scripts[tabControl1.SelectedIndex].Lines.Length;
                }
                for (int i = 1; i < number; i++)
                {
                    j = text.IndexOf('\n', j + 1);
                    if (j == -1)
                    {
                        break;
                    }
                }
                if (number > 1)
                {
                    scripts[tabControl1.SelectedIndex].Select(j + 1, 0);
                }
                else
                {
                    scripts[tabControl1.SelectedIndex].Select(j, 0);
                }
            }
        }

        private void Find()
        {
            Form4 newForm = new Form4();
            openFindForm = newForm;
            openFindForm.btnFind.Click += new System.EventHandler(this.PressedFind);
            newForm.Show();
            openFindForm.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form4_FormClosing);
        }

        private void FindAndReplace()
        {
            Form5 newForm = new Form5();
            openFARForm = newForm;
            openFARForm.btnFind.Click += new System.EventHandler(this.PressedNewFind);
            openFARForm.btnReplace.Click += new System.EventHandler(this.PressedReplace);
            openFARForm.btnReplaceAll.Click += new System.EventHandler(this.PressedReplaceAll);
            openFARForm.Show();
            openFARForm.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form5_FormClosing);
        }

        private void PressedFind(object sender, EventArgs e)
        {
            int startIndex = 0;
            int endIndex = 0;
            int start = scripts[tabControl1.SelectedIndex].SelectionStart + scripts[tabControl1.SelectedIndex].SelectionLength;
            if (openFindForm.txtSearch.Text.Length > 0)
            {
                startIndex = FindMyText(openFindForm.txtSearch.Text.Trim(), start, scripts[tabControl1.SelectedIndex].Text.Length);
                if (startIndex == -1)
                {
                    start = 0;
                    startIndex = FindMyText(openFindForm.txtSearch.Text.Trim(), start, scripts[tabControl1.SelectedIndex].Text.Length);
                    if (startIndex == -1)
                    {
                        MessageBox.Show("The requested string could not be found.");
                        return;
                    }
                }
            }
            if (startIndex >= 0)
            {
                endIndex = openFindForm.txtSearch.Text.Length;
                start = startIndex + endIndex;
            }
            scripts[tabControl1.SelectedIndex].Focus();
        }

        private void PressedNewFind(object sender, EventArgs e)
        {
            int startIndex = 0;
            int endIndex = 0;
            int start = scripts[tabControl1.SelectedIndex].SelectionStart + scripts[tabControl1.SelectedIndex].SelectionLength;
            if (openFARForm.txtSearch.Text.Length > 0)
            {
                startIndex = FindMyText(openFARForm.txtSearch.Text.Trim(), start, scripts[tabControl1.SelectedIndex].Text.Length);
                if (startIndex == -1)
                {
                    start = 0;
                    startIndex = FindMyText(openFARForm.txtSearch.Text.Trim(), start, scripts[tabControl1.SelectedIndex].Text.Length);
                    if (startIndex == -1)
                    {
                        MessageBox.Show("The requested string could not be found.");
                        return;
                    }
                }
            }
            if (startIndex >= 0)
            {
                endIndex = openFARForm.txtSearch.Text.Length;
                start = startIndex + endIndex;
            }
            scripts[tabControl1.SelectedIndex].Focus();
        }

        private void PressedReplace(object sender, EventArgs e)
        {
            bool search = true;
            if (!scripts[tabControl1.SelectedIndex].SelectedText.Equals(openFARForm.txtSearch.Text))
            {
                PressedFindNew(out search);
            }
            if (search)
            {
                scripts[tabControl1.SelectedIndex].SelectedText = openFARForm.txtReplace.Text;
            }
            else
            {
                MessageBox.Show("The requested string could not be found.");
                return;
            }
            scripts[tabControl1.SelectedIndex].Focus();
        }

        private void PressedReplaceAll(object sender, EventArgs e)
        {
            bool search = true;
            bool firstTime = true;
            int start = 0;
            while (search)
            {
                if (!scripts[tabControl1.SelectedIndex].SelectedText.Equals(openFARForm.txtSearch.Text))
                {
                    NewPressedFind(out search, start, out start);
                }
                if (search)
                {
                    scripts[tabControl1.SelectedIndex].SelectedText = openFARForm.txtReplace.Text;
                    firstTime = false;
                }
                else
                {
                    if (firstTime)
                    {
                        MessageBox.Show("The requested string could not be found.");
                        return;
                    }
                }
            }
            scripts[tabControl1.SelectedIndex].Focus();
        }

        private void PressedFindNew(out bool search)
        {
            int startIndex = 0;
            int endIndex = 0;
            int start = scripts[tabControl1.SelectedIndex].SelectionStart + scripts[tabControl1.SelectedIndex].SelectionLength;
            if (openFARForm.txtSearch.Text.Length > 0)
            {
                startIndex = FindMyText(openFARForm.txtSearch.Text.Trim(), start, scripts[tabControl1.SelectedIndex].Text.Length);
                if (startIndex == -1)
                {
                    start = 0;
                    startIndex = FindMyText(openFARForm.txtSearch.Text.Trim(), start, scripts[tabControl1.SelectedIndex].Text.Length);
                    if (startIndex == -1)
                    {
                        search = false;
                        return;
                    }
                }
            }
            if (startIndex >= 0)
            {
                endIndex = openFARForm.txtSearch.Text.Length;
                start = startIndex + endIndex;
            }
            search = true;
        }

        private void NewPressedFind(out bool search, int start, out int newStart)
        {
            int startIndex = 0;
            int endIndex = 0;
            newStart = start;
            if (openFARForm.txtSearch.Text.Length > 0)
            {
                startIndex = FindMyText(openFARForm.txtSearch.Text.Trim(), start, scripts[tabControl1.SelectedIndex].Text.Length);
                if (startIndex == -1)
                {
                    search = false;
                    return;
                }
            }
            if (startIndex >= 0)
            {
                endIndex = openFARForm.txtSearch.Text.Length;
                newStart = startIndex + endIndex;
            }
            search = true;
        }

        private int FindMyText(string txtToSearch, int searchStart, int searchEnd)
        {
            int indexOfSearchText = 0;
            if (searchStart > 0 && searchEnd > 0 && indexOfSearchText >= 0)
            {
                scripts[tabControl1.SelectedIndex].Select();
            }
            int retVal = -1;
            if (searchStart >= 0 && indexOfSearchText >= 0)
            {
                if (searchEnd > searchStart || searchEnd == -1)
                {
                    indexOfSearchText = scripts[tabControl1.SelectedIndex].Find(txtToSearch, searchStart, searchEnd, RichTextBoxFinds.None);
                    if (indexOfSearchText != -1)
                    {
                        retVal = indexOfSearchText;
                    }
                }
            }
            return retVal;
        }

        private void SetupTool()
        {
            moveEffectTableLocation = 0;
            numericUpDown1.Value = 0;
            ParseINIPersonalSettings();
            commands = new Dictionary<string, Command>();
            superCommands = new Dictionary<string, SuperCommand>();
            baseBattleRamLocations = new Dictionary<string, uint>();
            userDefinitions = new Dictionary<string, uint>();
            keywords = new Dictionary<string, uint>();
            pointerNames = new Dictionary<DynamicPointer, bool>();
            dynamicStrings = new Dictionary<string, string>();
            decompiledOffsets = new List<int>();
            scripts = new List<RichTextBox>();
            unsaved = new List<bool>();
            currentlyOpenBS = new List<string>();
            newScriptNames = new List<int>();
            FixedRichTextBox rtb;
            LineNumbersControlForRichTextBox.LineNumbersForRichText lineNos;
            CreateRTBAndNumbersList(out rtb, out lineNos);
            rtbNotes.BackColor = rtbBackColour;
            rtbNotes.Font = rtbFont;
            rtbNotes.ForeColor = rtbForeColour;
            this.tabPage1.Controls.Add(rtb);
            this.tabPage1.Controls.Add(lineNos);
            unsaved.Add(false);
            newScriptNames.Add(1);
            scriptInserted = false;
            insideHashOrgBracket = false;
            freeSpaceLocation = 0;
            dynamicLocationSet = false;
            currentlyOpenBS.Add(null);
            freeSpaceByte = 0xFF;
            LoadSuperCommands();
            LoadCustomCommands();
        }

        private void ParseINIPersonalSettings()
        {
            string[] ini = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini");
            foreach (string s in ini)
            {
                if (s.StartsWith("backcolour"))
                {
                    rtbBackColour = Color.FromName(s.Split('=')[1]);
                }
                else if (s.StartsWith("textcolour"))
                {
                    rtbForeColour = Color.FromName(s.Split('=')[1]);
                }
                else if (s.StartsWith("font="))
                {
                    rtbFontName = s.Split('=')[1];
                }
                else if (s.StartsWith("fontstyle"))
                {
                    rtbFontStyle = (FontStyle)Enum.Parse(typeof(FontStyle), s.Split('=')[1], true);
                }
                else if (s.StartsWith("fontsize"))
                {
                    rtbFont = new Font(rtbFontName, (float)(Double.Parse(s.Split('=')[1])), rtbFontStyle, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                }
                else if (s.StartsWith("commentString"))
                {
                    commentString = s.Split('=')[1];
                }
                else if (s.StartsWith("decompileOffsets"))
                {
                    decompileCommandOffsetsToolStripMenuItem.Checked = Boolean.Parse(s.Split('=')[1]);
                }
                else if (s.StartsWith("decompileMode"))
                {
                    SetDecompileModeByString(s.Split('=')[1]);
                }
                else if (s.StartsWith("numberDecompileMode"))
                {
                    SetNumberDecompileModeByString(s.Split('=')[1]);
                }
            }
        }

        private void CreateRTBAndNumbersList(out FixedRichTextBox richTextBox1, out LineNumbersControlForRichTextBox.LineNumbersForRichText lineNumbersForRichText1)
        {
            richTextBox1 = new FixedRichTextBox();
            richTextBox1.AcceptsTab = true;
            richTextBox1.BackColor = rtbBackColour;
            richTextBox1.ContextMenuStrip = this.contextMenuStrip1;
            richTextBox1.DetectUrls = false;
            richTextBox1.Font = rtbFont;
            richTextBox1.ForeColor = rtbForeColour;
            richTextBox1.Location = new System.Drawing.Point(36, 0);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            richTextBox1.Size = new System.Drawing.Size(528, 362);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            richTextBox1.WordWrap = false;
            richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            scripts.Add(richTextBox1);
            lineNumbersForRichText1 = new LineNumbersControlForRichTextBox.LineNumbersForRichText();
            lineNumbersForRichText1.AutoSizing = false;
            lineNumbersForRichText1.BackColor = System.Drawing.SystemColors.Control;
            lineNumbersForRichText1.BackgroundGradientAlphaColor = System.Drawing.Color.Transparent;
            lineNumbersForRichText1.BackgroundGradientBetaColor = System.Drawing.Color.Transparent;
            lineNumbersForRichText1.BackgroundGradientDirection = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
            lineNumbersForRichText1.BorderLinesColor = System.Drawing.Color.Transparent;
            lineNumbersForRichText1.BorderLinesStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            lineNumbersForRichText1.BorderLinesThickness = 1F;
            lineNumbersForRichText1.DockSide = LineNumbersControlForRichTextBox.LineNumbersForRichText.LineNumberDockSide.Left;
            lineNumbersForRichText1.GridLinesColor = System.Drawing.Color.Transparent;
            lineNumbersForRichText1.GridLinesStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            lineNumbersForRichText1.GridLinesThickness = 1F;
            lineNumbersForRichText1.LineNumbersAlignment = System.Drawing.ContentAlignment.MiddleRight;
            lineNumbersForRichText1.LineNumbersAntiAlias = true;
            lineNumbersForRichText1.LineNumbersAsHexadecimal = false;
            lineNumbersForRichText1.LineNumbersClippedByItemRectangle = true;
            lineNumbersForRichText1.LineNumbersLeadingZeroes = true;
            lineNumbersForRichText1.LineNumbersOffset = new System.Drawing.Size(0, 0);
            lineNumbersForRichText1.Location = new System.Drawing.Point(-8, 0);
            lineNumbersForRichText1.Margin = new System.Windows.Forms.Padding(0);
            lineNumbersForRichText1.MarginLinesColor = System.Drawing.Color.Transparent;
            lineNumbersForRichText1.MarginLinesSide = LineNumbersControlForRichTextBox.LineNumbersForRichText.LineNumberDockSide.None;
            lineNumbersForRichText1.MarginLinesStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            lineNumbersForRichText1.MarginLinesThickness = 1F;
            lineNumbersForRichText1.Name = "lineNumbersForRichText1";
            lineNumbersForRichText1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            lineNumbersForRichText1.ParentRichTextBox = richTextBox1;
            lineNumbersForRichText1.SeeThroughMode = false;
            lineNumbersForRichText1.ShowBackgroundGradient = true;
            lineNumbersForRichText1.ShowBorderLines = false;
            lineNumbersForRichText1.ShowGridLines = false;
            lineNumbersForRichText1.ShowLineNumbers = true;
            lineNumbersForRichText1.ShowMarginLines = true;
            lineNumbersForRichText1.Size = new System.Drawing.Size(43, 362);
            lineNumbersForRichText1.TabIndex = 12;
        }

        private void LoadSuperCommands()
        {
            List<string> temp = new List<string>();
            List<byte> temp2 = new List<byte>();
            temp.Add("Type");
            temp2.Add(1);
            superCommands.Add("ForceType".ToUpper(), new SuperCommand(1, temp.ToArray(), temp2.ToArray()));
            temp.Add("Message Wait");
            temp[0] = "Message Location";
            temp2[0] = 4;
            temp2.Add(2);
            superCommands.Add("PrintMessage".ToUpper(), new SuperCommand(2, temp.ToArray(), temp2.ToArray()));
            temp.Clear();
            temp.Add("Memory Address");
            temp.Add("Half-Word to store");
            temp2.Clear();
            temp2.Add(4);
            temp2.Add(2);
            superCommands.Add("SetHalfWord".ToUpper(), new SuperCommand(2, temp.ToArray(), temp2.ToArray()));
            temp[1] = "Word to store";
            temp2[1] = 4;
            superCommands.Add("SetWord".ToUpper(), new SuperCommand(2, temp.ToArray(), temp2.ToArray()));
            superCommands.Add("CalculateDamage".ToUpper(), new SuperCommand(0));
        }

        private byte[] PrintMessageSuperCommand(byte slotID, uint romPointer, ushort messageWait)
        {
            byte[] message;
            if (commands.ContainsKey("SETWORD"))
            {
                ushort commandID = commands["SETWORD"].HexID;
                byte length = 1;
                if (commandID > 0x100)
                {
                    length = 2;
                }
                message = new byte[length + 8];
                for (int i = 0; i < length; i++)
                {
                    message[i] = (Byte.Parse(ToDecimal("0x" + commandID.ToString("X4").Substring(4 - ((i * 2) + 2), 2))));
                }
                message[length] = 0x24;
                message[length + 1] = 0xC0;
                message[length + 2] = 0x3;
                message[length + 3] = 0x2;
                if (romPointer < 0x8000000)
                {
                    romPointer += 0x8000000;
                }
                for (int i = 0; i < 4; i++)
                {
                    message[length + 4 + i] = (Byte.Parse(ToDecimal("0x" + romPointer.ToString("X8").Substring(8 - ((i * 2) + 2), 2))));
                }
            }
            else
            {
                message = SetWordSuperCommand(romPointer, 0x0203C024);
            }
            byte[] stuff = new byte[14 + message.Length];
            for (int i = 0; i < message.Length; i++)
            {
                stuff[i] = message[i];
            }
            stuff[message.Length] = 0x2E;
            stuff[1 + message.Length] = 0x87;
            stuff[2 + message.Length] = 0x3E;
            stuff[3 + message.Length] = 0x2;
            stuff[4 + message.Length] = 0x2;
            stuff[5 + message.Length] = 0x0;
            stuff[6 + message.Length] = 0x13;
            stuff[7 + message.Length] = 0x24;
            stuff[8 + message.Length] = 0xC0;
            stuff[9 + message.Length] = 0x3;
            stuff[10 + message.Length] = 0x2;
            stuff[11 + message.Length] = 0x12;
            for (int i = 0; i < 2; i++)
            {
                stuff[12 + i + message.Length] = (Byte.Parse(ToDecimal("0x" + messageWait.ToString("X4").Substring(4 - ((i * 2) + 2), 2))));
            }
            return stuff;
        }

        private byte[] CalculateDamageSuperCommand()
        {
            return new byte[] { 04, 05, 06, 07 };
        }

        private byte[] ForceTypeSuperCommand(byte type)
        {
            if (type > 0x11 || type == 0x9)
            {
                type = 0;
            }
            byte[] stuff = new byte[12];
            stuff[0] = 0x2E;
            stuff[1] = 0xD2;
            stuff[2] = 0x3F;
            for (int i = 0; i < 3; i++)
            {
                stuff[3 + i] = 2;
            }
            stuff[6] = 0x2E;
            stuff[7] = 0xFB;
            stuff[8] = 0x3F;
            for (int i = 0; i < 2; i++)
            {
                stuff[9 + i] = 2;
            }
            stuff[11] = Convert.ToByte((0x80 + type));
            return stuff;
        }

        private byte[] SetHalfWordSuperCommand(ushort value, uint memoryAddress)
        {
            byte[] stuff = new byte[12];
            for (int i = 0; i < 2; i++)
            {
                stuff[i * 6] = 0x2E;
                for (int j = 0; j < 4; j++)
                {
                    stuff[((i * 6) + 1) + j] = Byte.Parse(ToDecimal("0x" + (memoryAddress + i).ToString("X8").Substring(8 - ((j * 2) + 2), 2)));
                }
                stuff[(i * 6) + 5] = Byte.Parse(ToDecimal("0x" + value.ToString("X4").Substring(4 - ((i * 2) + 2), 2)));
            }
            return stuff;
        }

        private byte[] SetWordSuperCommand(uint value, uint memoryAddress)
        {
            byte[] stuff = new byte[24];
            for (int i = 0; i < 4; i++)
            {
                stuff[i * 6] = 0x2E;
                for (int j = 0; j < 4; j++)
                {
                    stuff[((i * 6) + 1) + j] = Byte.Parse(ToDecimal("0x" + (memoryAddress + i).ToString("X8").Substring(8 - ((j * 2) + 2), 2)));
                }
                stuff[(i * 6) + 5] = Byte.Parse(ToDecimal("0x" + value.ToString("X8").Substring(8 - ((i * 2) + 2), 2)));
            }
            return stuff;
        }

        private void LoadCustomCommands()
        {
            string filePath = System.Windows.Forms.Application.StartupPath + @"\Data\commands.bsh";
            if (File.Exists(filePath))
            {
                string[] file = File.ReadAllLines(filePath);
                foreach (string line in file)
                {
                    if (line.StartsWith("#command"))
                    {
                        string[] theLine = line.Split(' ');
                        List<string> paramNames = new List<string>();
                        List<byte> paramLengths = new List<byte>();
                        byte numberOfParams = 0;
                        ushort commandId = 0;
                        byte paramLength = 0;
                        string commandName = "";
                        string paramName = "";
                        bool getString = false;
                        bool getParamLength = false;
                        for (int i = 1; i < theLine.Length; i++)
                        {
                            if (i == 1)
                            {
                                commandName = theLine[i];
                                continue;
                            }
                            if (i == 2)
                            {
                                bool success = UInt16.TryParse(theLine[i], out commandId);
                                if (!success)
                                {
                                    success = UInt16.TryParse(ToDecimal(theLine[i]), out commandId);
                                    if (!success)
                                    {
                                        MessageBox.Show("The command ID was not formatted correctly. A number is required");
                                    }
                                }
                                continue;
                            }
                            if (i == 3)
                            {
                                bool success = Byte.TryParse(theLine[i], out numberOfParams);
                                if (!success)
                                {
                                    success = Byte.TryParse(ToDecimal(theLine[i]), out numberOfParams);
                                    if (!success)
                                    {
                                        MessageBox.Show("The number of parameters was not formatted correctly. A number is required");
                                    }
                                }
                                continue;
                            }
                            if (theLine[i].StartsWith("\""))
                            {
                                if (!theLine[i].EndsWith("\""))
                                {
                                    getString = true;
                                }
                                else
                                {
                                    getParamLength = true;
                                    paramName = theLine[i].Substring(1, theLine[i].Length - 2);
                                    continue;
                                }
                                paramName += theLine[i].Substring(1);
                                continue;
                            }
                            if (getString)
                            {
                                if (theLine[i].EndsWith("\""))
                                {
                                    getString = false;
                                    getParamLength = true;
                                    paramName += " " + theLine[i].Substring(0, theLine[i].Length - 1);
                                    continue;
                                }
                                paramName += " " + theLine[i];
                            }
                            if (getParamLength)
                            {
                                getParamLength = false;
                                bool success = Byte.TryParse(theLine[i], out paramLength);
                                if (!success)
                                {
                                    success = Byte.TryParse(ToDecimal(theLine[i]), out paramLength);
                                    if (!success)
                                    {
                                        MessageBox.Show("The parameter length was not formatted correctly. A number is required");
                                    }
                                }
                                paramNames.Add(paramName);
                                paramLengths.Add(paramLength);
                                paramName = "";
                                paramLength = 0;
                                continue;
                            }
                        }
                        commands.Add(commandName.ToUpper(), new Command(commandId, numberOfParams, paramNames.ToArray(), paramLengths.ToArray()));
                    }
                }
            }
        }

        private ReturnWorked ParseByteParameter(string input, int lineNumber, int parameterNumber)
        {
            uint temp = 0;
            bool succeeded = UInt32.TryParse(input, out temp);
            if (!succeeded)
            {
                byte value = 0;
                try
                {
                    value = Byte.Parse(ToDecimal(input));
                }
                catch (OverflowException)
                {
                    return (new ReturnWorked(false, Convert.ToByte(0)));
                }
                catch
                {
                    uint potentialValue = 0;
                    bool success = userDefinitions.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        try
                        {
                            byte toReturn = Convert.ToByte(potentialValue);
                            return (new ReturnWorked(true, toReturn));
                        }
                        catch
                        {
                            return (new ReturnWorked(false, Convert.ToByte(0)));
                        }
                    }
                    success = keywords.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        try
                        {
                            byte toReturn = Convert.ToByte(potentialValue);
                            return (new ReturnWorked(true, toReturn));
                        }
                        catch
                        {
                            return (new ReturnWorked(false, Convert.ToByte(0)));
                        }
                    }
                    return (new ReturnWorked(false, Convert.ToByte(0)));
                }
                return (new ReturnWorked(true, value));
            }
            else
            {
                byte value = 0;
                try
                {
                    value = Byte.Parse(input);
                }
                catch (OverflowException)
                {
                    return (new ReturnWorked(false, Convert.ToByte(0)));
                }
                catch
                {
                    uint potentialValue = 0;
                    bool success = userDefinitions.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        try
                        {
                            byte toReturn = Convert.ToByte(potentialValue);
                            return (new ReturnWorked(true, toReturn));
                        }
                        catch
                        {
                            return (new ReturnWorked(false, Convert.ToByte(0)));
                        }
                    }
                    success = keywords.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        try
                        {
                            byte toReturn = Convert.ToByte(potentialValue);
                            return (new ReturnWorked(true, toReturn));
                        }
                        catch
                        {
                            return (new ReturnWorked(false, Convert.ToByte(0)));
                        }
                    }
                    return (new ReturnWorked(false, "Error parsing parameter on line " + lineNumber + ", parameter " + (parameterNumber + 1), false));
                }
                return (new ReturnWorked(true, value));
            }
        }

        private ReturnWorked ParseHalfWordParameter(string input, int lineNumber, int parameterNumber)
        {
            uint temp = 0;
            bool succeeded = UInt32.TryParse(input, out temp);
            if (!succeeded)
            {
                ushort value = 0;
                try
                {
                    value = UInt16.Parse(ToDecimal(input));
                }
                catch (OverflowException)
                {
                    return (new ReturnWorked(false, Convert.ToUInt16(0)));
                }
                catch
                {
                    uint potentialValue = 0;
                    bool success = userDefinitions.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        try
                        {
                            ushort toReturn = Convert.ToUInt16(potentialValue);
                            return (new ReturnWorked(true, toReturn));
                        }
                        catch
                        {
                            return (new ReturnWorked(false, Convert.ToUInt16(0)));
                        }
                    }
                    success = keywords.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        try
                        {
                            ushort toReturn = Convert.ToUInt16(potentialValue);
                            return (new ReturnWorked(true, toReturn));
                        }
                        catch
                        {
                            return (new ReturnWorked(false, Convert.ToUInt16(0)));
                        }
                    }
                    return (new ReturnWorked(false, Convert.ToUInt16(0)));
                }
                return (new ReturnWorked(true, value));
            }
            else
            {
                ushort value = 0;
                try
                {
                    value = UInt16.Parse(input);
                }
                catch (OverflowException)
                {
                    return (new ReturnWorked(false, Convert.ToUInt16(0)));
                }
                catch
                {
                    uint potentialValue = 0;
                    bool success = userDefinitions.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        try
                        {
                            ushort toReturn = Convert.ToUInt16(potentialValue);
                            return (new ReturnWorked(true, toReturn));
                        }
                        catch
                        {
                            return (new ReturnWorked(false, Convert.ToUInt16(0)));
                        }
                    }
                    success = keywords.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        try
                        {
                            ushort toReturn = Convert.ToUInt16(potentialValue);
                            return (new ReturnWorked(true, toReturn));
                        }
                        catch
                        {
                            return (new ReturnWorked(false, Convert.ToUInt16(0)));
                        }
                    }
                    return (new ReturnWorked(false, "Error parsing parameter on line " + lineNumber + ", parameter " + (parameterNumber + 1), false));
                }
                return (new ReturnWorked(true, value));
            }
        }

        private ReturnWorked ParseWordParameter(string input, int lineNumber, int parameterNumber, bool dynamicPointersAllowed)
        {
            uint temp = 0;
            bool succeeded = UInt32.TryParse(input, out temp);
            if (!succeeded && !input.StartsWith("@"))
            {
                uint value = 0;
                try
                {
                    value = UInt32.Parse(ToDecimal(input));
                }
                catch (OverflowException)
                {
                    return (new ReturnWorked(false, Convert.ToUInt32(0)));
                }
                catch
                {
                    uint potentialValue = 0;
                    bool success = userDefinitions.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        return (new ReturnWorked(true, potentialValue));
                    }
                    success = keywords.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        return (new ReturnWorked(true, potentialValue));
                    }
                    return (new ReturnWorked(false, 0));
                }
                return (new ReturnWorked(true, value));
            }
            else if (input.StartsWith("@"))
            {
                if (dynamicPointersAllowed)
                {
                    if (dynamicLocationSet)
                    {
                        bool notIn = true;
                        foreach (DynamicPointer dp in pointerNames.Keys)
                        {
                            if (dp.Name.Equals(input.Substring(1)))
                            {
                                notIn = false;
                                break;
                            }
                        }
                        if (notIn)
                        {
                            pointerNames.Add(new DynamicPointer(input.Substring(1), lineNumber, parameterNumber), false);
                        }
                        return (new ReturnWorked(true, input.Substring(1), true));
                    }
                    else
                    {
                        return (new ReturnWorked(false, "Dynamic search location not set", false));
                    }
                }
                else
                {
                    return (new ReturnWorked(false, "Dynamic Pointers Not Allowed in command at line " + lineNumber + ", parameter " + parameterNumber, false));
                }
            }
            else
            {
                uint value = 0;
                try
                {
                    value = UInt32.Parse(input);
                }
                catch (OverflowException)
                {
                    return (new ReturnWorked(false, Convert.ToUInt32(0)));
                }
                catch
                {
                    uint potentialValue = 0;
                    bool success = userDefinitions.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        return (new ReturnWorked(true, potentialValue));
                    }
                    success = keywords.TryGetValue(input.ToUpper(), out potentialValue);
                    if (success)
                    {
                        return (new ReturnWorked(true, potentialValue));
                    }
                    return (new ReturnWorked(false, "Error parsing parameter on line " + lineNumber + ", parameter " + (parameterNumber + 1), false));
                }
                return (new ReturnWorked(true, value));
            }
        }

        private string ToDecimal(string input)
        {
            if (input.ToLower().StartsWith("0x") || input.ToUpper().StartsWith("&H"))
            {
                return Convert.ToUInt32(input.Substring(2), 16).ToString();
            }
            else if (input.ToLower().StartsWith("0o"))
            {
                return Convert.ToUInt32(input.Substring(2), 8).ToString();
            }
            else if (input.ToLower().StartsWith("0b"))
            {
                return Convert.ToUInt32(input.Substring(2), 2).ToString();
            }
            else if (input.ToLower().StartsWith("0t"))
            {
                return ThornalToDecimal(input.Substring(2));
            }
            else if ((input.StartsWith("[") && input.EndsWith("]")) || (input.StartsWith("{") && input.EndsWith("}")))
            {
                return Convert.ToUInt32(input.Substring(1, (input.Length - 2)), 2).ToString();
            }
            else if (input.ToLower().EndsWith("h"))
            {
                return Convert.ToUInt32(input.Substring(0, (input.Length - 1)), 16).ToString();
            }
            else if (input.ToLower().EndsWith("b"))
            {
                return Convert.ToUInt32(input.Substring(0, (input.Length - 1)), 2).ToString();
            }
            else if (input.ToLower().EndsWith("t"))
            {
                return ThornalToDecimal(input.Substring(0, (input.Length - 1)));
            }
            else if (input.StartsWith("$"))
            {
                return Convert.ToUInt32(input.Substring(1), 16).ToString();
            }
            else
            {
                return Convert.ToUInt32(input, 16).ToString();
            }
        }

        private string ThornalToDecimal(string input)
        {
            uint total = 0;
            char[] temp = input.ToCharArray();
            for (int i = input.Length - 1; i >= 0; i--)
            {
                int value = 0;
                bool success = Int32.TryParse(temp[i].ToString(), out value);
                if (!success)
                {
                    if (temp[i] < 'W' && temp[i] >= 'A')
                    {
                        value = temp[i] - 'A' + 10;
                    }
                    else
                    {
                        throw new FormatException(temp[i] + " is an invalid character in the Base 32 number set.");
                    }
                }
                total += (uint)(Math.Pow((double)32, (double)(input.Length - 1 - i)) * value);
            }
            return total.ToString();
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            toolStripStatusLabel1.Text = "Loading Dummy Data...";
            string romCode = "TEST";
            string iniPath = System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini";
            if (File.Exists(iniPath))
            {
                string[] iniFile = File.ReadAllLines(iniPath);
                ParseINI(iniFile, romCode);
            }
            iniPath = System.Windows.Forms.Application.StartupPath + @"\Data\std.bsh";
            if (File.Exists(iniPath))
            {
                string[] iniFile = File.ReadAllLines(iniPath);
                ParseHeaderFile(iniFile);
            }
            toolStripStatusLabel1.Text = "Starting Debug...";
            bool success = DebugWrittenScript();
            if (success)
            {
                MessageBox.Show("No errors found.");
            }
            keywords.Clear();
            pointerNames.Clear();
            this.Cursor = Cursors.Default;
        }

        private bool DebugDynamicPointerNames()
        {
            List<DynamicPointer> tempList = new List<DynamicPointer>();
            int foundMatches = 0;
            Dictionary<DynamicPointer, int> values = new Dictionary<DynamicPointer, int>();
            foreach (DynamicPointer dp in pointerNames.Keys)
            {
                tempList.Add(dp);
                values.Add(dp, 0);
            }
            foreach (DynamicPointer dp in tempList)
            {
                foreach (string line in scripts[tabControl1.SelectedIndex].Lines)
                {
                    if (line.StartsWith("#org ") && dp.Name.Equals(line.Split(' ')[1].Substring(1)))
                    {
                        pointerNames[dp] = true;
                        values[dp]++;
                        if (foundMatches != 1)
                        {
                            pointerNames[dp] = false;
                        }
                    }
                }
            }
            foreach (DynamicPointer dp in pointerNames.Keys)
            {
                if (!pointerNames[dp] && values[dp] == 0)
                {
                    MessageBox.Show("The dynamic pointer \"@" + dp.Name + "\" (on line " + dp.LineNumber + ", parameter number " + (dp.ParameterNumber + 1) + ") does not have a valid value associated with it.");
                    return false;
                }
                else if (!pointerNames[dp] && values[dp] > 1)
                {
                    MessageBox.Show("The dynamic pointer \"@" + dp.Name + "\" (on line " + dp.LineNumber + ", parameter number " + (dp.ParameterNumber + 1) + ") has more than one valid value associated with it.\nDid you accidentally duplicate a #org line?");
                    return false;
                }
            }
            return true;
        }

        private bool DebugWrittenScript()
        {
            toolStripStatusLabel1.Text = "Looking for strings...";
            FindStrings();
            toolStripStatusLabel1.Text = "Checking Commands...";
            bool result = DebugCommands();
            if (result)
            {
                toolStripStatusLabel1.Text = "Checking number of parameters...";
                result = DebugNumberOfParameters();
                if (result)
                {
                    toolStripStatusLabel1.Text = "Checking Parameter Content...";
                    result = DebugParameters();
                    if (result)
                    {
                        toolStripStatusLabel1.Text = "Checking Dynamic Pointers...";
                        result = DebugDynamicPointerNames();
                        if (result)
                        {
                            toolStripStatusLabel1.Text = "Done!";
                        }
                        else
                        {
                            toolStripStatusLabel1.Text = "Invalid Dynamic Pointer(s) Detected!";
                        }
                    }
                    else
                    {
                        toolStripStatusLabel1.Text = "Invalid Parameter Contents Detected!";
                    }
                }
                else
                {
                    toolStripStatusLabel1.Text = "Invalid Number of Parameters Detected!";
                }
            }
            else
            {
                toolStripStatusLabel1.Text = "Invalid Commands Detected!";
            }
            dynamicStrings.Clear();
            userDefinitions.Clear();
            dynamicLocationSet = false;
            return result;
        }

        private void FindStrings()
        {
            bool checkNextLine = false;
            string name = "";
            foreach (string s in scripts[tabControl1.SelectedIndex].Lines)
            {
                if (s.StartsWith("#org "))
                {
                    checkNextLine = true;
                    name = s.Split(' ')[1];
                    continue;
                }
                if (checkNextLine)
                {
                    if (s.StartsWith("="))
                    {
                        dynamicStrings.Add(name, s.Substring(2));
                    }
                    checkNextLine = false;
                }
            }
        }

        private bool DebugCommands()
        {
            int lineNumber = 1;
            foreach (string line in scripts[tabControl1.SelectedIndex].Lines)
            {
                if (!line.Equals("") && !line.StartsWith("#") && !line.StartsWith("="))
                {
                    if (insideHashOrgBracket)
                    {
                        string newLine = line;
                        if (line.Contains(commentString))
                        {
                            newLine = Regex.Split(newLine, commentString)[0].TrimEnd();
                        }
                        string[] info = newLine.Split(' ');
                        Command c;
                        bool success = commands.TryGetValue(info[0].ToUpper(), out c);
                        if (!success)
                        {
                            SuperCommand sc;
                            success = superCommands.TryGetValue(info[0].ToUpper(), out sc);
                            if (!success)
                            {
                                SQLiteConnection con = new SQLiteConnection("Data Source=" + System.Windows.Forms.Application.StartupPath + @"\Data\Commands.sqlite;Version=3;");
                                con.Open();
                                SQLiteCommand com = new SQLiteCommand("select * from commands where name = '" + info[0].ToUpper() + "'", con);
                                SQLiteDataReader reader = com.ExecuteReader();
                                if (reader.HasRows)
                                {
                                    c = GetCommand(reader, con);
                                }
                                else
                                {
                                    con.Close();
                                    MessageBox.Show("The command \"" + info[0] + "\"on line " + lineNumber + " is not recognised as a valid command.");
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        string[] info = line.Split(' ');
                        MessageBox.Show("The command \"" + info[0] + "\" on line " + lineNumber + " is not inside a valid #org compilation area.");
                        return false;
                    }
                }
                else if (line.StartsWith("#org "))
                {
                    uint temp;
                    if ((line.Split(' ')[1].StartsWith("@") && (line.Split(' ').Length == 2)) || UInt32.TryParse(line.Split(' ')[1], out temp) || UInt32.TryParse(ToDecimal(line.Split(' ')[1]), out temp))
                    {
                        insideHashOrgBracket = true;
                    }
                    else
                    {
                        MessageBox.Show("The script fragment \"" + line.Substring(5) + "\" on line " + lineNumber + " is not a valid pointer. It should be either a 32-bit ROM address or a dynamic pointer beginning with the character \"@\".");
                        return false;
                    }
                }
                lineNumber++;
            }
            return true;
        }

        private bool DebugNumberOfParameters()
        {
            int lineNumber = 1;
            foreach (string line in scripts[tabControl1.SelectedIndex].Lines)
            {
                if (!line.Equals("") && !line.StartsWith("#") && !line.StartsWith("="))
                {
                    string newLine = line;
                    if (line.Contains(commentString))
                    {
                        newLine = Regex.Split(newLine, commentString)[0].TrimEnd();
                    }
                    string[] info = newLine.Split(' ');                                 
                    Command c;
                    bool success = commands.TryGetValue(info[0].ToUpper(), out c);
                    if (!success)
                    {
                        SuperCommand sc; 
                        success = superCommands.TryGetValue(info[0].ToUpper(), out sc);
                        if (success)
                        {
                            if (info.Length - 1 < sc.NumberOfParameters)
                            {
                                MessageBox.Show("Line " + lineNumber + " has too few parameters. Correct number is " + sc.NumberOfParameters);
                                return false;
                            }
                            else if (info.Length - 1 > sc.NumberOfParameters)
                            {
                                MessageBox.Show("Line " + lineNumber + " has too many parameters. Correct number is " + sc.NumberOfParameters);
                                return false;
                            }
                        }
                        else
                        {
                            SQLiteConnection con = new SQLiteConnection("Data Source=" + System.Windows.Forms.Application.StartupPath + @"\Data\Commands.sqlite;Version=3;");
                            con.Open();
                            SQLiteCommand com = new SQLiteCommand("select * from commands where name = '" + info[0].ToUpper() + "'", con);
                            SQLiteDataReader reader = com.ExecuteReader();
                            if (reader != null)
                            {
                                c = GetCommand(reader, con);
                                if (info.Length - 1 < c.NumberOfParameters)
                                {
                                    MessageBox.Show("Line " + lineNumber + " has too few parameters. Correct number is " + c.NumberOfParameters);
                                    return false;
                                }
                                else if (info.Length - 1 > c.NumberOfParameters)
                                {
                                    MessageBox.Show("Line " + lineNumber + " has too many parameters. Correct number is " + c.NumberOfParameters);
                                    return false;
                                }
                            }
                            else
                            {
                                con.Close();
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (info.Length - 1 < c.NumberOfParameters)
                        {
                            MessageBox.Show("Line " + lineNumber + " has too few parameters. Correct number is " + c.NumberOfParameters);
                            return false;
                        }
                        else if (info.Length - 1 > c.NumberOfParameters)
                        {
                            MessageBox.Show("Line " + lineNumber + " has too many parameters. Correct number is " + c.NumberOfParameters);
                            return false;
                        }
                    }
                }
                lineNumber++;
            }
            return true;
        }

        private Command GetCommand(SQLiteDataReader reader, SQLiteConnection con)
        {
            Command c = new Command(0, 0);
            while (reader.Read())
            {
                if (Int32.Parse(reader["numberofparameters"].ToString()) != 0)
                {
                    List<string> parameterNames = new List<string>();
                    SQLiteCommand com2 = new SQLiteCommand("select name from parameternames where id = " + Byte.Parse(reader["id"].ToString()), con);
                    SQLiteDataReader reader2 = com2.ExecuteReader();
                    while (reader2.Read())
                    {
                        parameterNames.Add(reader2["name"].ToString());
                    }
                    List<byte> parameterLengths = new List<byte>();
                    com2 = new SQLiteCommand("select length from parameterlengths where id = " + Byte.Parse(reader["id"].ToString()), con);
                    reader2 = com2.ExecuteReader();
                    while (reader2.Read())
                    {
                        parameterLengths.Add(Byte.Parse(reader2["length"].ToString()));
                    }
                    c = new Command(UInt16.Parse(reader["id"].ToString()), Byte.Parse(reader["numberofparameters"].ToString()), parameterNames.ToArray(), parameterLengths.ToArray());
                }
                else
                {
                    c = new Command(UInt16.Parse(reader["id"].ToString()), Byte.Parse(reader["numberofparameters"].ToString()));
                }
            }
            con.Close();
            return c;
        }

        private bool DebugParameters()
        {
            int lineNumber = 1;
            foreach (string line in scripts[tabControl1.SelectedIndex].Lines)
            {
                if (!line.Equals("") && !line.StartsWith("#") && !line.StartsWith("="))
                {
                    string newLine = line;
                    if (line.Contains(commentString))
                    {
                        newLine = Regex.Split(newLine, commentString)[0].TrimEnd();
                    }
                    string[] info = newLine.Split(' ');
                    Command c;
                    bool success = commands.TryGetValue(info[0].ToUpper(), out c);
                    if (success)
                    {
                        byte[] parameterLengths = c.ParameterLengths;
                        for (int parameterIndex = 0; parameterIndex < c.NumberOfParameters; parameterIndex++)
                        {
                            if (c.HexID == 0x89 && parameterIndex == 0)
                            {
                                byte paramTwo;
                                if (info[2].ToUpper().Equals("TRUE"))
                                {
                                    paramTwo = 0x40;
                                }
                                else if (info[2].ToUpper().Equals("FALSE"))
                                {
                                    paramTwo = 0;
                                }
                                else
                                {
                                    paramTwo = ParseByteParameter(info[2], lineNumber, 2).Byte;
                                    if (paramTwo != 0x40)
                                    {
                                        paramTwo = 0;
                                    }
                                }
                                byte byteOne = (byte)((int)ParseByteParameter(info[1], lineNumber, 1).Byte | (int)paramTwo);
                                bool value = ParseByteParameter(info[1], lineNumber, 1).Result;
                                if (!value)
                                {
                                    MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is invalid. Maximum value is 0xFFFF or 65535.");
                                    return false;
                                }
                                parameterIndex++;
                            }
                            else
                            {
                                switch (parameterLengths[parameterIndex])
                                {
                                    case 2:
                                        {
                                            ReturnWorked test = ParseHalfWordParameter(info[parameterIndex + 1], lineNumber, parameterIndex);
                                            if (!test.Result)
                                            {
                                                if (test.GeneralReason == null)
                                                {
                                                    MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is too long. Maximum value is 0xFF or 255.");
                                                }
                                                else
                                                {
                                                    MessageBox.Show(test.GeneralReason);
                                                }
                                                return false;
                                            }
                                            break;
                                        }
                                    case 4:
                                        {
                                            ReturnWorked test = ParseWordParameter(info[parameterIndex + 1], lineNumber, parameterIndex, true);
                                            if (!test.Result)
                                            {
                                                if (test.GeneralReason == null)
                                                {
                                                    MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is too long. Maximum value is 0xFF or 255.");
                                                }
                                                else
                                                {
                                                    MessageBox.Show(test.GeneralReason);
                                                }
                                                return false;
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            ReturnWorked test = ParseByteParameter(info[parameterIndex + 1], lineNumber, parameterIndex);
                                            if (!test.Result)
                                            {
                                                if (test.GeneralReason == null)
                                                {
                                                    MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is too long. Maximum value is 0xFF or 255.");
                                                }
                                                else
                                                {
                                                    MessageBox.Show(test.GeneralReason);
                                                }
                                                return false;
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                    }
                    else
                    {
                        SQLiteConnection con = new SQLiteConnection("Data Source=" + System.Windows.Forms.Application.StartupPath + @"\Data\Commands.sqlite;Version=3;");
                        con.Open();
                        SQLiteCommand com = new SQLiteCommand("select * from commands where name = '" + info[0].ToUpper() + "'", con);
                        SQLiteDataReader reader = com.ExecuteReader();
                        if (reader != null)
                        {
                            c = GetCommand(reader, con);
                            byte[] parameterLengths = c.ParameterLengths;
                            for (int parameterIndex = 0; parameterIndex < c.NumberOfParameters; parameterIndex++)
                            {
                                if (c.HexID == 0x89 && parameterIndex == 0)
                                {
                                    byte paramTwo;
                                    if (info[2].ToUpper().Equals("TRUE"))
                                    {
                                        paramTwo = 0x40;
                                    }
                                    else if (info[2].ToUpper().Equals("FALSE"))
                                    {
                                        paramTwo = 0;
                                    }
                                    else
                                    {
                                        paramTwo = ParseByteParameter(info[2], lineNumber, 2).Byte;
                                        if (paramTwo != 0x40)
                                        {
                                            paramTwo = 0;
                                        }
                                    }
                                    byte byteOne = (byte)((int)ParseByteParameter(info[1], lineNumber, 1).Byte | (int)paramTwo);
                                    bool value = ParseByteParameter(info[1], lineNumber, 1).Result;
                                    if (!value)
                                    {
                                        MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is invalid. Maximum value is 0xFFFF or 65535.");
                                        return false;
                                    }
                                    parameterIndex++;
                                }
                                else
                                {
                                    switch (parameterLengths[parameterIndex])
                                    {
                                        case 2:
                                            {
                                                ReturnWorked test = ParseHalfWordParameter(info[parameterIndex + 1], lineNumber, parameterIndex);
                                                if (!test.Result)
                                                {
                                                    if (test.GeneralReason == null)
                                                    {
                                                        MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is too long. Maximum value is 0xFFFFFFFF or 4294967295.");
                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show(test.GeneralReason);
                                                    }
                                                    return false;
                                                }
                                                break;
                                            }
                                        case 4:
                                            {
                                                ReturnWorked test = ParseWordParameter(info[parameterIndex + 1], lineNumber, parameterIndex, true);
                                                if (!test.Result)
                                                {
                                                    if (test.GeneralReason == null)
                                                    {
                                                        MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is too long. Maximum value is 0xFFFFFFFF or 4294967295.");
                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show(test.GeneralReason);
                                                    }
                                                    return false;
                                                }
                                                break;
                                            }
                                        default:
                                            {
                                                ReturnWorked test = ParseByteParameter(info[parameterIndex + 1], lineNumber, parameterIndex);
                                                if (!test.Result)
                                                {
                                                    if (test.GeneralReason == null)
                                                    {
                                                        MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is too long. Maximum value is 0xFFFFFFFF or 4294967295.");
                                                    }
                                                    else
                                                    {
                                                        MessageBox.Show(test.GeneralReason);
                                                    }
                                                    return false;
                                                }
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                        else
                        {
                            con.Close();
                            SuperCommand sc;
                            success = superCommands.TryGetValue(info[0].ToUpper(), out sc);
                            if (success)
                            {
                                byte[] parameterLengths = sc.ParameterLengths;
                                for (int parameterIndex = 0; parameterIndex < sc.NumberOfParameters; parameterIndex++)
                                {
                                    switch (parameterLengths[parameterIndex])
                                    {
                                        case 2:
                                            {
                                                ReturnWorked test = ParseHalfWordParameter(info[parameterIndex + 1], lineNumber, parameterIndex);
                                                if (!test.Result)
                                                {
                                                    MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is too long. Maximum value is 0xFFFF or 65535.");
                                                    return false;
                                                }
                                                break;
                                            }
                                        case 4:
                                            {
                                                ReturnWorked test = ParseWordParameter(info[parameterIndex + 1], lineNumber, parameterIndex, true);
                                                if (!test.Result && test.PointerName == null)
                                                {
                                                    MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is too long. Maximum value is 0xFFFFFFFF or 4294967295.");
                                                    return false;
                                                }
                                                else if (!test.Result && test.PointerName != null)
                                                {
                                                    MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is a dynamic pointer. Dynamic pointers cannot be used without first defining a dynamic search location.");
                                                    return false;
                                                }
                                                break;
                                            }
                                        default:
                                            {
                                                ReturnWorked test = ParseByteParameter(info[parameterIndex + 1], lineNumber, parameterIndex);
                                                if (!test.Result)
                                                {
                                                    MessageBox.Show("Parameter " + (parameterIndex + 1) + " on line " + lineNumber + " is too long. Maximum value is 0xFF or 255.");
                                                    return false;
                                                }
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (line.StartsWith("#"))
                {
                    string[] theLine = line.Split(' ');
                    switch (theLine[0])
                    {
                        case "#define":
                            {
                                int temp = 0;
                                bool success = Int32.TryParse(theLine[2], out temp);
                                if (success)
                                {
                                    try
                                    {
                                        userDefinitions.Add(theLine[1].ToUpper(), UInt32.Parse(theLine[2]));
                                    }
                                    catch (FormatException)
                                    {
                                        MessageBox.Show("A number was expected for parameter 2 of the definition");
                                    }
                                    catch
                                    {
                                        MessageBox.Show("There was an unknown error with the custom definition");
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        userDefinitions.Add(theLine[1].ToUpper(), UInt32.Parse(ToDecimal(theLine[2])));
                                    }
                                    catch (FormatException)
                                    {
                                        MessageBox.Show("A number was expected for parameter 2 of the definition");
                                    }
                                    catch
                                    {
                                        MessageBox.Show("There was an unknown error with the custom definition");
                                    }
                                }
                                break;
                            }
                        case "#org":
                            string[] splitLine = line.Split(' ');
                            if (splitLine[1].StartsWith("@"))
                            {
                                pointerNames.Add(new DynamicPointer(line.Split(' ')[1].Substring(1), lineNumber, 1), false);
                                insideHashOrgBracket = true;
                                if (!dynamicLocationSet)
                                {
                                    MessageBox.Show("#org on line " + lineNumber + " references a non existant dynamic location.");
                                    return false;
                                }
                            }
                            break;
                        case "#dynamic":
                            {
                                freeSpaceLocation = 0x720000;
                                ReturnWorked result = ParseWordParameter(theLine[1], lineNumber, 1, false);
                                if (!result.Result)
                                {
                                    MessageBox.Show(result.PointerName);
                                    return false;
                                }
                                dynamicLocationSet = true;
                                break;
                            }
                        case "#include":
                            string filePath = System.Windows.Forms.Application.StartupPath + @"\Data\" + theLine[1];
                            if (!filePath.EndsWith(".bsh"))
                            {
                                filePath += ".bsh";
                            }
                            if (!File.Exists(filePath))
                            {
                                MessageBox.Show("The included header file could not be found.");
                                break;
                            }
                            else
                            {
                                if (!theLine[1].Equals("std"))
                                {
                                    string[] rbhFile = File.ReadAllLines(filePath);
                                    ParseHeaderFile(rbhFile);
                                }
                            }
                            break;
                        case "#freespacebyte":
                            {
                                byte temp = 0;
                                bool success = Byte.TryParse(theLine[1], out temp);
                                if (!success)
                                {
                                    success = Byte.TryParse(ToDecimal(theLine[1]), out temp);
                                    if (!success)
                                    {
                                        MessageBox.Show("Error. Free space byte could not be parsed from script.");
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                lineNumber++;
            }
            return true;
        }

        private void ParseHeaderFile(string[] rbhFile)
        {
            foreach (string s in rbhFile)
            {
                if (s.StartsWith("#define"))
                {
                    int temp = 0;
                    string[] rbhLine = s.Split(' ');
                    bool success = Int32.TryParse(rbhLine[2], out temp);
                    if (success)
                    {
                        try
                        {
                            userDefinitions.Add(rbhLine[1].ToUpper(), UInt32.Parse(rbhLine[2]));
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("A number was expected for parameter 2 of the definition");
                        }
                        catch
                        {
                            MessageBox.Show("There was an unknown error with the custom definition");
                        }
                    }
                    else
                    {
                        try
                        {
                            userDefinitions.Add(rbhLine[1].ToUpper(), UInt32.Parse(ToDecimal(rbhLine[2])));
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("A number was expected for parameter 2 of the definition");
                        }
                        catch
                        {
                            MessageBox.Show("There was an unknown error with the custom definition");
                        }
                    }
                }
            }
        }

        private void ParseHeaderFileToDecompileDictionaries(string[] rbhFile, Dictionary<uint, string> tempDict)
        {
            foreach (string s in rbhFile)
            {
                if (s.StartsWith("#define"))
                {
                    int temp = 0;
                    string[] rbhLine = s.Split(' ');
                    bool success = Int32.TryParse(rbhLine[2], out temp);
                    if (success)
                    {
                        try
                        {
                            tempDict.Add(UInt32.Parse(rbhLine[2]), rbhLine[1].ToUpper());
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            tempDict.Add(UInt32.Parse(ToDecimal(rbhLine[2])), rbhLine[1].ToUpper());
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void ParseHeaderFileToDecompileDictionaries(string[] rbhFile, Dictionary<uint, string> tempDict, Dictionary<uint, string> tempDict2, Dictionary<uint, string> tempDict3)
        {
            foreach (string s in rbhFile)
            {
                if (s.StartsWith("#define"))
                {
                    int temp = 0;
                    string[] rbhLine = s.Split(' ');
                    bool success = Int32.TryParse(rbhLine[2], out temp);
                    if (success)
                    {
                        try
                        {
                            if (rbhLine[1].ToUpper().Contains("TYPE"))
                            {
                                tempDict2.Add(UInt32.Parse(rbhLine[2]), rbhLine[1].ToUpper());
                            }
                            else if (rbhLine[1].ToUpper().Contains("BANK"))
                            {
                                tempDict3.Add(UInt32.Parse(rbhLine[2]), rbhLine[1].ToUpper());
                            }
                            else
                            {
                                tempDict.Add(UInt32.Parse(rbhLine[2]), rbhLine[1].ToUpper());
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            if (rbhLine[1].ToUpper().Contains("TYPE"))
                            {
                                tempDict2.Add(UInt32.Parse(ToDecimal(rbhLine[2])), rbhLine[1].ToUpper());
                            }
                            else if (rbhLine[1].ToUpper().Contains("BANK"))
                            {
                                tempDict3.Add(UInt32.Parse(ToDecimal(rbhLine[2])), rbhLine[1].ToUpper());
                            }
                            else
                            {
                                tempDict.Add(UInt32.Parse(ToDecimal(rbhLine[2])), rbhLine[1].ToUpper());
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private string CalculateNumberInNumberSystem(int value)
        {
            switch (numberDecompileMode)
            {
                case 0:
                    if (decompileMode == 0)
                    {
                        return "0b" + Convert.ToString(value, 2);
                    }
                    else
                    {
                        return "0b" + Convert.ToString(value, 2);
                    }
                case 1:
                    if (decompileMode == 0)
                    {
                        return "0o" + Convert.ToString(value, 8);
                    }
                    else
                    {
                        return "0o" + Convert.ToString(value, 8);
                    }
                case 2:
                    if (decompileMode == 0)
                    {
                        return Convert.ToString(value, 10);
                    }
                    else
                    {
                        return Convert.ToString(value, 10);
                    }
                case 4:
                    if (decompileMode == 0)
                    {
                        return "0t" + DecimalToThornal(value).ToUpper();
                    }
                    else
                    {
                        return "0t" + DecimalToThornal(value).ToUpper();
                    }
                default:
                    if (decompileMode == 0)
                    {
                        return "0x" + Convert.ToString(value, 16).ToUpper();
                    }
                    else
                    {
                        return "0x" + Convert.ToString(value, 16).ToUpper();
                    }
            }
        }

        private string DecimalToThornal(int value)
        {
            string s = "";
            if (value != 0)
            {
                for (int i = 6; i >= 0; i--)
                {
                    int numberOfHighestValues = 0;
                    int highestValue = (int)(Math.Pow(32, i));
                    while (value >= highestValue)
                    {
                        value -= highestValue;
                        numberOfHighestValues++;
                    }
                    if (numberOfHighestValues != 0)
                    {
                        if (numberOfHighestValues < 10)
                        {
                            s += numberOfHighestValues.ToString();
                        }
                        else
                        {
                            s += (char)('A' + numberOfHighestValues - 10);
                        }
                    }
                    else if (s.Length != 0)
                    {
                        s += numberOfHighestValues.ToString();
                    }
                }
            }
            else
            {
                s = "0";
            }
            return s;
        }

        private string[] DecompileScript(int offset, byte[] rom)
        {
            if (decompileMode == 2)
            {
                pokemonExport = new Dictionary<uint,string>();
                stdExport = new Dictionary<uint, string>();
                typeExport = new Dictionary<uint, string>();
                movesExport = new Dictionary<uint, string>();
                itemsExport = new Dictionary<uint, string>();
                abilitiesExport = new Dictionary<uint, string>();
                bankExport = new Dictionary<uint, string>();
                ParseHeaderFileToDecompileDictionaries(File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\std.bsh"), stdExport, typeExport, bankExport);
                ParseHeaderFileToDecompileDictionaries(File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\moves.bsh"), movesExport);
                ParseHeaderFileToDecompileDictionaries(File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\abilities.bsh"), abilitiesExport);
                ParseHeaderFileToDecompileDictionaries(File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\items.bsh"), itemsExport);
                ParseHeaderFileToDecompileDictionaries(File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\pokemon.bsh"), pokemonExport);
            }
            List<string> toReturn = new List<string>();
            string theNumber = CalculateNumberInNumberSystem(offset);
            toReturn.Add("#org " + theNumber);
            List<int> alternateDebugLocations = new List<int>();
            bool decompile = true;
            int startingOffset = offset;
            while (decompile)
            {
                if (alternateDebugLocations.Contains(offset) && !decompiledOffsets.Contains(offset))
                {
                    toReturn.Add("");
                    string theNumber2 = CalculateNumberInNumberSystem(offset);
                    toReturn.Add("#org " + theNumber2);
                    for (int i = 0; i < alternateDebugLocations.Count; i++)
                    {
                        if (alternateDebugLocations[i] == offset)
                        {
                            alternateDebugLocations.RemoveAt(i);
                            i--;
                        }
                    }
                    startingOffset = offset;
                }
                byte theValue = rom[offset];
                int commandOffset = offset;
                string superCommand = "";
                bool success = false;
                if (decompileMode != 0)
                {
                    success = TryGetSuperCommand(offset, rom, out superCommand, out offset);
                }
                if (success)
                {
                    if (decompileCommandOffsetsToolStripMenuItem.Checked)
                    {
                        superCommand += "\t\t" + commentString + "  " + CalculateNumberInNumberSystem(commandOffset + 0x08000000);
                    }
                    toReturn.Add(superCommand);
                    continue;
                }
                string line = "";
                Command c = null;
                SQLiteConnection con = new SQLiteConnection("Data Source=" + System.Windows.Forms.Application.StartupPath + @"\Data\Commands.sqlite;Version=3;");
                con.Open();
                SQLiteCommand com = new SQLiteCommand("select * from commands where id = " + theValue, con);
                SQLiteDataReader reader = com.ExecuteReader();
                if (reader.HasRows)
                {
                    line += reader["name"].ToString().ToLower();
                    c = GetCommand(reader, con);
                }
                else if (theValue == 0xFF)
                {
                    int tempOffset = offset + 1;
                    ushort val = (ushort)((0xFF00) + (rom[tempOffset]));
                    foreach (string s in commands.Keys)
                    {
                        if (commands[s].HexID == val)
                        {
                            line += s.ToLower();
                            c = commands[s];
                            offset++;
                        }
                    }
                }
                else
                {
                    foreach (string s in commands.Keys)
                    {
                        if (commands[s].HexID == theValue)
                        {
                            line += s.ToLower();
                            c = commands[s];
                            break;
                        }
                    }
                }
                con.Close();
                if (c == null)
                {
                    MessageBox.Show("Error with command at offset " + CalculateNumberInNumberSystem(offset) + ". Unknown command.");
                    break;
                }
                else
                {
                    for (int i = 0; i < c.NumberOfParameters; i++)
                    {
                        if (c.HexID == 0x89 && i == 0)
                        {
                            if (decompileMode == 2)
                            {
                                line += " " + CalculateNumberInNumberSystem(rom[offset + 1] & 0xBF);
                                if ((rom[offset + 1] & 0x40) != 0)
                                {
                                    line += " true";
                                }
                                else
                                {
                                    line += " false";
                                }
                            }
                            else
                            {
                                line += " " + CalculateNumberInNumberSystem(rom[offset + 1]);
                            }
                            offset++;
                            i++;
                        }
                        else
                        {
                            switch (c.ParameterLengths[i])
                            {
                                case 4:
                                    {
                                        int value = DecompileWordParameter(offset + 1, rom);
                                        if (c.ParameterNames[i].Contains("ROM Address") && c.HexID != 0x7A)
                                        {
                                            if (!alternateDebugLocations.Contains(value - 0x08000000) && !decompiledOffsets.Contains(value - 0x08000000))
                                            {
                                                alternateDebugLocations.Add(value - 0x08000000);
                                            }
                                        }
                                        else if (decompileMode == 2)
                                        {
                                            con.Open();
                                            com = new SQLiteCommand("select type from commanddecompiletypes where commandid = " + c.HexID + " AND parameterid = " + i, con);
                                            reader = com.ExecuteReader();
                                            if (reader.HasRows)
                                            {
                                                string type = reader["type"].ToString();
                                                GetStringFromType(type, Convert.ToUInt32(value));
                                            }
                                            con.Close();
                                        }
                                        line += " " + CalculateNumberInNumberSystem(value);
                                        offset += 4;
                                        if (c.HexID == 0x28)
                                        {
                                            decompile = false;
                                            if (!decompiledOffsets.Contains(value - 0x08000000))
                                            {
                                                offset = value - 0x08000001;
                                            }
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        if (decompileMode == 2)
                                        {
                                            con.Open();
                                            com = new SQLiteCommand("select type from commanddecompiletypes where commandid = " + c.HexID + " AND parameterid = " + i, con);
                                            reader = com.ExecuteReader();
                                            if (reader.HasRows)
                                            {
                                                string type = reader["type"].ToString();
                                                line += " " + GetStringFromType(type, Convert.ToUInt32(DecompileHalfWordParameter(offset + 1, rom)));
                                            }
                                            else
                                            {
                                                line += " " + CalculateNumberInNumberSystem(DecompileHalfWordParameter(offset + 1, rom));
                                            }
                                            con.Close();
                                        }
                                        else
                                        {
                                            line += " " + CalculateNumberInNumberSystem(DecompileHalfWordParameter(offset + 1, rom));
                                        }
                                        offset += 2;
                                        break;
                                    }
                                default:
                                    {
                                        if (decompileMode == 2)
                                        {
                                            con.Open();
                                            com = new SQLiteCommand("select type from commanddecompiletypes where commandid = " + c.HexID + " AND parameterid = " + i, con);
                                            reader = com.ExecuteReader();
                                            if (reader.HasRows)
                                            {
                                                string type = reader["type"].ToString();
                                                line += " " + GetStringFromType(type, Convert.ToUInt32(DecompileByteParameter(offset + 1, rom)));
                                            }
                                            else
                                            {
                                                line += " " + CalculateNumberInNumberSystem(DecompileByteParameter(offset + 1, rom));
                                            }
                                            con.Close();
                                        }
                                        else
                                        {
                                            line += " " + CalculateNumberInNumberSystem(DecompileByteParameter(offset + 1, rom));
                                        }
                                        offset++;
                                        break;
                                    }
                            }
                        }
                    }
                    if (decompileCommandOffsetsToolStripMenuItem.Checked)
                    {
                        line += "\t\t" + commentString + "  " + CalculateNumberInNumberSystem(commandOffset + 0x08000000);
                    }
                    toReturn.Add(line);
                    offset++;
                    con = new SQLiteConnection("Data Source=" + System.Windows.Forms.Application.StartupPath + @"\Data\Commands.sqlite;Version=3;");
                    con.Open();
                    com = new SQLiteCommand("select * from endingcommands where id = " + c.HexID, con);
                    reader = com.ExecuteReader();
                    if (reader.HasRows)
                    {
                        decompile = false;
                    }
                    if (!decompile)
                    {
                        if (c.HexID == 0x28)
                        {
                            if (!decompiledOffsets.Contains(startingOffset))
                            {
                                decompiledOffsets.Add(startingOffset);
                            }
                            else
                            {
                                decompile = true;
                            }
                        }
                    }
                }
            }
            toReturn.Add("");
            foreach (int i in alternateDebugLocations)
            {
                if (!decompiledOffsets.Contains(i))
                {
                    string[] temp = DecompileScript(i, rom);
                    foreach (string s in temp)
                    {
                        toReturn.Add(s);
                    }
                }
            }
            pokemonExport = null;
            stdExport = null;
            movesExport = null;
            itemsExport = null;
            abilitiesExport = null;
            return toReturn.ToArray();
        }

        private string GetStringFromType(string s, uint value)
        {
            if (headersNeeded.Keys.Contains(s))
            {
                headersNeeded[s] = true;
            }
            switch (s)
            {
                case "pokemon":
                    if (pokemonExport.ContainsKey(value))
                    {
                        pokemonExport.TryGetValue(value, out s);
                    }
                    else
                    {
                        s = "0x" + value.ToString("X");
                    }
                    break;
                case "moves":
                    if (movesExport.ContainsKey(value))
                    {
                        movesExport.TryGetValue(value, out s);
                    }
                    else
                    {
                        s = "0x" + value.ToString("X");
                    }
                    break;
                case "abilities":
                    if (abilitiesExport.ContainsKey(value))
                    {
                        abilitiesExport.TryGetValue(value, out s);
                    }
                    else
                    {
                        s = "0x" + value.ToString("X");
                    }
                    break;
                case "item":
                    if (itemsExport.ContainsKey(value))
                    {
                        itemsExport.TryGetValue(value, out s);
                    }
                    else
                    {
                        s = "0x" + value.ToString("X");
                    }
                    break;
                case "type":
                    if (typeExport.ContainsKey(value))
                    {
                        typeExport.TryGetValue(value, out s);
                    }
                    else
                    {
                        s = "0x" + value.ToString("X");
                    }
                    break;
                case "bank":
                    if (bankExport.ContainsKey(value))
                    {
                        bankExport.TryGetValue(value, out s);
                    }
                    else
                    {
                        s = "0x" + value.ToString("X");
                    }
                    break;
                case "std":
                    if (stdExport.ContainsKey(value))
                    {
                        stdExport.TryGetValue(value, out s);
                    }
                    else
                    {
                        s = "0x" + value.ToString("X");
                    }
                    break;
                case "special1":
                    specialOneTriggered = true;
                    s = "0x" + value.ToString("X");
                    ParseINI(File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini"), GetROMCode());
                    if (value == keywords["MOVE"])
                    {
                        specialOneType = "moves";
                    }
                    keywords.Clear();
                    break;
                case "special2":
                    if (specialOneTriggered)
                    {
                        specialOneTriggered = false;
                        if (headersNeeded.Keys.Contains(specialOneType))
                        {
                            headersNeeded[specialOneType] = true;
                        }
                        switch (specialOneType)
                        {
                            case "pokemon":
                                if (pokemonExport.ContainsKey(value))
                                {
                                    pokemonExport.TryGetValue(value, out s);
                                }
                                else
                                {
                                    s = "0x" + value.ToString("X");
                                }
                                break;
                            case "moves":
                                if (movesExport.ContainsKey(value))
                                {
                                    movesExport.TryGetValue(value, out s);
                                }
                                else
                                {
                                    s = "0x" + value.ToString("X");
                                }
                                break;
                            case "abilities":
                                if (abilitiesExport.ContainsKey(value))
                                {
                                    abilitiesExport.TryGetValue(value, out s);
                                }
                                else
                                {
                                    s = "0x" + value.ToString("X");
                                }
                                break;
                            case "item":
                                if (itemsExport.ContainsKey(value))
                                {
                                    itemsExport.TryGetValue(value, out s);
                                }
                                else
                                {
                                    s = "0x" + value.ToString("X");
                                }
                                break;
                            case "type":
                                if (typeExport.ContainsKey(value))
                                {
                                    typeExport.TryGetValue(value, out s);
                                }
                                else
                                {
                                    s = "0x" + value.ToString("X");
                                }
                                break;
                            case "bank":
                                if (bankExport.ContainsKey(value))
                                {
                                    bankExport.TryGetValue(value, out s);
                                }
                                else
                                {
                                    s = "0x" + value.ToString("X");
                                }
                                break;
                            case "std":
                                if (stdExport.ContainsKey(value))
                                {
                                    stdExport.TryGetValue(value, out s);
                                }
                                else
                                {
                                    s = "0x" + value.ToString("X");
                                }
                                break;
                            default:
                                s = "0x" + value.ToString("X");
                                break;
                        }
                        specialOneType = "";
                    }
                    else
                    {
                        s = "0x" + value.ToString("X");
                    }
                    break;
                default:
                    s = "0x" + value.ToString("X");
                    break;
            }
            return s;
        }

        private bool TryGetSuperCommand(int offset, byte[] rom, out string name, out int newOffset)
        {
            byte theValue = rom[offset];
            if (theValue == 4 && rom[offset + 1] == 5 && rom[offset + 2] == 6 && rom[offset + 3] == 7)
            {
                name = "calculatedamage";
                newOffset = offset + 4;
                return true;
            }
            else if (theValue == 0x2E && rom[offset + 6] == 0x13 && rom[offset + 11] == 0x12 && rom[offset + 1] == 0x87 && rom[offset + 2] == 0x3E && rom[offset + 3] == 2 && rom[offset + 4] == 2)
            {
                name = "printmessage 0x" + DecompileWordParameter((DecompileWordParameter(((offset + 7) + (rom[offset + 5] * 4)), rom) - 0x08000000), rom).ToString("X") + " 0x" + DecompileHalfWordParameter(offset + 12, rom).ToString("X");
                newOffset = offset + 14;
                return true;
            }
            else if (theValue == 0x2E && rom[offset + 1] == 0xD2 && rom[offset + 2] == 0x3F && rom[offset + 3] == 0x2 && rom[offset + 4] == 0x2 && rom[offset + 5] == 0x2 && rom[offset + 6] == 0x2E && rom[offset + 7] == 0xFB && rom[offset + 8] == 0x3F && rom[offset + 9] == 0x2 && rom[offset + 10] == 0x2)
            {
                name = "forcetype 0x" + (DecompileByteParameter(offset + 11, rom) - 0x80);
                newOffset = offset + 12;
                return true;
            }
            else
            {
                name = null;
                newOffset = offset;
                return false;
            }
        }

        private int DecompileWordParameter(int offset, byte[] rom)
        {
            string value = "";
            for (int i = 0; i < 4; i++)
            {
                value += rom[offset + 3 - i].ToString("X2");
            }
            return Int32.Parse(ToDecimal("0x" + value));
        }

        private ushort DecompileHalfWordParameter(int offset, byte[] rom)
        {
            string value = "";
            for (int i = 0; i < 2; i++)
            {
                value += rom[offset + 1 - i].ToString("X2");
            }
            return UInt16.Parse(ToDecimal("0x" + value));
        }

        private byte DecompileByteParameter(int offset, byte[] rom)
        {
            return rom[offset];
        }

        private void btnCompile_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            freeSpaceByte = 0xFF;
            decompiledOffsets.Clear();
            Dictionary<string, int> compiledHashOrgLocations = new Dictionary<string, int>();
            firstTime = true;
            DialogResult dialogResult = DialogResult.OK;
            if (selectedROMPath == null || selectedROMPath.Equals(""))
            {
                dialogResult = openFileDialog1.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    selectedROMPath = openFileDialog1.FileName;
                    numericUpDown1_ValueChanged(sender, e);
                    btnDecompile.Enabled = true;
                    txtDecompileOffset.Enabled = true;
                }
            }
            if (File.Exists(selectedROMPath))
            {
                string romCode = GetROMCode();
                string iniPath = System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini";
                if (File.Exists(iniPath))
                {
                    string[] iniFile = File.ReadAllLines(iniPath);
                    ParseINI(iniFile, romCode);
                }
                iniPath = System.Windows.Forms.Application.StartupPath + @"\Data\std.bsh";
                if (File.Exists(iniPath))
                {
                    string[] iniFile = File.ReadAllLines(iniPath);
                    ParseHeaderFile(iniFile);
                }
                else
                {
                    MessageBox.Show("The Ini file could not be parsed. The script can still be compiled, but keywords may not be used.");
                }
                bool result = DebugWrittenScript();
                iniPath = System.Windows.Forms.Application.StartupPath + @"\Data\std.bsh";
                if (File.Exists(iniPath))
                {
                    string[] iniFile = File.ReadAllLines(iniPath);
                    ParseHeaderFile(iniFile);
                }
                if (result)
                {
                    int lineNumber = 1;
                    FindStrings();
                    string[] hashOrgs = FindHashOrgs();
                    CompiledStrings x = CompileStrings(selectedROMPath);
                    List<byte> compiledScript = new List<byte>();
                    List<byte> compiledHashOrg;
                    Dictionary<string, int> dynamicGotos = new Dictionary<string, int>();
                    Dictionary<string, int> lengths = new Dictionary<string, int>();
                    foreach (string hashOrgName in hashOrgs)
                    {
                        lengths.Add(hashOrgName, DryRunForLength(hashOrgName));
                    }
                    foreach (string hashOrgName in hashOrgs)
                    {
                        compiledHashOrg = new List<byte>();
                        bool compile = false;
                        int scriptLength = lengths[hashOrgName];
                        int location2 = 0;
                        foreach (string line in scripts[tabControl1.SelectedIndex].Lines)
                        {
                            if (compile)
                            {
                                if (!line.Equals("") && !line.StartsWith("#"))
                                {
                                    string newLine = line;
                                    if (line.Contains(commentString))
                                    {
                                        newLine = Regex.Split(newLine, commentString)[0].TrimEnd();
                                    }
                                    string[] info = newLine.Split(' ');
                                    Command c;
                                    bool success = commands.TryGetValue(info[0].ToUpper(), out c);
                                    if (success)
                                    {
                                        HashOrgAddCommand(compiledHashOrg, compiledHashOrgLocations, c, compile, info, lengths, lineNumber, out compile);
                                    }
                                    else
                                    {
                                        SuperCommand sc;
                                        success = superCommands.TryGetValue(info[0].ToUpper(), out sc);
                                        if (success)
                                        {
                                            switch (info[0].ToUpper())
                                            {
                                                case "FORCETYPE":
                                                    {
                                                        byte parameter = ParseByteParameter(info[1], 0, 1).Byte;
                                                        byte[] stuff = ForceTypeSuperCommand(parameter);
                                                        foreach (byte b in stuff)
                                                        {
                                                            compiledHashOrg.Add(b);
                                                        }
                                                        break;
                                                    }
                                                case "PRINTMESSAGE":
                                                    {
                                                        byte position = 0;
                                                        uint locationNew = 0;
                                                        foreach (string s in dynamicStrings.Keys)
                                                        {
                                                            if (s.Equals(info[1]))
                                                            {
                                                                if (s.StartsWith("@"))
                                                                {
                                                                    locationNew = 0x7FFEFEFE;
                                                                }
                                                                else
                                                                {
                                                                    bool succeeded = UInt32.TryParse(s, out locationNew);
                                                                    if (!succeeded)
                                                                    {
                                                                        UInt32.TryParse(ToDecimal(s), out locationNew);
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                            position++;
                                                        }
                                                        ushort parameter2 = ParseHalfWordParameter(info[2], 0, 2).HalfWord;
                                                        byte[] stuff = PrintMessageSuperCommand(position, locationNew, parameter2);
                                                        foreach (byte b in stuff)
                                                        {
                                                            compiledHashOrg.Add(b);
                                                        }
                                                        break;
                                                    }
                                                case "SETHALFWORD":
                                                    {
                                                        uint ramLocation = ParseWordParameter(info[1], 0, 1, false).Word;
                                                        ushort value = ParseHalfWordParameter(info[2], 0, 2).HalfWord;
                                                        byte[] stuff = SetHalfWordSuperCommand(value, ramLocation);
                                                        foreach (byte b in stuff)
                                                        {
                                                            compiledHashOrg.Add(b);
                                                        }
                                                    }
                                                    break;
                                                case "SETWORD":
                                                    {
                                                        uint ramLocation = ParseWordParameter(info[1], 0, 1, false).Word;
                                                        uint value = ParseWordParameter(info[2], 0, 2, false).Word;
                                                        byte[] stuff = SetWordSuperCommand(value, ramLocation);
                                                        foreach (byte b in stuff)
                                                        {
                                                            compiledHashOrg.Add(b);
                                                        }
                                                    }
                                                    break;
                                                case "CALCULATEDAMAGE":
                                                    {
                                                        byte[] stuff = CalculateDamageSuperCommand();
                                                        foreach (byte b in stuff)
                                                        {
                                                            compiledHashOrg.Add(b);
                                                        }
                                                        break;
                                                    }
                                                default:
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            SQLiteConnection con = new SQLiteConnection("Data Source=" + System.Windows.Forms.Application.StartupPath + @"\Data\Commands.sqlite;Version=3;");
                                            con.Open();
                                            SQLiteCommand com = new SQLiteCommand("select * from commands where name = '" + info[0].ToUpper() + "'", con);
                                            SQLiteDataReader reader = com.ExecuteReader();
                                            if (reader != null)
                                            {
                                                c = GetCommand(reader, con);
                                                HashOrgAddCommand(compiledHashOrg, compiledHashOrgLocations, c, compile, info, lengths, lineNumber, out compile);
                                            }
                                        }
                                    }
                                }
                            }
                            if (line.StartsWith("#"))
                            {
                                string[] theLine = line.Split(' ');
                                switch (theLine[0])
                                {
                                    case "#define":
                                        {
                                            int temp = 0;
                                            bool success = Int32.TryParse(theLine[2], out temp);
                                            if (success)
                                            {
                                                try
                                                {
                                                    userDefinitions.Add(theLine[1].ToUpper(), UInt32.Parse(theLine[2]));
                                                }
                                                catch (FormatException)
                                                {
                                                    MessageBox.Show("A number was expected for parameter 2 of the definition");
                                                }
                                                catch
                                                {
                                                    MessageBox.Show("There was an unknown error with the custom definition");
                                                }
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    userDefinitions.Add(theLine[1].ToUpper(), UInt32.Parse(ToDecimal(theLine[2])));
                                                }
                                                catch (FormatException)
                                                {
                                                    MessageBox.Show("A number was expected for parameter 2 of the definition");
                                                }
                                                catch
                                                {
                                                    MessageBox.Show("There was an unknown error with the custom definition");
                                                }
                                            }
                                            break;
                                        }
                                    case "#dynamic":
                                        {
                                            freeSpaceLocation = 0;
                                            bool success = Int32.TryParse(theLine[1], out freeSpaceLocation);
                                            if (!success)
                                            {
                                                success = Int32.TryParse(ToDecimal(theLine[1]), out freeSpaceLocation);
                                                if (!success)
                                                {
                                                    MessageBox.Show("Error. Dynamic offset could not be parsed from script. Dynamic offset set to 0. WARNING: SLOW!");
                                                }
                                            }
                                            if (success)
                                            {
                                                dynamicLocationSet = true;
                                            }
                                            break;
                                        }
                                    case "#include":
                                        string filePath = System.Windows.Forms.Application.StartupPath + @"\Data\" + theLine[1];
                                        if (!filePath.EndsWith(".bsh"))
                                        {
                                            filePath += ".bsh";
                                        }
                                        if (!File.Exists(filePath))
                                        {
                                            MessageBox.Show("The included header file could not be found.");
                                            break;
                                        }
                                        else
                                        {
                                            if (!theLine[1].Equals("std"))
                                            {
                                                string[] rbhFile = File.ReadAllLines(filePath);
                                                ParseHeaderFile(rbhFile);
                                            }
                                        }
                                        break;
                                    case "#clean":
                                        {
                                            if (scriptInserted)
                                            {
                                                byte[] romFile = File.ReadAllBytes(selectedROMPath);
                                                for (int i = 0; i < lastInsertedScriptLength; i++)
                                                {
                                                    romFile[lastInsertedScriptLocation + i] = 0xFF;
                                                }
                                                try
                                                {
                                                    File.WriteAllBytes(selectedROMPath, romFile);
                                                    scriptInserted = false;
                                                }
                                                catch
                                                {
                                                    MessageBox.Show("There was an error writing the compiled script to the ROM. Try closing any tools that the ROM is already open in. If the ROM is not open in any tools, contact Jambo51 on the tool's page.");
                                                }
                                            }
                                        }
                                        break;
                                    case "#org":
                                        if (hashOrgs.Contains(theLine[1]))
                                        {
                                            if (theLine[1].StartsWith("@"))
                                            {
                                                if (theLine[1].Equals(hashOrgName))
                                                {
                                                    location2 = FindFreeSpace(scriptLength, freeSpaceLocation, File.ReadAllBytes(selectedROMPath));
                                                    compile = true;
                                                }
                                                else
                                                {
                                                    if (compile)
                                                    {
                                                        compile = false;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (theLine[1].Equals(hashOrgName))
                                                {
                                                    compile = true;
                                                }
                                                else
                                                {
                                                    if (compile)
                                                    {
                                                        compile = false;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (theLine[1].Equals(hashOrgName))
                                            {
                                                compile = true;
                                            }
                                            else
                                            {
                                                if (compile)
                                                {
                                                    compile = false;
                                                }
                                            }
                                        }
                                        break;
                                    case "#freespacebyte":
                                        {
                                            bool success = Byte.TryParse(theLine[1], out freeSpaceByte);
                                            if (!success)
                                            {
                                                success = Byte.TryParse(ToDecimal(theLine[1]), out freeSpaceByte);
                                                if (!success)
                                                {
                                                    MessageBox.Show("Error. Free space byte could not be parsed from script. Free Space Byte set to 0xFF.");
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            lineNumber++;
                        }
                        byte[] rom = File.ReadAllBytes(selectedROMPath);
                        if (firstTime)
                        {
                            firstTime = false;
                            location = FindFreeSpace(lengths[hashOrgName], freeSpaceLocation, rom);
                        }
                        if (hashOrgName.StartsWith("@"))
                        {
                            location2 = FindFreeSpace(lengths[hashOrgName], freeSpaceLocation, rom);
                        }
                        else
                        {
                            bool succeeded = Int32.TryParse(hashOrgName, out location2);
                            if (!succeeded)
                            {
                                succeeded = Int32.TryParse(ToDecimal(hashOrgName), out location2);
                            }
                        }
                        compiledHashOrgLocations.Add(hashOrgName, location2);
                        int index = 0;
                        foreach (byte b in compiledHashOrg)
                        {
                            compiledScript.Add(b);
                            rom[location2 + index] = b;
                            index++;
                        }
                        File.WriteAllBytes(selectedROMPath, rom);
                        userDefinitions.Clear();
                    }
                    byte[] romNew = File.ReadAllBytes(selectedROMPath);
                    int searchLocation = freeSpaceLocation;
                    int tableLocation = 1;
                    while (tableLocation % 4 != 0)
                    {
                        tableLocation = FindFreeSpace(x.Table.Length * 4, searchLocation, romNew);
                        searchLocation++;
                    }
                    lastInsertedScriptLength = tableLocation - (location + compiledScript.Count);
                    int searchLocation2 = tableLocation + (x.Table.Length * 4);
                    searchLocation2 = FindFreeSpace(x.CompiledStringsArray.Length, searchLocation2, romNew);
                    int indexNew = 0;
                    foreach (byte b in x.CompiledStringsArray)
                    {
                        romNew[searchLocation2 + indexNew] = b;
                        indexNew++;
                    }
                    indexNew = 0;
                    foreach (int k in x.Table)
                    {
                        if (k < 0x8000000)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                romNew[tableLocation + j + (indexNew * 4)] = (Byte.Parse(ToDecimal("0x" + (k + 0x08000000 + searchLocation2).ToString("X8").Substring((k + 0x08000000 + searchLocation2).ToString("X8").Length - ((j * 2) + 2), 2))));
                            }
                        }
                        else
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                romNew[tableLocation + j + (indexNew * 4)] = (Byte.Parse(ToDecimal("0x" + k.ToString("X8").Substring(k.ToString("X8").Length - ((j * 2) + 2), 2))));
                            }
                        }
                        indexNew++;
                    }
                    indexNew = 0;
                    int ignoreCounter = 0;
                    for (int i = 0; i < compiledScript.Count; i++)
                    {
                        byte b = romNew[location + i];
                        if (ignoreCounter == 0)
                        {
                            if (b == 0x13)
                            {
                                if (romNew[location + i + 1] == 0xFE && romNew[location + i + 2] == 0xFE && romNew[location + i + 3] == 0xFE && romNew[location + i + 4] == 0x7F)
                                {
                                    romNew[location + i] = b;
                                    for (int j = 0; j < 4; j++)
                                    {
                                        romNew[location + j + 1 + i] = (Byte.Parse(ToDecimal("0x" + (tableLocation + 0x08000000).ToString("X8").Substring((tableLocation + 0x08000000).ToString("X8").Length - ((j * 2) + 2), 2))));
                                    }
                                    indexNew += 4;
                                    ignoreCounter = 4;
                                }
                            }
                            else
                            {
                                romNew[location + i] = b;
                            }
                        }
                        else
                        {
                            ignoreCounter--;
                        }
                    }
                    File.WriteAllBytes(selectedROMPath, romNew);
                    foreach (string s in hashOrgs)
                    {
                        lastInsertedScriptLength += lengths[s];
                    }
                    lastInsertedScriptLength += x.CompiledStringsArray.Length + 4 * x.Table.Length;
                    lastInsertedScriptLocation = location;
                    scriptInserted = true;
                    keywords.Clear();
                    dynamicStrings.Clear();
                    List<string> left = new List<string>();
                    List<string> right = new List<string>();
                    for (int i = 0; i < compiledHashOrgLocations.Count; i++)
                    {
                        left.Add(hashOrgs[i]);
                        right.Add("0x" + compiledHashOrgLocations[hashOrgs[i]].ToString("X6"));
                    }
                    toolStripStatusLabel1.Text = "Compilation Complete!";
                    Form8 fr = new Form8(left, right);
                    fr.Show();
                }
                else
                {
                    MessageBox.Show("Compilation cancelled.");
                }
            }
            this.Cursor = Cursors.Default;
            keywords.Clear();
            userDefinitions.Clear();
        }

        private void HashOrgAddCommand(List<byte> compiledHashOrg, Dictionary<string, int> compiledHashOrgLocations, Command c, bool compile, string[] info, Dictionary<string, int> lengths, int lineNumber, out bool newCompile)
        {
            newCompile = compile;
            if (c.HexID < 0x100)
            {
                compiledHashOrg.Add(Convert.ToByte(c.HexID));
            }
            else
            {
                compiledHashOrg.Add(Byte.Parse(ToDecimal(c.HexID.ToString("X4").Substring(0, 2))));
                compiledHashOrg.Add(Byte.Parse(ToDecimal(c.HexID.ToString("X4").Substring(2, 2))));
            }
            SQLiteConnection con = new SQLiteConnection("Data Source=" + System.Windows.Forms.Application.StartupPath + @"\Data\Commands.sqlite;Version=3;");
            con.Open();
            SQLiteCommand com = new SQLiteCommand("select * from endingcommands where id = " + c.HexID, con);
            SQLiteDataReader reader = com.ExecuteReader();
            if (reader.HasRows)
            {
                newCompile = false;
            }
            for (int i = 0; i < c.NumberOfParameters; i++)
            {
                if (c.HexID == 0x89 && i == 0)
                {
                    byte paramTwo;
                    if (info[2].ToUpper().Equals("TRUE"))
                    {
                        paramTwo = 0x40;
                    }
                    else if (info[2].ToUpper().Equals("FALSE"))
                    {
                        paramTwo = 0;
                    }
                    else
                    {
                        paramTwo = ParseByteParameter(info[2], lineNumber, 2).Byte;
                        if (paramTwo != 0x40)
                        {
                            paramTwo = 0;
                        }
                    }
                    byte byteOne = (byte)((int)ParseByteParameter(info[1], lineNumber, 1).Byte | (int)paramTwo);
                    compiledHashOrg.Add(byteOne);
                    i++;
                }
                else
                {
                    switch (c.ParameterLengths[i])
                    {
                        case 2:
                            {
                                ushort value = ParseHalfWordParameter(info[i + 1], 0, i).HalfWord;
                                for (int j = 0; j < 2; j++)
                                {
                                    compiledHashOrg.Add(Byte.Parse(ToDecimal("0x" + value.ToString("X4").Substring(value.ToString("X4").Length - ((j * 2) + 2), 2))));
                                }
                                break;
                            }
                        case 4:
                            {
                                if (info[i + 1].StartsWith("@"))
                                {
                                    int location3 = freeSpaceLocation;
                                    if (compiledHashOrgLocations.ContainsKey(info[i + 1]))
                                    {
                                        location3 = compiledHashOrgLocations[info[i + 1]];
                                    }
                                    else
                                    {
                                        foreach (string s in lengths.Keys)
                                        {
                                            if (!compiledHashOrgLocations.ContainsKey(s))
                                            {
                                                location3 = FindFreeSpace(lengths[s], location3, File.ReadAllBytes(selectedROMPath));
                                            }
                                            if (s.Equals(info[i + 1]))
                                            {
                                                break;
                                            }
                                            if (s.StartsWith("@"))
                                            {
                                                location3 += lengths[s];
                                            }
                                        }
                                    }
                                    location3 += 0x08000000;
                                    for (int j = 0; j < 4; j++)
                                    {
                                        compiledHashOrg.Add(Byte.Parse(ToDecimal("0x" + location3.ToString("X8").Substring(8 - ((j * 2) + 2), 2))));
                                    }
                                }
                                else
                                {
                                    uint value = ParseWordParameter(info[i + 1], 0, i, true).Word;
                                    for (int j = 0; j < 4; j++)
                                    {
                                        if (j == 3 && c.ParameterNames[i].ToUpper().Contains("ADDRESS") && (value.ToString("X8").Substring(8 - ((j * 2) + 2), 2).Equals("00") || value.ToString("X8").Substring(8 - ((j * 2) + 2), 2).Equals("01")))
                                        {
                                            compiledHashOrg.Add(Convert.ToByte(8 + Int32.Parse(value.ToString("X8").Substring(8 - ((j * 2) + 2), 2))));
                                        }
                                        else
                                        {
                                            compiledHashOrg.Add(Byte.Parse(ToDecimal("0x" + value.ToString("X8").Substring(8 - ((j * 2) + 2), 2))));
                                        }
                                    }
                                }
                                if (c.ParameterNames[i].Contains("Address"))
                                {
                                    if (c.HexID == 0x28)
                                    {
                                        newCompile = false;
                                    }
                                }
                                break;
                            }
                        default:
                            {
                                compiledHashOrg.Add(ParseByteParameter(info[i + 1], 0, i).Byte);
                                break;
                            }
                    }
                }
            }
        }

        private void WriteToClipboard(string stringToWrite)
        {
            bool written = false;
            while (!written)
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(stringToWrite);
                    written = true;
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    written = false;
                }
                catch
                {
                    written = true;
                }
            }
        }

        private string GetFromClipboard()
        {
            return System.Windows.Forms.Clipboard.GetText();
        }

        private string[] FindHashOrgs()
        {
            bool checkNextLine = false;
            string name = "";
            List<string> hashOrgs = new List<string>();
            foreach (string s in scripts[tabControl1.SelectedIndex].Lines)
            {
                if (s.StartsWith("#org "))
                {
                    checkNextLine = true;
                    name = s.Split(' ')[1];
                    continue;
                }
                if (checkNextLine)
                {
                    if (!s.StartsWith("="))
                    {
                        hashOrgs.Add(name);
                    }
                    checkNextLine = false;
                }
            }
            return hashOrgs.ToArray();
        }

        private int DryRunForLength(string name)
        {
            if (name.StartsWith("@"))
            {
                List<byte> theScript = new List<byte>();
                bool compile = false;
                foreach (string line in scripts[tabControl1.SelectedIndex].Lines)
                {
                    if (line.Equals("#org " + name))
                    {
                        compile = true;
                        continue;
                    }
                    if (compile)
                    {
                        if (!line.Equals("") && !line.StartsWith("#"))
                        {
                            string newLine = line;
                            if (line.Contains(commentString))
                            {
                                newLine = Regex.Split(newLine, commentString)[0].TrimEnd();
                            }
                            string[] info = newLine.Split(' ');
                            Command c;
                            bool success = commands.TryGetValue(info[0].ToUpper(), out c);
                            if (success)
                            {
                                DryRunCompileCommand(c, theScript, compile, out compile);
                            }
                            else
                            {
                                SQLiteConnection con = new SQLiteConnection("Data Source=" + System.Windows.Forms.Application.StartupPath + @"\Data\Commands.sqlite;Version=3;");
                                con.Open();
                                SQLiteCommand com = new SQLiteCommand("select * from commands where name = '" + info[0].ToUpper() + "'", con);
                                SQLiteDataReader reader = com.ExecuteReader();
                                if (reader != null)
                                {
                                    c = GetCommand(reader, con);
                                    DryRunCompileCommand(c, theScript, compile, out compile);
                                }
                                else
                                {
                                    con.Close();
                                    SuperCommand sc;
                                    success = superCommands.TryGetValue(info[0].ToUpper(), out sc);
                                    if (success)
                                    {
                                        switch (info[0].ToUpper())
                                        {
                                            case "FORCETYPE":
                                                {
                                                    byte[] stuff = ForceTypeSuperCommand(0);
                                                    foreach (byte b in stuff)
                                                    {
                                                        theScript.Add(b);
                                                    }
                                                    break;
                                                }
                                            case "PRINTMESSAGE":
                                                {
                                                    byte position = 0;
                                                    uint locationNew = 0;
                                                    foreach (string s in dynamicStrings.Keys)
                                                    {
                                                        if (s.Equals(info[1]))
                                                        {
                                                            if (s.StartsWith("@"))
                                                            {
                                                                locationNew = 0x7FFEFEFE;
                                                            }
                                                            else
                                                            {
                                                                bool succeeded = UInt32.TryParse(s, out locationNew);
                                                                if (!succeeded)
                                                                {
                                                                    UInt32.TryParse(ToDecimal(s), out locationNew);
                                                                }
                                                            }
                                                            break;
                                                        }
                                                        position++;
                                                    }
                                                    byte[] stuff = PrintMessageSuperCommand(position, locationNew, 0);
                                                    foreach (byte b in stuff)
                                                    {
                                                        theScript.Add(b);
                                                    }
                                                    break;
                                                }
                                            case "SETHALFWORD":
                                                {
                                                    byte[] stuff = SetHalfWordSuperCommand(0, 0);
                                                    foreach (byte b in stuff)
                                                    {
                                                        theScript.Add(b);
                                                    }
                                                    break;
                                                }
                                            case "SETWORD":
                                                {
                                                    byte[] stuff = SetWordSuperCommand(0, 0);
                                                    foreach (byte b in stuff)
                                                    {
                                                        theScript.Add(b);
                                                    }
                                                    break;
                                                }
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (line.StartsWith("#"))
                        {
                            string[] theLine = line.Split(' ');
                            switch (theLine[0])
                            {
                                case "#org":
                                    compile = false;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (line.Equals(""))
                        {
                            compile = false;
                            break;
                        }
                    }
                }
                return theScript.Count;
            }
            else
            {
                return 0;
            }
        }

        private void DryRunCompileCommand(Command c, List<byte> theScript, bool compile, out bool newCompile)
        {
            newCompile = compile;
            if (c.HexID < 0x100)
            {
                theScript.Add(Convert.ToByte(c.HexID));
            }
            else
            {
                theScript.Add(Byte.Parse(c.HexID.ToString().Substring(0, 2)));
                theScript.Add(Byte.Parse(c.HexID.ToString().Substring(2, 2)));
            }
            for (int i = 0; i < c.NumberOfParameters; i++)
            {
                if (c.HexID == 0x89 && i == 0)
                {
                    theScript.Add(0);
                    i++;
                }
                else if (c.HexID == 0x28)
                {
                    newCompile = false;
                    for (int j = 0; j < 4; j++)
                    {
                        theScript.Add(0);
                    }
                }
                else if (c.HexID == 0x3C || c.HexID == 0x3D || c.HexID == 0x3E || c.HexID == 0x3F)
                {
                    newCompile = false;
                }
                else
                {
                    switch (c.ParameterLengths[i])
                    {
                        case 2:
                            {
                                for (int j = 0; j < 2; j++)
                                {
                                    theScript.Add(0);
                                }
                                break;
                            }
                        case 4:
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    theScript.Add(0);
                                }
                                break;
                            }
                        default:
                            {
                                theScript.Add(0);
                                break;
                            }
                    }
                }
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            unsaved[tabControl1.SelectedIndex] = true;
        }

        private void ParseINI(string[] iniFile, string romCode)
        {
            bool getValues = false;
            foreach (string s in iniFile)
            {
                if (s.Equals("[" + romCode + "]"))
                {
                    getValues = true;
                    continue;
                }
                if (getValues)
                {
                    if (s.Equals(@"[/" + romCode + "]"))
                    {
                        break;
                    }
                    else
                    {
                        uint value = 0;
                        bool success = UInt32.TryParse(s.Split('=')[1], out value);
                        if (!success)
                        {
                            success = UInt32.TryParse(ToDecimal(s.Split('=')[1]), out value);
                            if (!success)
                            {
                                MessageBox.Show("Error parsing the move keyword value from the main ini. Are you sure it is formatted correctly as a number?");
                            }
                        }
                        keywords.Add(s.Split('=')[0].ToUpper(), value);
                    }
                }
            }
        }

        private CompiledStrings CompileStrings(string filePath)
        {
            List<int> pointers = new List<int>();
            List<byte> compiledStrings = new List<byte>();
            int index = 0;
            Dictionary<byte, char> characterValues = ReadTableFile(System.Windows.Forms.Application.StartupPath + @"\Data\table.ini");
            foreach (string s in dynamicStrings.Keys)
            {
                if (s.StartsWith("@"))
                {
                    pointers.Add(compiledStrings.Count);
                    index = 0;
                    bool ignoreNextIndex = false;
                    foreach (char c in dynamicStrings[s])
                    {
                        if (ignoreNextIndex)
                        {
                            ignoreNextIndex = false;
                            index += 2;
                            continue;
                        }
                        if (c == '\\')
                        {
                            char c2 = (dynamicStrings[s][index + 1]);
                            switch (c2)
                            {
                                case 'n':
                                    compiledStrings.Add(0xFE);
                                    break;
                                case 'p':
                                    compiledStrings.Add(0xFA);
                                    break;
                                case 'l':
                                    compiledStrings.Add(0xFB);
                                    break;
                                default:
                                    break;
                            }
                            ignoreNextIndex = true;
                            continue;
                        }
                        else
                        {
                            byte toStore = 0;
                            foreach (byte b in characterValues.Keys)
                            {
                                if (characterValues[b] == c)
                                {
                                    toStore = b;
                                }
                            }
                            compiledStrings.Add(toStore);
                        }
                        index++;
                    }
                    compiledStrings.Add(0xFF);
                }
                else
                {
                    int location2 = 0;
                    index = 0;
                    int index2 = 0;
                    try
                    {
                        location2 = Int32.Parse(s);
                    }
                    catch (FormatException)
                    {
                        location2 = Int32.Parse(ToDecimal(s));
                    }
                    catch
                    {
                        MessageBox.Show("There was an unknown error.");
                    }
                    pointers.Add(location2 + 0x08000000);
                    byte[] rom = File.ReadAllBytes(filePath);
                    bool ignoreNextIndex = false;
                    foreach (char c in dynamicStrings[s])
                    {
                        if (ignoreNextIndex)
                        {
                            ignoreNextIndex = false;
                            index += 2;
                            index2++;
                            continue;
                        }
                        if (c == '\\')
                        {
                            char c2 = (dynamicStrings[s][index + 1]);
                            switch (c2)
                            {
                                case 'n':
                                    rom[location2 + index2] = 0xFE;
                                    break;
                                case 'p':
                                    rom[location2 + index2] = 0xFA;
                                    break;
                                case 'l':
                                    rom[location2 + index2] = 0xFB;
                                    break;
                                default:
                                    break;
                            }
                            ignoreNextIndex = true;
                            continue;
                        }
                        else
                        {
                            byte toStore = 0;
                            foreach (byte b in characterValues.Keys)
                            {
                                if (characterValues[b] == c)
                                {
                                    toStore = b;
                                }
                            }
                            rom[location2 + index2] = toStore;
                        }
                        index++;
                        index2++;
                    }
                    rom[location2 + index2] = 0xFF;
                    rom[location2 + index2 + 1] = 0x0;
                    File.WriteAllBytes(filePath, rom);
                }
            }
            if (compiledStrings.Count > 0)
            {
                compiledStrings.Add(0);
            }
            return new CompiledStrings(pointers.ToArray(), compiledStrings.ToArray());
        }

        private void QuickSave(string filePath)
        {
            if (filePath != null)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.WriteAllLines(filePath, scripts[tabControl1.SelectedIndex].Lines.ToArray());
                        tabControl1.TabPages[tabControl1.SelectedIndex].Text = Path.GetFileNameWithoutExtension(filePath);
                    }
                    catch
                    {
                        MessageBox.Show("File could not be written. Saving cancelled.");
                    }
                }
                else
                {
                    DialogResult result = saveFileDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        File.WriteAllLines(saveFileDialog1.FileName, scripts[tabControl1.SelectedIndex].Lines.ToArray());
                        newScriptNames.Remove(Int32.Parse(tabControl1.TabPages[tabControl1.SelectedIndex].Text.Split(' ')[1]));
                        tabControl1.TabPages[tabControl1.SelectedIndex].Text = Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                        currentlyOpenBS[tabControl1.SelectedIndex] = saveFileDialog1.FileName;
                    }
                }
            }
            else
            {
                DialogResult result = saveFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    File.WriteAllLines(saveFileDialog1.FileName, scripts[tabControl1.SelectedIndex].Lines.ToArray());
                    newScriptNames.Remove(Int32.Parse(tabControl1.TabPages[tabControl1.SelectedIndex].Text.Split(' ')[1]));
                    tabControl1.TabPages[tabControl1.SelectedIndex].Text = Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                    currentlyOpenBS[tabControl1.SelectedIndex] = saveFileDialog1.FileName;
                }
            }
        }

        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            foreach (TabPage t in tabControl1.TabPages)
            {
                int i = tabControl1.TabPages.IndexOf(t);
                if (unsaved[i])
                {
                    DialogResult result = MessageBox.Show("Your script \"" + t.Text + "\" is unsaved. Would you like to save it before closing?",
            "Save?",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (result)
                    {
                        case DialogResult.Yes:
                            result = saveFileDialog1.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                try
                                {
                                    File.WriteAllLines(saveFileDialog1.FileName, scripts[tabControl1.SelectedIndex].Lines.ToArray());
                                }
                                catch
                                {
                                    MessageBox.Show("The file could not be saved. This programme will be kept open.");
                                    e.Cancel = true;
                                }
                            }
                            else
                            {
                                MessageBox.Show("The file could not be saved. This programme will be kept open.");
                                e.Cancel = true;
                            }
                            break;
                        case DialogResult.No:
                            break;
                        default:
                            e.Cancel = true;
                            break;
                    }
                }
            }
        }

        private void Form4_FormClosing(Object sender, FormClosingEventArgs e)
        {
            openFindForm = null;
        }

        private void Form5_FormClosing(Object sender, FormClosingEventArgs e)
        {
            openFARForm = null;
        }

        private int FindFreeSpace(int dataSize, int startLocation, byte[] rom)
        {
            bool spaceFound = false;
            while (!spaceFound)
            {
                for (int i = 0; i <= dataSize; i++)
                {
                    if (startLocation + dataSize <= rom.Length)
                    {
                        if (rom[startLocation + i] != freeSpaceByte)
                        {
                            break;
                        }
                        else if (i == dataSize)
                        {
                            spaceFound = true;
                            break;
                        }
                    }
                    else
                    {
                        spaceFound = true;
                        MessageBox.Show("No free space could be found in the alloted area. Try widening the search parameters.");
                        startLocation = Int32.MaxValue;
                        break;
                    }
                }
                if (!spaceFound)
                {
                    startLocation++;
                }
            }
            return startLocation;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (Path.GetExtension(openFileDialog1.FileName).ToUpper().Equals(".GBA"))
                {
                    selectedROMPath = openFileDialog1.FileName;
                    numericUpDown1_ValueChanged(sender, e);
                    comboBox1_SelectedIndexChanged(sender, e);
                    string romCode = GetROMCode();
                    btnDecompile.Enabled = true;
                    txtDecompileOffset.Enabled = true;
                    btnDecompile2.Enabled = true;
                    btnChangeMoveScriptPointer.Enabled = true;
                    numericUpDown1.Enabled = true;
                    txtMoveEffectScriptLocation.Enabled = true;
                    comboBox1.Enabled = true;
                    textBox1.Enabled = true;
                    button1.Enabled = true;
                    button2.Enabled = true;
                    txtRomPath.Text = selectedROMPath;
                }
                else
                {
                    try
                    {
                        CreateNewTab(openFileDialog1.FileName);
                        scripts[tabControl1.SelectedIndex].Lines = File.ReadAllLines(openFileDialog1.FileName);
                        unsaved[tabControl1.SelectedIndex] = false;
                    }
                    catch
                    {
                        MessageBox.Show("The file could not be opened.");
                    }
                }
            }
        }

        private string GetROMCode()
        {
            byte[] file = File.ReadAllBytes(selectedROMPath);
            char c;
            string toReturn = "";
            for (int i = 0; i < 4; i++)
            {
                c = Convert.ToChar(file[0xAC + i]);
                toReturn += c;
            }
            return toReturn;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QuickSave(currentlyOpenBS[tabControl1.SelectedIndex]);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    File.WriteAllLines(saveFileDialog1.FileName, scripts[tabControl1.SelectedIndex].Lines.ToArray());
                    int num = 0;
                    if (tabControl1.TabPages[tabControl1.SelectedIndex].Text.Split(' ').Length > 1)
                    {
                        bool success = Int32.TryParse(tabControl1.TabPages[tabControl1.SelectedIndex].Text.Split(' ')[1], out num);
                        if (success)
                        {
                            newScriptNames.Remove(num);
                        }
                    }
                    tabControl1.TabPages[tabControl1.SelectedIndex].Text = Path.GetFileNameWithoutExtension(saveFileDialog1.FileName);
                    currentlyOpenBS[tabControl1.SelectedIndex] = saveFileDialog1.FileName;
                }
                catch (IOException)
                {
                    MessageBox.Show("The file could not be saved.");
                }
                catch
                {
                    MessageBox.Show("There was an unknown error.");
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void newtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewTab(null);
        }
        
        private void CloseScript()
        {
            if (tabControl1.TabPages.Count > 1)
            {
                int toRemove = tabControl1.SelectedIndex;
                bool continueRoutine = true;
                if (unsaved[toRemove])
                {
                    DialogResult result = MessageBox.Show("Your script \"" + tabControl1.TabPages[toRemove].Text + "\" is unsaved. Would you like to save it before closing?",
            "Save?",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (result)
                    {
                        case DialogResult.Yes:
                            result = saveFileDialog1.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                try
                                {
                                    File.WriteAllLines(saveFileDialog1.FileName, scripts[tabControl1.SelectedIndex].Lines.ToArray());
                                }
                                catch
                                {
                                    MessageBox.Show("The file could not be saved. This programme will be kept open.");
                                    continueRoutine = false;
                                }
                            }
                            else
                            {
                                MessageBox.Show("The file could not be saved. This programme will be kept open.");
                                continueRoutine = false;
                            }
                            break;
                        case DialogResult.No:
                            break;
                        default:
                            continueRoutine = false;
                            break;
                    }
                }
                if (continueRoutine)
                {
                    int value = 0;
                    if (tabControl1.TabPages[tabControl1.SelectedIndex].Text.StartsWith("Script ") && Int32.TryParse(tabControl1.TabPages[tabControl1.SelectedIndex].Text.Split(' ')[1], out value))
                    {
                        newScriptNames.Remove(value);
                    }
                    tabControl1.SelectedIndex = 0;
                    tabControl1.TabPages.RemoveAt(toRemove);
                    scripts.RemoveAt(toRemove);
                    unsaved.RemoveAt(toRemove);
                    currentlyOpenBS.RemoveAt(toRemove);
                }
            }
            else
            {
                bool continueRoutine = true;
                if (unsaved[0])
                {
                    DialogResult result = MessageBox.Show("Your script \"" + tabControl1.TabPages[0].Text + "\" is unsaved. Would you like to save it before closing?",
            "Save?",
            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (result)
                    {
                        case DialogResult.Yes:
                            result = saveFileDialog1.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                try
                                {
                                    File.WriteAllLines(saveFileDialog1.FileName, scripts[tabControl1.SelectedIndex].Lines.ToArray());
                                }
                                catch
                                {
                                    MessageBox.Show("The file could not be saved. This programme will be kept open.");
                                    continueRoutine = false;
                                }
                            }
                            else
                            {
                                MessageBox.Show("The file could not be saved. This programme will be kept open.");
                                continueRoutine = false;
                            }
                            break;
                        case DialogResult.No:
                            break;
                        default:
                            continueRoutine = false;
                            break;
                    }
                }
                if (continueRoutine)
                {
                    scripts[0].Clear();
                    currentlyOpenBS[0] = null;
                    tabControl1.TabPages[0].Text = "Script 1";
                    unsaved[0] = true;
                }
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseScript();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void Cut()
        {
            WriteToClipboard(scripts[tabControl1.SelectedIndex].SelectedText);
            scripts[tabControl1.SelectedIndex].SelectedText = "";
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void Copy()
        {
            WriteToClipboard(scripts[tabControl1.SelectedIndex].SelectedText);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void Paste()
        {
            if (this.ActiveControl.GetType() == typeof(FixedRichTextBox))
            {
                if (((FixedRichTextBox)(this.ActiveControl)).SelectionLength == 0)
                {
                    ((FixedRichTextBox)(this.ActiveControl)).SelectedText += GetFromClipboard();
                }
                else
                {
                    ((FixedRichTextBox)(this.ActiveControl)).SelectedText = GetFromClipboard();
                }
            }
            else if (this.ActiveControl.GetType() == typeof(TextBox))
            {
                if (((TextBox)(this.ActiveControl)).SelectionLength == 0)
                {
                    ((TextBox)(this.ActiveControl)).SelectedText += GetFromClipboard();
                }
                else
                {
                    ((TextBox)(this.ActiveControl)).SelectedText = GetFromClipboard();
                }
            }
        }

        private void btnDecompile_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            int location = 0;
            bool success = Int32.TryParse(txtDecompileOffset.Text, out location);
            if (!success)
            {
                success = Int32.TryParse(ToDecimal(txtDecompileOffset.Text), out location);
                if (!success)
                {
                    MessageBox.Show("Could not parse the offset you entered.");
                }
            }
            try
            {
                CreateNewTab(null);
                headersNeeded = new Dictionary<string, bool>();
                headersNeeded.Add("abilities", false);
                headersNeeded.Add("moves", false);
                headersNeeded.Add("pokemon", false);
                headersNeeded.Add("item", false);
                List<string> toReturn = DecompileScript(location, File.ReadAllBytes(selectedROMPath)).ToList();
                bool insertReturn = false;
                int countLines = 0;
                foreach (string s in headersNeeded.Keys)
                {
                    if (headersNeeded[s])
                    {
                        toReturn.Insert(0, "#include " + s + ".bsh");
                        countLines++;
                        insertReturn = true;
                    }
                }
                if (insertReturn)
                {
                    toReturn.Insert(countLines, "");
                }
                scripts[tabControl1.SelectedIndex].Lines = toReturn.ToArray();
                headersNeeded.Clear();
            }
            catch (IOException)
            {
                MessageBox.Show("Unable to read ROM. Please close whatever the ROM is open in, and try again.");
            }
            catch (Exception exc)
            {
                MessageBox.Show("There was an unknown exception. Please forward this error message to Jambo51 through the tool's thread:\n" + exc);
            }
            this.Cursor = Cursors.Default;
            decompiledOffsets.Clear();
            unsaved[tabControl1.SelectedIndex] = false;
        }

        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void commandGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dictionary<string, Command> newCommands = new Dictionary<string, Command>();
            SQLiteConnection con = new SQLiteConnection("Data Source=" + System.Windows.Forms.Application.StartupPath + @"\Data\Commands.sqlite;Version=3;");
            con.Open();
            Command c;
            SQLiteCommand com = new SQLiteCommand("select * from commands order by id asc", con);
            SQLiteDataReader reader = com.ExecuteReader();
            if (reader != null)
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    if (Int32.Parse(reader["numberofparameters"].ToString()) != 0)
                    {
                        List<string> parameterNames = new List<string>();
                        SQLiteCommand com2 = new SQLiteCommand("select name from parameternames where id = " + Byte.Parse(reader["id"].ToString()), con);
                        SQLiteDataReader reader2 = com2.ExecuteReader();
                        while (reader2.Read())
                        {
                            parameterNames.Add(reader2["name"].ToString());
                        }
                        List<byte> parameterLengths = new List<byte>();
                        com2 = new SQLiteCommand("select length from parameterlengths where id = " + Byte.Parse(reader["id"].ToString()), con);
                        reader2 = com2.ExecuteReader();
                        while (reader2.Read())
                        {
                            parameterLengths.Add(Byte.Parse(reader2["length"].ToString()));
                        }
                        c = new Command(UInt16.Parse(reader["id"].ToString()), Byte.Parse(reader["numberofparameters"].ToString()), parameterNames.ToArray(), parameterLengths.ToArray());
                    }
                    else
                    {
                        c = new Command(UInt16.Parse(reader["id"].ToString()), Byte.Parse(reader["numberofparameters"].ToString()));
                    }
                    newCommands.Add(name, c);
                }
            }
            con.Close();
            foreach (string s in commands.Keys)
            {
                newCommands.Add(s, commands[s]);
            }
            Form newForm = new Form2(newCommands, superCommands);
            newForm.Show();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (selectedROMPath != null && !selectedROMPath.Equals(""))
            {
                byte[] rom = File.ReadAllBytes(selectedROMPath);
                string romCode = GetROMCode();
                string[] iniFile = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini");
                bool getValues = false;
                foreach (string s in iniFile)
                {
                    if (s.Equals("[" + romCode + "]"))
                    {
                        getValues = true;
                        continue;
                    }
                    if (getValues)
                    {
                        if (s.StartsWith("MoveEffectTable"))
                        {
                            uint value = 0;
                            bool success = UInt32.TryParse(s.Split('=')[1], out value);
                            if (!success)
                            {
                                success = UInt32.TryParse(ToDecimal(s.Split('=')[1]), out value);
                                if (!success)
                                {
                                    MessageBox.Show("Error parsing the move keyword value from the main ini. Are you sure it is formatted correctly as a number?");
                                }
                            }
                            moveEffectTableLocation = value;
                            getValues = false;
                            break;
                        }
                    }
                }
                string x = "";
                for (int i = 0; i < 4; i++)
                {
                    x += rom[((int)numericUpDown1.Value * 4) + moveEffectTableLocation + 3 - i].ToString("X2");
                }
                int theValue = 0;
                bool newSuccess = Int32.TryParse(ToDecimal("0x" + x), out theValue);
                if (!newSuccess)
                {
                    MessageBox.Show("There was a problem parsing the pointer from the ROM");
                }
                else
                {
                    theValue -= 0x08000000;
                    txtMoveEffectScriptLocation.Text = "0x" + theValue.ToString("X");
                }
            }
        }

        private void btnDecompile2_Click(object sender, EventArgs e)
        {
            txtDecompileOffset.Text = txtMoveEffectScriptLocation.Text;
            btnDecompile_Click(sender, e);
        }

        private void btnChangeMoveScriptPointer_Click(object sender, EventArgs e)
        {
            if (selectedROMPath != null && !selectedROMPath.Equals(""))
            {
                byte[] rom = File.ReadAllBytes(selectedROMPath);
                string romCode = GetROMCode();
                string[] iniFile = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini");
                bool getValues = false;
                foreach (string s in iniFile)
                {
                    if (s.Equals("[" + romCode + "]"))
                    {
                        getValues = true;
                        continue;
                    }
                    if (getValues)
                    {
                        if (s.StartsWith("MoveEffectTable"))
                        {
                            uint value = 0;
                            bool success = UInt32.TryParse(s.Split('=')[1], out value);
                            if (!success)
                            {
                                success = UInt32.TryParse(ToDecimal(s.Split('=')[1]), out value);
                                if (!success)
                                {
                                    MessageBox.Show("Error parsing the move keyword value from the main ini. Are you sure it is formatted correctly as a number?");
                                }
                            }
                            moveEffectTableLocation = value;
                            getValues = false;
                            break;
                        }
                    }
                }
                uint x = 0;
                bool newSuccess = UInt32.TryParse(txtMoveEffectScriptLocation.Text, out x);
                if (!newSuccess)
                {
                    newSuccess = UInt32.TryParse(ToDecimal(txtMoveEffectScriptLocation.Text), out x);
                }
                if (x > 0 && x < 0x02000000)
                {
                    x += 0x08000000;
                }
                for (int i = 0; i < 4; i++)
                {
                    rom[((int)numericUpDown1.Value * 4) + moveEffectTableLocation + 3 - i] = Byte.Parse(ToDecimal("0x" + x.ToString("X8").Substring((i * 2), 2)));
                }
                File.WriteAllBytes(selectedROMPath, rom);
            }
        }

        private void gotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Goto();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Find();
        }

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindAndReplace();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            QuickSave(currentlyOpenBS[tabControl1.SelectedIndex]);
        }

        private void backgroundColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog d = new ColorDialog();
            DialogResult result = d.ShowDialog();
            if (result == DialogResult.OK)
            {
                rtbBackColour = d.Color;
                foreach (FixedRichTextBox rtb in scripts)
                {
                    rtb.BackColor = d.Color;
                    rtbNotes.BackColor = d.Color;
                }
                string[] ini = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini");
                List<string> newIni = new List<string>();
                foreach (string s in ini)
                {
                    if (s.StartsWith("backcolour"))
                    {
                        string[] line = s.Split('=');
                        line[1] = d.Color.ToString().Substring(7).Substring(0, d.Color.ToString().Substring(7).Length - 1);
                        newIni.Add(line[0] + "=" + line[1]);
                    }
                    else
                    {
                        newIni.Add(s);
                    }
                }
                File.WriteAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini", newIni.ToArray());
            }
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form6 info = new Form6();
            info.Show();
        }

        private void checkForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Version newVersion = null;
            XmlTextReader reader = null;
            string url = null;
            try
            {
                string xmlURL = "https://dl.dropbox.com/u/24219056/BSPVersion.xml";
                reader = new XmlTextReader(xmlURL);
                reader.MoveToContent(); 
                string elementName = "";
                if ((reader.NodeType == XmlNodeType.Element) &&
                    (reader.Name == "battlescriptpro"))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            elementName = reader.Name;
                        }
                        else
                        {
                            if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
                            {
                                switch (elementName)
                                {
                                    case "version":
                                        newVersion = new Version(reader.Value);
                                        break;
                                    case "url":
                                        url = reader.Value;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            this.Cursor = Cursors.Default;
            Version curVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (curVersion.CompareTo(newVersion) < 0)
            {
                if (DialogResult.Yes == MessageBox.Show(this, "Download the new version?", "New Version Detected",
                  MessageBoxButtons.YesNo,
                  MessageBoxIcon.Question))
                {
                    System.Diagnostics.Process.Start(url);
                }
            }
            else
            {
                MessageBox.Show("Could not find newer version");
            }
        }

        private void textColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog d = new ColorDialog();
            DialogResult result = d.ShowDialog();
            if (result == DialogResult.OK)
            {
                rtbForeColour = d.Color;
                foreach (FixedRichTextBox rtb in scripts)
                {
                    rtb.SelectAll();
                    rtb.SelectionColor = d.Color;
                    rtb.DeselectAll();
                    rtbNotes.SelectAll();
                    rtbNotes.SelectionColor = d.Color;
                    rtbNotes.DeselectAll();
                    rtb.ForeColor = d.Color;
                    rtbNotes.ForeColor = d.Color;
                }
                string[] ini = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini");
                List<string> newIni = new List<string>();
                foreach (string s in ini)
                {
                    if (s.StartsWith("textcolour"))
                    {
                        string[] line = s.Split('=');
                        line[1] = d.Color.ToString().Substring(7).Substring(0, d.Color.ToString().Substring(7).Length - 1);
                        newIni.Add(line[0] + "=" + line[1]);
                    }
                    else
                    {
                        newIni.Add(s);
                    }
                }
                File.WriteAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini", newIni.ToArray());
            }
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog d = new FontDialog();
            d.Font = rtbFont;
            DialogResult result = d.ShowDialog();
            if (result == DialogResult.OK && (!rtbFont.Name.Equals(d.Font.Name) || (rtbFont.Style != d.Font.Style) || (rtbFont.Size != d.Font.Size)))
            {
                rtbFont = d.Font;
                foreach (FixedRichTextBox rtb in scripts)
                {
                    rtb.SelectAll();
                    rtb.SelectionFont = d.Font;
                    rtb.DeselectAll();
                    rtbNotes.SelectAll();
                    rtbNotes.SelectionFont = d.Font;
                    rtbNotes.DeselectAll();
                    rtb.Font = d.Font;
                    rtbNotes.Font = d.Font;
                }
                string[] ini = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini");
                List<string> newIni = new List<string>();
                foreach (string s in ini)
                {
                    if (s.StartsWith("font="))
                    {
                        string[] line = s.Split('=');
                        line[1] = d.Font.Name;
                        newIni.Add(line[0] + "=" + line[1]);
                    }
                    else if (s.StartsWith("fontstyle="))
                    {
                        string[] line = s.Split('=');
                        line[1] = d.Font.Style.ToString();
                        newIni.Add(line[0] + "=" + line[1]);
                    }
                    else if (s.StartsWith("fontsize="))
                    {
                        string[] line = s.Split('=');
                        line[1] = d.Font.SizeInPoints.ToString();
                        newIni.Add(line[0] + "=" + line[1]);
                    }
                    else
                    {
                        newIni.Add(s);
                    }
                }
                File.WriteAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini", newIni.ToArray());
            }
        }

        private void guideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebsite();
        }

        private void OpenWebsite()
        {
            System.Diagnostics.Process.Start("http://www.pokecommunity.com/showpost.php?p=7595098&postcount=1");
        }

        private void dataLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDataLocation();
        }

        private void OpenDataLocation()
        {
            System.Diagnostics.Process.Start(System.Windows.Forms.Application.StartupPath + @"\Data\");
        }

        private void decompileCommandOffsetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] ini = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini");
            List<string> newIni = new List<string>();
            foreach (string s in ini)
            {
                if (s.StartsWith("decompileOffsets"))
                {
                    string[] line = s.Split('=');
                    line[1] = decompileCommandOffsetsToolStripMenuItem.Checked.ToString();
                    newIni.Add(line[0] + "=" + line[1]);
                }
                else
                {
                    newIni.Add(s);
                }
            }
            File.WriteAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini", newIni.ToArray());
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSettingsForm();
        }

        private void OpenSettingsForm()
        {
            Form7 frm = new Form7(this);
            frm.Show();
        }

        public void ChangeCommentString(string s)
        {
            commentString = s;
        }

        public void ChangeDecompileMode(string s)
        {
            SetDecompileModeByString(s);
        }

        public void ChangeNumberDecompileMode(string s)
        {
            SetNumberDecompileModeByString(s);
        }

        private void SetDecompileModeByString(string s)
        {
            switch (s)
            {
                case "Enhanced":
                    decompileMode = 2;
                    break;
                case "Normal":
                    decompileMode = 1;
                    break;
                default:
                    decompileMode = 0;
                    break;
            }
        }

        private void SetNumberDecompileModeByString(string s)
        {
            switch (s)
            {
                case "Binary":
                    numberDecompileMode = 0;
                    break;
                case "Octal":
                    numberDecompileMode = 1;
                    break;
                case "Decimal":
                    numberDecompileMode = 2;
                    break;
                case "Thornal":
                    numberDecompileMode = 4;
                    break;
                default:
                    numberDecompileMode = 3;
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedROMPath != null && !selectedROMPath.Equals(""))
            {
                byte[] rom = File.ReadAllBytes(selectedROMPath);
                string romCode = GetROMCode();
                string[] iniFile = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini");
                Dictionary<byte, char> characterValues = ReadTableFile(System.Windows.Forms.Application.StartupPath + @"\Data\Table.ini");
                bool getValues = false;
                if (comboBox1.Items.Count == 0)
                {
                    uint numberOfMoves = 0;
                    uint moveNamesLocation = 0;
                    foreach (string s in iniFile)
                    {
                        if (s.Equals("[" + romCode + "]"))
                        {
                            getValues = true;
                            continue;
                        }
                        if (s.Equals("[/" + romCode + "]"))
                        {
                            getValues = false;
                            break;
                        }
                        if (getValues)
                        {
                            if (s.StartsWith("MoveDataTable"))
                            {
                                uint value = 0;
                                bool success = UInt32.TryParse(s.Split('=')[1], out value);
                                if (!success)
                                {
                                    success = UInt32.TryParse(ToDecimal(s.Split('=')[1]), out value);
                                    if (!success)
                                    {
                                        MessageBox.Show("Error parsing the move keyword value from the main ini. Are you sure it is formatted correctly as a number?");
                                    }
                                }
                                moveDataTableLocation = value;
                            }
                            else if (s.StartsWith("NumberOfMoves"))
                            {
                                uint value = 0;
                                bool success = UInt32.TryParse(s.Split('=')[1], out value);
                                if (!success)
                                {
                                    success = UInt32.TryParse(ToDecimal(s.Split('=')[1]), out value);
                                    if (!success)
                                    {
                                        MessageBox.Show("Error parsing the move keyword value from the main ini. Are you sure it is formatted correctly as a number?");
                                    }
                                }
                                numberOfMoves = value;
                            }
                            else if (s.StartsWith("MoveNames"))
                            {
                                uint value = 0;
                                bool success = UInt32.TryParse(s.Split('=')[1], out value);
                                if (!success)
                                {
                                    success = UInt32.TryParse(ToDecimal(s.Split('=')[1]), out value);
                                    if (!success)
                                    {
                                        MessageBox.Show("Error parsing the move keyword value from the main ini. Are you sure it is formatted correctly as a number?");
                                    }
                                }
                                moveNamesLocation = value;
                            }
                        }
                    }
                    for (uint i = 0; i < numberOfMoves; i++)
                    {
                        uint moveNameLocation = moveNamesLocation + ((i + 1) * 13);
                        string s = "";
                        for (int j = 0; j < 13; j++)
                        {
                            if ((rom[moveNameLocation + j] != 0xFF))
                            {
                                char temp = ';';
                                characterValues.TryGetValue(rom[moveNameLocation + j], out temp);
                                s += temp;
                            }
                            else
                            {
                                break;
                            }
                        }
                        comboBox1.Items.Add(s);
                    }
                    comboBox1.SelectedIndex = 0;
                    GetEffectID(rom);
                }
                else
                {
                    GetEffectID(rom);
                }
            }
        }

        private Dictionary<byte, char> ReadTableFile(string iniLocation)
        {
            Dictionary<byte, char> characterValues = new Dictionary<byte, char>();
            string[] tableFile = File.ReadAllLines(iniLocation);
            int index = 0;
            foreach (string s in tableFile)
            {
                if (!s.Equals("") && !s.Equals("[Table]") && index != 0x9E && index != 0x9F)
                {
                    string[] stuff = s.Split('=');
                    switch (Byte.Parse(ToDecimal("0x" + stuff[0])))
                    {
                        case 0:
                            characterValues.Add(0, ' ');
                            break;
                        case 0x34:
                            break;
                        case 0x35:
                            characterValues.Add(0x35, '=');
                            break;
                        case 0x53:
                            break;
                        case 0x54:
                            break;
                        case 0x55:
                            break;
                        case 0x56:
                            break;
                        case 0x57:
                            break;
                        case 0x58:
                            break;
                        case 0x59:
                            break;
                        case 0x79:
                            break;
                        case 0x7A:
                            break;
                        case 0x7B:
                            break;
                        case 0x7C:
                            break;
                        case 0xB0:
                            break;
                        case 0xB5:
                            break;
                        case 0xB6:
                            break;
                        case 0xEF:
                            break;
                        case 0xF7:
                            break;
                        case 0xF8:
                            break;
                        case 0xF9:
                            break;
                        case 0xFA:
                            break;
                        case 0xFB:
                            break;
                        case 0xFC:
                            break;
                        case 0xFD:
                            break;
                        case 0xFE:
                            break;
                        case 0xFF:
                            break;
                        default:
                            characterValues.Add(Byte.Parse(ToDecimal("0x" + stuff[0])), stuff[1].ToCharArray()[0]);
                            break;
                    }
                    index++;
                }
            }
            return characterValues;
        }

        private void GetEffectID(byte[] rom)
        {
            textBox1.Text = rom[((comboBox1.SelectedIndex + 1) * 12) + moveDataTableLocation].ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            decimal temp = 0;
            Decimal.TryParse(textBox1.Text, out temp);
            if (numericUpDown1.Value == temp)
            {
                numericUpDown1_ValueChanged(sender, e);
            }
            else
            {
                numericUpDown1.Value = temp;
            }
            btnDecompile2_Click(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedROMPath != null && !selectedROMPath.Equals(""))
            {
                byte[] rom = File.ReadAllBytes(selectedROMPath);
                string romCode = GetROMCode();
                string[] iniFile = File.ReadAllLines(System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini");
                bool getValues = false;
                foreach (string s in iniFile)
                {
                    if (s.Equals("[" + romCode + "]"))
                    {
                        getValues = true;
                        continue;
                    }
                    if (getValues)
                    {
                        if (s.StartsWith("MoveDataTable"))
                        {
                            uint value = 0;
                            bool success = UInt32.TryParse(s.Split('=')[1], out value);
                            if (!success)
                            {
                                success = UInt32.TryParse(ToDecimal(s.Split('=')[1]), out value);
                                if (!success)
                                {
                                    MessageBox.Show("Error parsing the move keyword value from the main ini. Are you sure it is formatted correctly as a number?");
                                }
                            }
                            moveDataTableLocation = value;
                            getValues = false;
                            break;
                        }
                    }
                }
                uint x = 0;
                bool newSuccess = UInt32.TryParse(textBox1.Text, out x);
                if (!newSuccess)
                {
                    newSuccess = UInt32.TryParse(ToDecimal(textBox1.Text), out x);
                }
                if (x > 0xFF)
                {
                    x = 0;
                }
                rom[((comboBox1.SelectedIndex + 1) * 12) + moveDataTableLocation] = (byte)(x);
                File.WriteAllBytes(selectedROMPath, rom);
            }
        }

        private void downloadCleanINIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you wish to update your INI file? You will lose all modifications made to it.", "Really update?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DownloadINI();
            }
        }

        private void updateDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you wish to update your Database file? You will lose all modifications made to it.", "Really update?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DownloadCommandDatabase();
            }
        }

        private void DownloadINI()
        {
            DownloadFile("BattleScriptPro", "ini");
        }

        private void DownloadCommandDatabase()
        {
            DownloadFile("Commands", "sqlite");
        }

        private void DownloadFile(string fileName, string fileType)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (File.Exists(System.Windows.Forms.Application.StartupPath + @"/Data/" + fileName + "." + fileType))
            {
                bool success = true;
                if (File.Exists(System.Windows.Forms.Application.StartupPath + @"/Data/" + fileName + "_backup." + fileType))
                {
                    try
                    {
                        File.Delete(System.Windows.Forms.Application.StartupPath + @"/Data/" + fileName + "_backup." + fileType);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to remove existing backed up file. Please manually update your file.");
                        success = false;
                    }
                }
                if (success)
                {
                    try
                    {
                        File.Move(System.Windows.Forms.Application.StartupPath + @"/Data/" + fileName + "." + fileType, System.Windows.Forms.Application.StartupPath + @"/Data/" + fileName + "_backup." + fileType);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to backup file.");
                        success = false;
                    }
                    if (success)
                    {
                        try
                        {
                            using (WebClient Client = new WebClient())
                            {
                                Client.DownloadFile("https://dl.dropboxusercontent.com/u/24219056/" + fileName + "." + fileType, System.Windows.Forms.Application.StartupPath + @"/Data/" + fileName + "." + fileType);
                            }
                            MessageBox.Show("Success!");
                        }
                        catch
                        {
                            MessageBox.Show("Could not update file. Restoring backup...");
                            File.Move(System.Windows.Forms.Application.StartupPath + @"/Data/" + fileName + "_backup." + fileType, System.Windows.Forms.Application.StartupPath + @"/Data/" + fileName + "." + fileType);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    using (WebClient Client = new WebClient())
                    {
                        Client.DownloadFile("https://dl.dropboxusercontent.com/u/24219056/" + fileName + "." + fileType, System.Windows.Forms.Application.StartupPath + @"/Data/" + fileName + "." + fileType);
                    }
                    MessageBox.Show("Success!");
                }
                catch
                {
                    MessageBox.Show("Unable to download file. Please manually update your file.");
                }
            }
            Cursor.Current = Cursors.Default;
        }
    }
}