using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Battle_Script_Pro
{
    public partial class Form8 : Form
    {
        private bool doChangeOne = true;
        private bool doChangeTwo = true;
        
        public Form8(List<string> left, List<string> right)
        {
            InitializeComponent();
            foreach (string s in left)
            {
                listBox1.Items.Add(s);
            }
            foreach (string s in right)
            {
                listBox2.Items.Add(s);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (doChangeOne)
            {
                doChangeTwo = false;
                listBox2.SelectedIndex = listBox1.SelectedIndex;
                doChangeTwo = true;
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (doChangeTwo)
            {
                doChangeOne = false;
                listBox1.SelectedIndex = listBox2.SelectedIndex;
                doChangeOne = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = listBox2.SelectedItem.ToString();
            bool unwritten = true;
            while (unwritten)
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(s);
                    unwritten = false;
                }
                catch
                {
                }
            }
        }
    }
}
