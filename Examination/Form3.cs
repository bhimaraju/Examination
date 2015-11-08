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
    public partial class Form3 : Form
    {
        private decimal _grade;
        private int _maxScore;

        public Form3(decimal score, int maxScore)
        {
            _grade = score;
            _maxScore = maxScore;
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            this.label1.Text = string.Format("Your score is: {0} out of {1}", _grade, _maxScore);

            label2.Left = (this.ClientSize.Width - label2.Width) / 2;
            label2.Top = 50;

            label1.Left = (this.ClientSize.Width - label1.Width) / 2;
            label1.Top = 100;

            button1.Left = (this.ClientSize.Width - button1.Width) / 2;
            button1.Top = 150;        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
