using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Examination;
using System.IO;
using System.Reflection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Web;

namespace Examination
{
    public partial class Form2 : Form
    {
        private string _examName;
        private Panel _questionsPanel = new Panel();
        private XmlDocument _xmlFile;
        private Dictionary<int, string> _candidateMCQAnswers;
        private Dictionary<int, string[]> _candidateEssayAnswers;
        private Dictionary<int, string> _answersKey = new Dictionary<int, string>();
        private decimal _grade;
        private int _numberOfQuestions;
        private int _numberOfMcqQuestions;
        private Label _timerText = new Label();
        private Button _submit = new Button();
        private string _studentName;
        private int _examDurationInMin = 10;
        private XmlWriter _writer;
        private DateTime _startTime;
        private XmlDocument _candidateAllAnswers;

        //Creates XML File of the responses
        private void CreateAnsXml()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (_writer = XmlWriter.Create(_studentName + "_" + string.Format("{0:yyyyMMdd}-{0:HHmmssfff}.xml", _startTime), settings))
            {
                _writer.WriteStartDocument(true);
                // _writer.Settings.Indent = true;
                _writer.WriteStartElement("Exam");
                _writer.WriteAttributeString("Name", _examName);
                _writer.WriteAttributeString("Candidate", _studentName);
                _writer.WriteAttributeString("StartTime", _startTime.ToLongTimeString());

                foreach (KeyValuePair<int, string> pair in _candidateMCQAnswers)
                {
                    CreateNode(pair.Key.ToString(), "MCQ", pair.Value);
                }

                foreach (KeyValuePair<int, string[]> pair in _candidateEssayAnswers)
                {
                    CreateNode(pair.Key.ToString(), "Essay", pair.Value[0], pair.Value[1]);
                }

                _writer.WriteEndElement();
                _writer.WriteEndDocument();
            }
        }


        //Helper Method to CreateAnsXML()
        private void CreateNode(string qId, string qType, string mainAns, string theoryAns = null)
        {
            _writer.WriteStartElement("Answer");

            _writer.WriteAttributeString("QId", qId);
            _writer.WriteAttributeString("Type", qType);

            _writer.WriteStartElement("MainAnswer");
            _writer.WriteString(mainAns);
            _writer.WriteEndElement();

            _writer.WriteStartElement("TheoryAnswer");
            _writer.WriteString(theoryAns);
            _writer.WriteEndElement();

            _writer.WriteEndElement();
        }

        public Form2(string examName, string studentName)
        {
            InitializeComponent();
            _examName = examName;
            _studentName = studentName;
            _startTime = DateTime.Now;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            GenerateHeaders();

            _questionsPanel.Name = "Questions";
            _questionsPanel.Location = new Point(12, 92);
            _questionsPanel.Size = new Size(700, 400);
            _questionsPanel.BackColor = Color.LightGray;
            _questionsPanel.BorderStyle = BorderStyle.FixedSingle;
            _questionsPanel.AutoScroll = true;

            GenerateQuestions();

            this.Controls.Add(_questionsPanel);

            _submit.Location = new Point(20, 512);
            _submit.Text = "Submit";
            _submit.Click += new System.EventHandler(this.submit_Click);
            this.Controls.Add(_submit);

            _questionsPanel.Controls.OfType<Panel>().FirstOrDefault(r => r.Name == "1").Controls.OfType<Label>().FirstOrDefault().Select();
        }

        //Candidate Name, Exam Name, Duration, Elapsed Time
        private void GenerateHeaders()
        {
            this.Text = _examName;
            Label heading1 = new Label();
            heading1.Text = "Candidate: " + _studentName;
            heading1.AutoSize = false;
            heading1.TextAlign = ContentAlignment.MiddleCenter;
            heading1.Dock = DockStyle.Top;
            heading1.Location = new Point(250, 2);


            Label heading2 = new Label();
            heading2.Text = " Examination: " + _examName;
            heading2.AutoSize = false;
            heading2.TextAlign = ContentAlignment.MiddleCenter;
            heading2.Dock = DockStyle.Top;
            heading2.Location = new Point(275, 22);


            Label heading3 = new Label();
            heading3.Text = "Duration: " + _examDurationInMin + " min";
            heading3.AutoSize = false;
            heading3.TextAlign = ContentAlignment.MiddleCenter;
            heading3.Dock = DockStyle.Top;
            heading3.Location = new Point(280, 42);

            timer1.Enabled = true;
            timer1.Start();
            _timerText.AutoSize = false;
            _timerText.TextAlign = ContentAlignment.MiddleCenter;
            _timerText.Dock = DockStyle.Top;
            _timerText.Location = new Point(265, 62);
            _timerText.Text = "Time Remaining: " + (_examDurationInMin).ToString() + " min";

            this.Controls.Add(_timerText);
            this.Controls.Add(heading3);
            this.Controls.Add(heading2);
            this.Controls.Add(heading1);
        }

        private void submit_Click(object sender, EventArgs e)
        {
            if (ShowConfirmationDialog())
            {
                timer1.Stop();
                SaveAnswers();
                Grade();
                LaunchScoreScreen();
            }
        }

        //Launches when user confirms submission
        private void LaunchScoreScreen()
        {
            this.Hide();
            var form3 = new Form3(_grade, _numberOfQuestions * 5);
            form3.Closed += (s, args) => this.Close();
            form3.Show();
        }

        private bool ShowConfirmationDialog()
        {
            DialogResult result = MessageBox.Show("Are you sure you want to Submit the test?", "Submit Confirmation", MessageBoxButtons.YesNo);
            return (result == DialogResult.Yes);
        }

        //Saves Answers
        private void SaveAnswers()
        {
            GetMCQAnswers();
            GetEssayAnswers();
            CreateAnsXml();
        }

        //Grades the user
        private void Grade()
        {
            ReadCandidateAnswers();
            _grade = 0;

            for (int i = 0; i < _numberOfMcqQuestions; i++)
            {
                string xPathCorrectChoice = string.Format("/Exams/Exam[@Name=\"{0}\"]/Question[@Id=\"{1}\"]/Options/Option[@Answer=\"True\"]", _examName, i + 1);
                string correctAns = _xmlFile.SelectSingleNode(xPathCorrectChoice).InnerText;

                if (!string.IsNullOrWhiteSpace(correctAns))
                {
                    string xPath = string.Format("/Exam/Answer[@QId=\"{0}\"]/MainAnswer[1]", (i + 1).ToString());

                    string candAnswer = _candidateAllAnswers.SelectSingleNode(xPath).InnerText;

                    if (correctAns == candAnswer) _grade += 5m;
                }
            }

            GradeEssayAnswers();
        }


        //Reads Candidate Answers
        private void ReadCandidateAnswers()
        {
            _candidateAllAnswers = new XmlDocument();
            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileName = _studentName + "_" + string.Format("{0:yyyyMMdd}-{0:HHmmssfff}.xml", _startTime);
            string path = Path.Combine(folder, fileName);
            _candidateAllAnswers.Load(path);
        }

        //Grades Essay Answers
        private void GradeEssayAnswers()
        {
            for (int i = _numberOfMcqQuestions; i < _numberOfQuestions; i++)
            {
                string xPathCorrectMainAns = string.Format("/Exams/Exam[@Name=\"{0}\"]/Question[@Id=\"{1}\"]/SubQuestion[@Part='Blank']/Answer", _examName, i + 1);
                string correctMainAns = _xmlFile.SelectSingleNode(xPathCorrectMainAns).InnerText;

                string xPathCorrectTheoryAns = string.Format("/Exams/Exam[@Name=\"{0}\"]/Question[@Id=\"{1}\"]/SubQuestion[@Part='Theory']/Answer", _examName, i + 1);
                string correctTheoryAns = _xmlFile.SelectSingleNode(xPathCorrectTheoryAns).InnerText;

                string xPathMain = string.Format("/Exam/Answer[@QId=\"{0}\"]/MainAnswer", (i + 1).ToString());
                string candMainAns = _candidateAllAnswers.SelectSingleNode(xPathMain).InnerText;

                string xPathTheory = string.Format("/Exam/Answer[@QId=\"{0}\"]/TheoryAnswer", (i + 1).ToString());
                string candTheoryAns = _candidateAllAnswers.SelectSingleNode(xPathTheory).InnerText;

                if (!string.IsNullOrWhiteSpace(candMainAns))
                {

                    string newWord = ApiSpellCheck(candMainAns);

                    if (!string.IsNullOrWhiteSpace(newWord) && !string.Equals(newWord, candMainAns))
                    {
                        candMainAns = newWord;
                    }

                    if (string.Equals(candMainAns.ToLower(), correctMainAns.ToLower())) _grade += 3.75m;
                }

                if (!string.IsNullOrWhiteSpace(candTheoryAns))
                {
                    if (ConceptCheck(i)) _grade += 1.25m;
                }
            }
        }

        //Checks API for typos
        public string ApiSpellCheck(string mainAns)
        {
            string uri = "http://service.afterthedeadline.com/";
            string uriText = Uri.EscapeDataString(mainAns);
            string variables = string.Format("key={0}&data={1}", GetRandomHexNumber(), uriText);
            string apiCall = "checkDocument?";
            //string xmlFromApi = RunAsync(uri, apiCall, variables).Result;
            string xmlFromApi = CallAPI(uri, apiCall, variables);

            XmlDocument xDoc = new XmlDocument();
            if (xmlFromApi != null) xDoc.LoadXml(xmlFromApi);
            else return null;

            //Console.WriteLine(xDoc.SelectSingleNode("/results/error/suggestions//option[1]").InnerText);
            if (xDoc.SelectSingleNode("/results").HasChildNodes)
            {
                return xDoc.SelectSingleNode("/results/error/suggestions//option[1]").InnerText;
            }
            else return null;
        }

        //Checks API for the concept of a text
        public bool ConceptCheck(int i)
        {
            string xPathTheory = string.Format("/Exam/Answer[@QId=\"{0}\"]/TheoryAnswer[1]", (i + 1).ToString());
            string candTheoryAns = _candidateAllAnswers.SelectSingleNode(xPathTheory).InnerText;

            string xPathCorrectTheoryAns = string.Format("/Exams/Exam[@Name=\"{0}\"]/Question[@Id=\"{1}\"]/SubQuestion[@Part='Theory']/Answer", _examName, i + 1);
            string correctTheoryAns = _xmlFile.SelectSingleNode(xPathCorrectTheoryAns).InnerText;

            string candTheoryConcept = GetConceptFromApi(candTheoryAns);
            if (string.IsNullOrWhiteSpace(candTheoryConcept)) return false;
            if (candTheoryConcept == GetConceptFromApi(correctTheoryAns)) return true;
            return false;
        }

        //Helper class to CheckConcept()
        private string GetConceptFromApi(string theory)
        {
            string uri = "http://gateway-a.watsonplatform.net/";
            string apiCall = "calls/text/TextGetRankedConcepts?";
            string apiKey = "30a207f7428e846712ecfca49854355a7952d616";
            string encodedText = Uri.EscapeDataString(theory);
            string variables = string.Format("apikey={0}&text={1}&maxRetrieve=1", apiKey, encodedText);
            string xmlFromApi = CallAPI(uri, apiCall, variables);
            //Console.WriteLine(xmlFromApi);
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(xmlFromApi);
                string concept = xDoc.SelectSingleNode("/results/concepts/concept/").InnerText;
                return concept;
            }
            catch (System.Xml.XPath.XPathException)
            {
                return null;
            }
            catch (XmlException)
            {
                return null;
            }
            catch (NullReferenceException)
            {
                return null;
            }

        }

        private static Random random = new Random();
        private static readonly object syncLock = new object();

        //Generates Random Number Not used anymroe
        private static string GetRandomHexNumber()
        {
            int digits = 32;
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }


        //Connects to API
        private string CallAPI(string uri, string apiCall, string variables)
        {
            try
            {
                //string apiUri = uri + apiCall + variables;
                //string content;
                //HttpWebRequest request = null;
                //HttpWebResponse response = null;

                //while (request == null)
                //{
                //    request = (HttpWebRequest)WebRequest.Create(apiUri);

                //}
                //while (response == null)
                //{
                //    response = (HttpWebResponse)request.GetResponse();
                //}

                //content = new StreamReader(response.GetResponseStream()).ReadToEnd();
                //return content;
                if (uri == "http://service.afterthedeadline.com/") System.Threading.Thread.Sleep(1000);
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(uri);

                //// Add an Accept header for JSON format.
                //client.DefaultRequestHeaders.Accept.Add(
                //new MediaTypeWithQualityHeaderValue("application/json"));

                // List data response.
                HttpResponseMessage response = client.GetAsync(apiCall + variables).Result;  // Blocking call!
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    string dataObjects = response.Content.ReadAsStringAsync().Result;
                    //Console.WriteLine(dataObjects);
                    return dataObjects;
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (HttpRequestException)
            {
                NetworkErrorDialogBox();
                return null;
            }
            catch (WebException)
            {
                NetworkErrorDialogBox();
                return null;
            }
            return null;
        }

        private void NetworkErrorDialogBox()
        {
            DialogResult result = MessageBox.Show("There seems to be something wrong with your network. Only your Multiple Choice Questions have been graded, please contact the administrator for your correct score.", "Network Error", MessageBoxButtons.OK);
            if (result == DialogResult.OK || result == DialogResult.Cancel) this.Close();
        }

        private void GetMCQAnswers()
        {
            _candidateMCQAnswers = new Dictionary<int, string>();
            for (int i = 0; i < _numberOfMcqQuestions; i++)
            {
                var panelName = string.Format("{0}", i + 1);
                string checkedAnswer = null;
                Panel radioPanel = _questionsPanel.Controls.OfType<Panel>().FirstOrDefault(r => r.Name == panelName);

                if (radioPanel.Controls.OfType<RadioButton>().Any(rb => rb.Checked))
                {
                    checkedAnswer = radioPanel.Controls.OfType<RadioButton>().First(r => r.Checked).Text;
                }
                _candidateMCQAnswers.Add(i + 1, checkedAnswer);
            }
        }

        private void GetEssayAnswers()
        {
            _candidateEssayAnswers = new Dictionary<int, string[]>();

            for (int i = _numberOfMcqQuestions; i < _numberOfQuestions; i++)
            {
                var panelName = string.Format("{0}", i + 1);
                Panel essayPanel = _questionsPanel.Controls.OfType<Panel>().FirstOrDefault(r => r.Name == panelName);
                string mainAns = essayPanel.Controls.OfType<TextBox>().FirstOrDefault().Text;
                string explanation = essayPanel.Controls.OfType<RichTextBox>().FirstOrDefault().Text;
                string[] essayArr = { mainAns, explanation };
                _candidateEssayAnswers.Add(i + 1, essayArr);
            }
        }

        private void GetNumberOfQuestions()
        {
            var nodeXPath = string.Format("/Exams/Exam[@Name = '{0}']", _examName);
            XmlElement root = _xmlFile.DocumentElement;
            XmlNode node = root.SelectSingleNode(nodeXPath);
            _numberOfQuestions = node.ChildNodes.Count;
            _numberOfMcqQuestions = node.SelectNodes("Question[@Type = \"MCQ\"]").Count;
            _numberOfEssayQuestions = _numberOfQuestions - _numberOfMcqQuestions;
        }

        //Gets the Questions from XML and loads them on the screen

        private void GenerateQuestions()
        {
            _xmlFile = new XmlDocument();
            var xmlLoading = Assembly.GetExecutingAssembly();
            using (var s = xmlLoading.GetManifestResourceStream(string.Format("{0}.ExamsData.xml",
                                                           xmlLoading.GetName().Name)))
            {
                _xmlFile.Load(s);
            }

            XmlElement root = _xmlFile.DocumentElement;
            GetNumberOfQuestions();

            Panel[] panels = new Panel[_numberOfQuestions];

            for (int j = 0; j < _numberOfMcqQuestions; j++)
            {
                var nodeXPath = string.Format("/Exams/Exam[@Name = '{0}']/Question[@Id = '{1}' ]", _examName, j + 1);
                XmlNode node = root.SelectSingleNode(nodeXPath);
                var question = new Label();
                question.Text = string.Format("{0}: ", j + 1) + node.SelectSingleNode("Text[1]").InnerText;
                question.Size = new Size(500, 20);
                panels[j] = new Panel();
                var panelName = string.Format("{0}", j + 1);
                panels[j].Name = panelName;
                panels[j].Controls.Add(question);
                panels[j].Location = new Point(30, j * 130 + 10);
                panels[j].Size = new Size(600, 120);
                panels[j].BackColor = Color.BurlyWood;
                panels[j].BorderStyle = BorderStyle.FixedSingle;

                for (int i = 0; i <= 3; i++)
                {
                    var radioButton = new RadioButton();
                    var xPath = String.Format("Options[1]/Option[@Id = '{0}']", i + 1);
                    radioButton.Text = node.SelectSingleNode(xPath).InnerText;
                    radioButton.Size = new Size(500, 18);
                    radioButton.Location = new Point(30, 20 + i * 20);
                    panels[j].Controls.Add(radioButton);
                }
                _questionsPanel.Controls.Add(panels[j]);
            }

            for (int j = _numberOfMcqQuestions; j < _numberOfQuestions; j++)
            {
                panels[j] = new Panel();
                var panelName = string.Format("{0}", j + 1);
                panels[j].Name = panelName;

                panels[j].Location = new Point(30, _numberOfMcqQuestions * 130 + (j - _numberOfMcqQuestions) * 170 + 10);
                panels[j].Size = new Size(600, 160);
                panels[j].BackColor = Color.BurlyWood;
                panels[j].BorderStyle = BorderStyle.FixedSingle;

                var nodeXPath = string.Format("/Exams/Exam[@Name = '{0}']/Question[@Id = '{1}' ]/Topic", _examName, j + 1);
                XmlNode node = root.SelectSingleNode(nodeXPath);
                var topic = new Label();
                topic.Text = string.Format("{0}: ", j + 1) + node.InnerText;
                topic.Location = new Point(0, 0);
                topic.Size = new Size(200, 20);
                panels[j].Controls.Add(topic);

                nodeXPath = string.Format("/Exams/Exam[@Name = '{0}']/Question[@Id = '{1}' ]/SubQuestion[@Part = 'Theory']/Text", _examName, j + 1);
                node = root.SelectSingleNode(nodeXPath);
                var question1 = new Label();
                question1.Text = "a: " + node.InnerText;
                question1.Size = new Size(500, 20);
                question1.Location = new Point(10, 25);
                panels[j].Controls.Add(question1);

                var richTextBox = new RichTextBox();
                richTextBox.Size = new Size(500, 60);
                richTextBox.Location = new Point(10, 50);
                panels[j].Controls.Add(richTextBox);

                nodeXPath = string.Format("/Exams/Exam[@Name = '{0}']/Question[@Id = '{1}' ]/SubQuestion[@Part = 'Blank']/Text", _examName, j + 1);
                node = root.SelectSingleNode(nodeXPath + "");
                var question2 = new Label();
                question2.Text = "b: " + node.InnerText;
                question2.Size = new Size(500, 20);
                question2.Location = new Point(10, 115);
                panels[j].Controls.Add(question2);

                var textBox = new TextBox();
                textBox.Size = new Size(500, 20);
                textBox.Location = new Point(10, 135);
                panels[j].Controls.Add(textBox);

                _questionsPanel.Controls.Add(panels[j]);
            }
        }

        private int _duration = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            _duration++;
            _timerText.Text = "Time Remaining: " + (_examDurationInMin - _duration).ToString() + " min";
            SaveAnswers(); //Saves answers every minute
        }

        public int _numberOfEssayQuestions { get; set; }
    }
}