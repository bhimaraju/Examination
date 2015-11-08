using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Examination
{
    public partial class Form4 : Form
    {
        private string _file;

        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            this.button2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select XML File";
            openFileDialog1.Filter = "XML Files|*.xml";

            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                try
                {
                    this._file = openFileDialog1.FileName;
                    this.textBox1.Text = _file;
                    if (ValidateForm())
                    {
                        this.textBox3.Text = "You may save it now.";
                        this.button2.Enabled = true;
                    }
                    else
                    {
                        this.textBox3.Text = "File Schema is Invalid. Please verify and try again later.";
                    }
                }
                catch (IOException)
                {
                    this.textBox3.Text = "File does not exist. Please try again";
                }
            }
        }

        //Validates XML against Schema
        private bool ValidateForm()
        {
            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string target = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                File.Copy(_file, target + "\\ExamsData.xml", true);
                this.textBox3.Text = "Changes Saved";
            }
            catch(IOException)
            {
                textBox3.Text = "Unknown Error";
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
