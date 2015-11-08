using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace Examination
{
    public partial class Form1 : Form
    {
        private XmlDocument _xmlFile;
        private List<string> _examNameList;
        private string _examSelected;
        private decimal _duration;
        private string _candidateName;

        public Form1(string candidateName)
        {
            InitializeComponent();
            _candidateName = candidateName;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _xmlFile = new XmlDocument();
                //var xmlLoading = Assembly.GetExecutingAssembly();
                //using (var s = xmlLoading.GetManifestResourceStream(string.Format("{0}.ExamsData.xml",
                                  //                             xmlLoading.GetName().Name)))
                {
                    string pathToFile = Directory.GetCurrentDirectory() + "\\ExamsData.xml";
                    _xmlFile.Load(pathToFile);
                }

                XmlNodeList examList = _xmlFile.DocumentElement.SelectSingleNode("/Exams").ChildNodes;

                _examNameList = new List<string>();
                foreach (XmlNode exam in examList)
                {
                    _examNameList.Add(exam.Attributes["Name"].Value);
                }

                GenerateExamButtons();
            }
            catch (Exception)
            {
                examBtn.Enabled = false;
                label1.Text = "Invalid XML File Detected, Please contact your Admin";
                panel1.Visible = false;
                checkBox1.Visible = false;
            }
        }

        private void GenerateExamButtons()
        {
            int vertical = 15;
            foreach (string examName in _examNameList)
            {
                RadioButton radio = new RadioButton();
                radio.Text = examName;
                radio.Location = new Point(15, vertical);
                radio.Size = new Size(200, 20);
                vertical += 25;
                panel1.Controls.Add(radio);
                panel1.AutoScroll = true;
                panel1.VerticalScroll.Value = 0;
            }

            if (vertical > 100) vertical = 95;
            panel1.Size = new Size(230, vertical + 15);
            this.Controls.Add(panel1);
        }

        private void examBtn_Click(object sender, EventArgs e)
        {
            if(GetSelectedExamName())
            {
                this.Hide();
                var form2 = new Form2(_examSelected, _candidateName);
                form2.Closed += (s, args) => this.Close();
                form2.Show();
            }
        }

        private bool GetSelectedExamName()
        {
            if (panel1.Controls.OfType<RadioButton>().Any(rb => rb.Checked))
            {
                _examSelected = panel1.Controls.OfType<RadioButton>().First(r => r.Checked).Text;
                if(checkBox1.Checked)  return true;
                DialogResult result = MessageBox.Show("Please agree to our terms", "Terms and Conditions", MessageBoxButtons.OK);
                return false;
            }
            else
            {
                DialogResult result = MessageBox.Show("Please select an exam", "No Exam Selected", MessageBoxButtons.OK);
                return false;
            }
        }


        private void button1_Click_1(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}