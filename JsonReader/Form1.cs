using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace JsonReader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool GoOn = true;
        // string json = @"{'CPU': 'Intel','PSU': '500W','Drives': ['DVD read/writer'/*(broken)*/,'500 gigabyte hard drive','200 gigabype hard drive']}";

        private void button1_Click(object sender, EventArgs e)
        { ReadPropertyNames(); }

        private void ReadPropertyNames()
        {
            button1.Enabled = false; button2.Enabled = false; GoOn = true;
            checkedListBox1.Items.Clear(); int Cntr = 0; int PropertyCntr = 0; 
            toolStripStatusLabel1.Text = "   Reading property names: ...";
            try
            {
                var json = new WebClient().DownloadString(textBox2.Text.Trim());
                JsonTextReader reader = new JsonTextReader(new StringReader(json));
                int level = 0; List<string> PropertyNames = new List<string>();
                while (reader.Read() && GoOn)
                {
                    if (reader.TokenType == JsonToken.StartObject) level++;
                    if (reader.TokenType == JsonToken.EndObject) level--;
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        Cntr++;
                        string PropertyName = reader.Value.ToString(); string NewProperty = "";
                        if (checkBox1.Checked) NewProperty = PropertyName; else {
                          if (level > PropertyNames.Count) PropertyNames.Add(PropertyName);
                            else PropertyNames[level - 1] = PropertyName;
                          for (int i = 0; i <= level - 1; i++) 
                            if (NewProperty == "") NewProperty = PropertyNames[i]; else NewProperty += ":" + PropertyNames[i];                            
                        }
                        // textBox1.AppendText(NewProperty + "\r\n");
                        if (!checkedListBox1.Items.Contains(NewProperty)) // add to listbox
                        { checkedListBox1.Items.Add(NewProperty); PropertyCntr++; }
                    }
                    if (GoOn) toolStripStatusLabel1.Text = "   Reading property names: " + Cntr; Application.DoEvents();
                }
                if (GoOn) toolStripStatusLabel1.Text = "   " + PropertyCntr + " property names found";
            }
            catch (Exception exc)
            {
                toolStripStatusLabel1.Text = "   Error reading " + textBox2.Text.Trim() + "; "+exc.Message;                
            }
            button1.Enabled = true;
        }


        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // SystemSounds.Beep.Play();
            // toolStripStatusLabel1.Text = "   " + checkedListBox1.CheckedItems.Count + " of " + checkedListBox1.Items.Count + "items selected ";
        }

        private void Selection_Changed()
        {
            //SystemSounds.Beep.Play();
            toolStripStatusLabel1.Text = "   " + checkedListBox1.CheckedItems.Count + " of " + checkedListBox1.Items.Count + " items are selected.";
            button2.Enabled = (checkedListBox1.CheckedItems.Count > 0);
        }

        private void checkedListBox1_MouseUp(object sender, MouseEventArgs e)
        { Selection_Changed(); }

        private void checkedListBox1_KeyUp(object sender, KeyEventArgs e)
        { if (!toolStripStatusLabel1.Text.Contains("cancel")) Selection_Changed(); }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = false; button2.Enabled = false; checkedListBox1.Enabled = false; GoOn = true;
            dataGridView1.Columns.Clear(); dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            for (int cols = 1; cols <= checkedListBox1.CheckedItems.Count; cols++)
            {
                string colname = "Column" + cols;
                dataGridView1.Columns.Add(colname, checkedListBox1.CheckedItems[cols - 1].ToString());
            }
            int Cntr = 0; int ValueCntr = 0; 
            toolStripStatusLabel1.Text = "   Reading values: ...";
            try
            {
                var json = new WebClient().DownloadString(textBox2.Text.Trim());
                JsonTextReader reader = new JsonTextReader(new StringReader(json));
                bool addrow = true; bool doreadvalue=false;
                int level = 0; int highestlevel = 1;
                List<string> PropertyNames = new List<string>();
                string FullProperty = "";
                while (reader.Read() && GoOn)
                {                    
                    if (doreadvalue && reader.Value !=null) { // do read value
                        for (int col = 1; col <= checkedListBox1.CheckedItems.Count; col++) {
                          if (dataGridView1.Columns[col - 1].HeaderText == FullProperty) {
                             // first add row if necessary: when new object or when value in cell != null                                
                             if (!addrow) if (dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[col - 1].Value != null) addrow = true;
                             if (addrow)
                             {
                                 dataGridView1.Rows.Add(1); addrow = false; 
                                 // dataGridView1.Rows[dataGridView1.Rows.Count - 1].HeaderCell.Value = (dataGridView1.Rows.Count).ToString();
                             }
                             dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[col - 1].Value = reader.Value;
                             // textBox1.AppendText(reader.Value.ToString()+"\r\n");
                             if (level > highestlevel) highestlevel = level;
                             ValueCntr++;
                          }
                        }
                    } // add value
                    if (reader.TokenType == JsonToken.StartObject)  level++; //addrow = true; }
                    if (reader.TokenType == JsonToken.EndObject) { 
                        level--; 
                        if (level < highestlevel) { addrow = true; highestlevel = level; }
                    }
                    if (reader.TokenType == JsonToken.PropertyName) {
                        Cntr++;
                        string PropertyName = reader.Value.ToString(); FullProperty = "";
                        if (checkBox1.Checked) FullProperty = PropertyName; else {
                            if (level > PropertyNames.Count) PropertyNames.Add(PropertyName);
                            else PropertyNames[level - 1] = PropertyName;
                            for (int i = 0; i <= level - 1; i++)
                                if (FullProperty == "") FullProperty = PropertyNames[i];
                                else FullProperty += ":" + PropertyNames[i];
                        }
                        doreadvalue = true;
                    } else doreadvalue = false;
                    if (GoOn) toolStripStatusLabel1.Text = "   Reading values: " + Cntr; Application.DoEvents();
                }
                if (GoOn)
                {
                    toolStripStatusLabel1.Text = "   " + ValueCntr + " values found";
                    foreach (DataGridViewRow row in dataGridView1.Rows) { row.HeaderCell.Value = String.Format("{0}", row.Index + 1); }
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                    List<int> ColWidths = new List<int>(); // to save current widths
                    foreach (DataGridViewColumn cl in dataGridView1.Columns) ColWidths.Add(cl.Width);
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    foreach (DataGridViewColumn cl in dataGridView1.Columns) cl.Width = ColWidths[cl.Index];                    
                }
                Application.DoEvents();
            }
            catch (Exception exc)
            {
                toolStripStatusLabel1.Text = "   Error reading " + textBox2.Text.Trim() + "; " + exc.Message;
            }
        button1.Enabled = true; button2.Enabled = true; checkedListBox1.Enabled = true;
        } //button2

        private void checkboxchanged()
        { if (checkedListBox1.Items.Count > 0) ReadPropertyNames(); }

        private void checkBox1_Click(object sender, EventArgs e)
        { checkboxchanged(); }

        private void checkBox1_KeyUp(object sender, KeyEventArgs e)
        { checkboxchanged(); }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
             if(e.KeyCode == Keys.A && Control.ModifierKeys == Keys.Control) //ctrl-A
             {
                 if (checkedListBox1.Items.Count>0 && !dataGridView1.Focused && !textBox2.Focused)
                 {
                     if (checkedListBox1.CheckedItems.Count < checkedListBox1.Items.Count)
                         for (int i = 1; i <= checkedListBox1.Items.Count; i++) checkedListBox1.SetItemCheckState(i-1, CheckState.Checked);
                     else for (int i = 1; i <= checkedListBox1.Items.Count; i++) checkedListBox1.SetItemCheckState(i-1, CheckState.Unchecked);        
                 }
             }
             if(e.KeyCode == Keys.C && Control.ModifierKeys == Keys.Control) //ctrl-C 
             {
                 if (!button1.Enabled)
                 {
                     GoOn = false; button1.Enabled = true; checkedListBox1.Enabled = true;
                     button2.Enabled = (checkedListBox1.CheckedItems.Count > 0);
                     Application.DoEvents();
                     toolStripStatusLabel1.Text = "   Process canceled by user.";                     
                 }
             }

        }

        private void textBox2_Enter(object sender, EventArgs e)
        { //
        }

    }
}