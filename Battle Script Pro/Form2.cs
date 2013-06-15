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
    public partial class Form2 : Form
    {
        Dictionary<string, Command> commands;
        Dictionary<string, SuperCommand> superCommands;
        int dividerLocation;
        public Form2(Dictionary<string, Command> theCommands, Dictionary<string, SuperCommand> theSuperCommands)
        {
            InitializeComponent();
            commands = theCommands;
            superCommands = theSuperCommands;
            foreach (string s in commands.Keys)
            {
                comboBox1.Items.Add(s.ToLower());
            }
            comboBox1.Items.Add("-----------------------");
            dividerLocation = comboBox1.Items.Count - 1;
            foreach (string s in superCommands.Keys)
            {
                comboBox1.Items.Add(s.ToLower());
            }
            comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            try
            {
                if (comboBox1.SelectedIndex < dividerLocation)
                {
                    Command c = commands[comboBox1.SelectedItem.ToString().ToUpper()];
                    for (int i = 0; i < c.NumberOfParameters; i++)
                    {
                        listBox1.Items.Add("Name: " + c.ParameterNames[i] + "  Length: " + c.ParameterLengths[i]);
                    }
                    label1.Text = "Command ID: 0x" + c.HexID.ToString("X");
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    SuperCommand s = superCommands[comboBox1.SelectedItem.ToString().ToUpper()];
                    for (int i = 0; i < s.NumberOfParameters; i++)
                    {
                        listBox1.Items.Add("Name: " + s.ParameterNames[i] + "  Length: " + s.ParameterLengths[i]);
                    }
                    label1.Text = "";
                }
                catch (KeyNotFoundException)
                {
                    label1.Text = "";
                }
            }
        }
    }
}
