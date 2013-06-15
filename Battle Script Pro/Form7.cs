using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Battle_Script_Pro
{
    public partial class Form7 : Form
    {
        Form1 parent = null;
        public Form7(Form1 frm)
        {
            InitializeComponent();
            parent = frm;
            foreach (Control c in this.Controls)
            {
                foreach (Control child in c.Controls)
                {
                    if (child is RadioButton)
                    {
                        ((RadioButton)(child)).CheckedChanged += new EventHandler(radioButtons_CheckedChanged);
                    }
                }
            }
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            string filePath = System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini";
            if (File.Exists(filePath))
            {
                string[] ini = File.ReadAllLines(filePath);
                foreach (string s in ini)
                {
                    if (s.StartsWith("commentString"))
                    {
                        switch (s.Split('=')[1])
                        {
                            case "//":
                                rdBtnBackSlash.Checked = true;
                                break;
                            case ";;":
                                rdBtnSemiColon.Checked = true;
                                break;
                            case "''":
                                rdBtnApostrophe.Checked = true;
                                break;
                            default:
                                rdBtnColon.Checked = true;
                                break;
                        }
                    }
                    else if (s.StartsWith("decompileMode"))
                    {
                        GroupBox g = (GroupBox)this.Controls["grpBoxDecMode"];
                        ((RadioButton)(g.Controls["rdBtn" + s.Split('=')[1]])).Checked = true;
                    }
                    else if (s.StartsWith("numberDecompileMode"))
                    {
                        GroupBox g = (GroupBox)this.Controls["grpBoxNumberDecMode"];
                        ((RadioButton)(g.Controls["rdBtn" + s.Split('=')[1]])).Checked = true;
                    }
                }
            }
            else
            {
                rdBtnColon.Checked = true;
                rdBtnNormal.Checked = true;
                rdBtnHex.Checked = true;
            }
        }

        private void radioButtons_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton originator = (RadioButton)sender;
            if (originator.Checked)
            {
                List<string> newIni = new List<string>();
                string name = originator.Name.Substring(5);
                string filePath = System.Windows.Forms.Application.StartupPath + @"\Data\BattleScriptPro.ini";
                if (originator.Parent == this.Controls["grpBoxComments"])
                {
                    if (File.Exists(filePath))
                    {
                        string[] ini = File.ReadAllLines(filePath);
                        switch (name)
                        {
                            case "SemiColon":
                                name = ";;";
                                break;
                            case "BackSlash":
                                name = "//";
                                break;
                            case "Apostrophe":
                                name = "''";
                                break;
                            default:
                                name = "::";
                                break;
                        }
                        foreach (string s in ini)
                        {
                            if (s.StartsWith("commentString"))
                            {
                                newIni.Add("commentString=" + name);
                            }
                            else
                            {
                                newIni.Add(s);
                            }
                        }
                        parent.ChangeCommentString(name);
                        File.WriteAllLines(filePath, newIni.ToArray());
                    }
                }
                else if (originator.Parent == this.Controls["grpBoxDecMode"])
                {
                    if (File.Exists(filePath))
                    {
                        string[] ini = File.ReadAllLines(filePath);
                        foreach (string s in ini)
                        {
                            if (s.StartsWith("decompileMode"))
                            {
                                newIni.Add("decompileMode=" + name);
                            }
                            else
                            {
                                newIni.Add(s);
                            }
                        }
                        parent.ChangeDecompileMode(name);
                        File.WriteAllLines(filePath, newIni.ToArray());
                    }
                }
                else
                {
                    if (File.Exists(filePath))
                    {
                        string[] ini = File.ReadAllLines(filePath);
                        foreach (string s in ini)
                        {
                            if (s.StartsWith("numberDecompileMode"))
                            {
                                newIni.Add("numberDecompileMode=" + name);
                            }
                            else
                            {
                                newIni.Add(s);
                            }
                        }
                        parent.ChangeNumberDecompileMode(name);
                        File.WriteAllLines(filePath, newIni.ToArray());
                    }
                }
            }
        }
    }
}
