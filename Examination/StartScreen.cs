using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Examination
{
    public partial class StartScreen : Form
    {
        private string _mode;
        private string _name;

        public StartScreen()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (panel1.Controls.OfType<RadioButton>().Any(rb => rb.Checked))
            {
                _mode = panel1.Controls.OfType<RadioButton>().First(r => r.Checked).Text;
                if (!string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    _name = textBox1.Text;
                    NextScreen();
                }
                else
                {
                    DialogResult result = MessageBox.Show("Please enter your name", "No Name Selected", MessageBoxButtons.OK);
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("Please select an option", "No Option Selected", MessageBoxButtons.OK);
            }
        }

        private void NextScreen(){
            if (_mode == "Administrator")
            {
                this.Hide();
                var form4 = new Form4();
                form4.Closed += (s, args) => this.Show();
                form4.Show();
            }
            else if (_mode == "Test Candidate")
            {
                this.Hide();
                var form2 = new Form1(_name);
                form2.Closed += (s, args) => this.Show();
                form2.Show();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
