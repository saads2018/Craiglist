using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace SnapchatBot
{
    public partial class Form1 : Form
    {
        IWebDriver driver;
        WebDriverWait wait;
        string loginPath;
        string timeFramesPath;
        string executeTime;
        string currentDayPath;
        List<DayStatus> dayStatuses;
        List<TimeFrame> frames1;
        int pause = 1;
        const int timeout = 20;
        int selectedTimeFrameIndex = -1;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private Size sizeNormal;


        public Form1()
        {
            InitializeComponent();
            createNecessaryFiles();
            initiateFormDetails();
        }

        public static ChromeOptions GetChromeOptionsMin()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalChromeOption("useAutomationExtension", false);
            options.BrowserVersion = "107.0";
            return options;
        }

        private void enterLoginDetails()
        {
            IWebElement sdksjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("inputEmailHandle")));
            sdksjl.Clear();
            sdksjl.SendKeys(textBox1.Text);

            IWebElement sdksjl1 = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("inputPassword")));
            sdksjl1.Clear();
            sdksjl1.SendKeys(textBox2.Text);

            Thread.Sleep(1500);
            IWebElement sdkdsjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("login")));
            sdkdsjl.Click();

            saveLoginDetails();
        }

        private void initiateFormDetails()
        {
            executeTime = "";
            button9.Text = "Today's Date: " + DateTime.Now.ToShortDateString();
            dayStatuses = new List<DayStatus>();
            string data =File.ReadAllText(loginPath);
            Login login = JsonConvert.DeserializeObject<Login>(data);
            this.dataGridView1.DataSource = getTimeFrameDetails();

            if(login != null)
            {
                textBox1.Text = login.UserName;
                textBox2.Text = login.Password;
            }

            dayStatuses.Add(new DayStatus("Monday", false));
            dayStatuses.Add(new DayStatus("Tuesday", false));
            dayStatuses.Add(new DayStatus("Wednesday", false));
            dayStatuses.Add(new DayStatus("Thursday", false));
            dayStatuses.Add(new DayStatus("Friday", false));
            dayStatuses.Add(new DayStatus("Saturday", false));
            dayStatuses.Add(new DayStatus("Sunday", false));

            checkPrevDate();
        }

        private void checkPrevDate()
        {
            var prevDate = File.ReadAllText(currentDayPath);

            if (!String.IsNullOrWhiteSpace(prevDate))
            {
                if (prevDate != DateTime.Now.ToShortDateString())
                {
                    var tempFramesList = getTimeFrameDetails();
                    for (int i = 0; i < tempFramesList.Count; i++)
                        tempFramesList[i].Status = "Incomplete";
                    saveTimeFrameDetails(tempFramesList);
                    button9.Text = "Today's Date: " + DateTime.Now.ToShortDateString();
                    File.WriteAllText(currentDayPath, DateTime.Now.ToShortDateString());
                }
            }
            this.dataGridView1.DataSource = getTimeFrameDetails();
            this.Refresh();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(getTimeFrameDetails().Count>0)
            {
                button1.Text = "Restart App";
                var chromeDriverService = ChromeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;
                timer1.Stop();
                button6.Enabled = false;
                button6.Text = "Initiate Reposting";
                panel2.Enabled = true;
                panel7.Enabled = true;
                pause = 1;
                if (driver != null)
                {
                    driver.Quit();
                }

                driver = new ChromeDriver(chromeDriverService, GetChromeOptionsMin());
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));

                //driver = new ChromeDriver();
                try
                {
                    driver.Url = "https://accounts.craigslist.org/login";
                    Activate();
                    Thread.Sleep(2500);

                    enterLoginDetails();
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                MessageBox.Show("Please Create At Least One Time Frame Before Starting The App!","Alert",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
           
        }

        private void saveLoginDetails()
        {
            try
            {
                IWebElement appear = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("account-homepage")));
                Login loginDetails = new Login();
                loginDetails.UserName=textBox1.Text;
                loginDetails.Password = textBox2.Text;
                button4.Enabled = false;
                button6.Enabled = true;

                string json = JsonConvert.SerializeObject(loginDetails);
                File.WriteAllText(loginPath, json);
            }
            catch
            {
                button4.Enabled = true;
                button6.Enabled = false;
            }
        }

        private List<TimeFrame> getTimeFrameDetails()
        {
            List<TimeFrame> timeList = new List<TimeFrame>();
            string data = File.ReadAllText(timeFramesPath);
            var times = JsonConvert.DeserializeObject<List<TimeFrame>>(data);

            if (String.IsNullOrWhiteSpace(data))
                return timeList;
            else
                return times;

        }
        private void saveTimeFrameDetails(List<TimeFrame> times)
        {
            try
            {
                string json = JsonConvert.SerializeObject(times);
                File.WriteAllText(timeFramesPath, json);
            }
            catch
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private void createNecessaryFiles()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"craigListLoginBotFiles");
            if(!File.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            loginPath = path + "\\login.json";
            timeFramesPath = path + "\\timeFrames.json";
            currentDayPath = path + "\\currentDay.txt";
            File.AppendAllText(loginPath, "");
            File.AppendAllText(timeFramesPath, "");
            File.AppendAllText(currentDayPath, "");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
               
            }
            catch(Exception ex)
            {

            }
        }

        private void checkLoginDetails()
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text) || String.IsNullOrWhiteSpace(textBox2.Text))
            {
                button1.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            checkLoginDetails();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            checkLoginDetails();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            enterLoginDetails();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!checkMonday.Checked && !checkTuesday.Checked && !checkWednesday.Checked && !checkThursday.Checked && !checkFriday.Checked && !checkSaturday.Checked && !checkSunday.Checked)
            {
                MessageBox.Show("Please Choose At Least One Day Of The Week!", "Alert!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                List<TimeFrame> frames = getTimeFrameDetails();
                DateTime timePicked = this.dateTimePicker1.Value;
                timePicked = timePicked.AddMinutes(30);
                string days = "";
                List<string> daysList = new List<string>();

                foreach (var day in dayStatuses)
                {
                    if (day.Status)
                    {
                        days += day.Day + ",";
                        daysList.Add(day.Day);
                    }
                }

                if (daysList.Count == 7)
                    days = "Every Day";
                else
                    days = days.Trim().Remove(days.Length - 1);


                if (frames.Count > 0 && (frames.Any(x => this.dateTimePicker1.Value.TimeOfDay >= getDateTime(x.StartTime).TimeOfDay && this.dateTimePicker1.Value.TimeOfDay <= getDateTime(x.EndTime).TimeOfDay && daysList.Any(y => x.Days.Contains(y) || x.Days.Contains("Every Day"))) || frames.Any(x => timePicked.TimeOfDay >= getDateTime(x.StartTime).TimeOfDay && timePicked.TimeOfDay <= getDateTime(x.EndTime).TimeOfDay && daysList.Any(y => x.Days.Contains(y) || x.Days.Contains("Every Day")))))
                {
                    MessageBox.Show("Please Choose A Unique Time-Frame!", "Alert!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    TimeFrame time = new TimeFrame();
                    time.No = frames.Count + 1;
                    time.StartTime = this.dateTimePicker1.Value.ToShortTimeString();
                    time.EndTime = timePicked.ToShortTimeString();
                    time.Status = "Incomplete";
                    time.Days = days;

                    frames.Add(time);
                    saveTimeFrameDetails(frames);

                    this.dateTimePicker1.Value = this.dateTimePicker1.Value.AddMinutes(31);
                    this.dataGridView1.DataSource = frames;
                    this.Refresh();
                }
            }
        }

        private DateTime getDateTime(string time)
        {
            return DateTime.ParseExact(time, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == System.Windows.Forms.Keys.Delete && this.dataGridView1.RowCount>0)
                {
                    var times = getTimeFrameDetails();
                    times.RemoveAt(this.dataGridView1.CurrentRow.Index);

                    for(int i=0;i<times.Count;i++)
                        times[i].No = i + 1;

                    saveTimeFrameDetails(times);
                    this.dataGridView1.DataSource = times;
                    this.Refresh();
                }
            }
            catch
            {

            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            var times = getTimeFrameDetails();
            var currentTime = DateTime.Now;
            checkPrevDate();

            if(executeTime=="")
                button6.Text = "Reposting Initiated: " + currentTime.ToLongTimeString();
            else
                button6.Text = "Reposting Initiated: " + currentTime.ToLongTimeString() + " - Random Time: " + executeTime;

            if (times.Count!= 0)
             {
                if (executeTime=="" && frames1.Any(x => currentTime.TimeOfDay >= getDateTime(x.StartTime).TimeOfDay && currentTime.TimeOfDay <= getDateTime(x.EndTime).TimeOfDay && (x.Days.Contains(currentTime.DayOfWeek.ToString()) || x.Days.Contains("Every Day")) && x.Status!="Completed For Today"))
                {
                    var index = frames1.IndexOf(frames1.Where(x => currentTime.TimeOfDay >= getDateTime(x.StartTime).TimeOfDay && currentTime.TimeOfDay <= getDateTime(x.EndTime).TimeOfDay).FirstOrDefault());
                    selectedTimeFrameIndex = index;
                    
                    Random random = new Random();
                    var limit = Convert.ToInt32((getDateTime(frames1[selectedTimeFrameIndex].EndTime).TimeOfDay - currentTime.TimeOfDay).TotalMinutes);
                    
                    if(limit!=0)
                    {
                        int randomNumber = random.Next(1, limit);
                        currentTime = currentTime.AddMinutes(1);
                    }
                    executeTime = currentTime.ToShortTimeString().Trim();
                }
                else if (currentTime.ToShortTimeString().Trim() == executeTime)
                {
                   executeProcess();
                   executeTime = "";
                   frames1[selectedTimeFrameIndex].Status = "Completed For Today";
                   saveTimeFrameDetails(frames1);
                   this.dataGridView1.DataSource = frames1;
                   this.Refresh();
                   selectedTimeFrameIndex = -1;
                }
            }
        }
        ///html/body/article/section/div[1]/table/tbody/tr[1]/td/b - renew
        private void executeProcess()
        {
            try
            {
                IList<IWebElement> elements = driver.FindElements(By.ClassName("managebtn")).Where(x => x.GetAttribute("value") == "edit").ToList();
                int count = 0;

                while (elements.Count != 0 && count!=elements.Count)
                {
                    IWebElement s7dksjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("managebtn")));
                    elements = driver.FindElements(By.ClassName("managebtn")).Where(x => x.GetAttribute("value") == "edit").ToList();
                    elements[count].Click();

                    IWebElement sdksjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("/html/body/article/section/div[1]/form/button")));
                    sdksjl.Click();

                    IWebElement sdk1sjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("/html/body/article/section/form/div/div/div/ul/li[4]/a")));
                    sdk1sjl.Click();

                    count++;
                }
            }
            catch(Exception ex)
            {
                driver.Url = "https://accounts.craigslist.org/login";
            }
        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(pause==1)
            {
                frames1 = getTimeFrameDetails();
                timer1.Start();
                pause = 0;
                panel2.Enabled = false;
                panel7.Enabled = false;
                //panel5.Enabled = false;
                if (String.IsNullOrWhiteSpace(File.ReadAllText(currentDayPath)))
                    File.WriteAllText(currentDayPath,DateTime.Now.ToShortDateString());
            }
            else if(pause==0 && selectedTimeFrameIndex==-1)
            {
                timer1.Stop();
                pause = 1;
                button6.Text = "Reposting Paused";
                panel2.Enabled = true;
                panel7.Enabled = true;
                //panel5.Enabled = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
           
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (driver != null)
            {
                driver.Quit();
            }

            foreach (var process in Process.GetProcessesByName("chromedriver"))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {

                }

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (driver != null)
            {
                driver.Quit();
            }

            foreach (var process in Process.GetProcessesByName("chromedriver"))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {

                }

            }

            System.Environment.Exit(0);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            dayStatuses[3].Status = checkThursday.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dayStatuses[0].Status = checkMonday.Checked;
        }

        private void checkTuesday_CheckedChanged(object sender, EventArgs e)
        {
            dayStatuses[1].Status = checkTuesday.Checked;
        }

        private void checkWednesday_CheckedChanged(object sender, EventArgs e)
        {
            dayStatuses[2].Status = checkWednesday.Checked;
        }

        private void checkFriday_CheckedChanged(object sender, EventArgs e)
        {
            dayStatuses[4].Status = checkFriday.Checked;
        }

        private void checkSaturday_CheckedChanged(object sender, EventArgs e)
        {
            dayStatuses[5].Status = checkSaturday.Checked;
        }

        private void checkSunday_CheckedChanged(object sender, EventArgs e)
        {
            dayStatuses[6].Status = checkSunday.Checked;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            checkPrevDate();
        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to revert the status of every timeframe to Incomplete?", "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                var frames = getTimeFrameDetails();

                for (int i = 0; i < frames.Count; i++)
                {
                    frames[i].Status = "Incomplete";
                }

                File.WriteAllText(timeFramesPath, JsonConvert.SerializeObject(frames));
                this.dataGridView1.DataSource = getTimeFrameDetails();
                this.Refresh();
            }
        }
    }
}