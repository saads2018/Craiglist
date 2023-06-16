using Newtonsoft.Json;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SnapchatBot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Threading;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager;

namespace CraiglistBot
{
    public partial class Form2 : Form
    {
        int pos = 0;
        BackgroundWorker worker;
        IWebDriver driver;
        WebDriverWait wait;
        string loginPath;
        string timeFramesPath;
        string executeTime;
        string currentDayPath;
        string selectedDay;
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
        public Form2()
        {
            InitializeComponent();
            createNecessaryFiles();
            initiateFormDetails();
            initBackgroundWorker();
        }

        private void initBackgroundWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted; ;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Visible = false;
            MessageBox.Show("The chromedriver has been updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\Chrome"))
                Directory.Delete(Directory.GetCurrentDirectory() + "\\Chrome", true);

            new DriverManager().SetUpDriver(new ChromeConfig());
            Task.Delay(1000);

            var chromeDirectoryFirst = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\Chrome");
            var chromeDirectorySecond = Directory.GetDirectories(chromeDirectoryFirst.FirstOrDefault());

            File.Copy(chromeDirectorySecond[0] + "\\chromedriver.exe", "chromedriver.exe", true);
        }

        private void initiateFormDetails()
        {
            executeTime = "";
            selectedDay = DateTime.Now.DayOfWeek.ToString();

            if(selectedDay=="Monday")
            {
                label7.ForeColor = Color.Black;
            }
            else if (selectedDay == "Tuesday")
            {
                label8.ForeColor = Color.Black;
            }
            else if (selectedDay == "Wednesday")
            {
                label9.ForeColor = Color.Black;
            }
            else if (selectedDay == "Thursday")
            {
                label10.ForeColor = Color.Black;
            }
            else if (selectedDay == "Friday")
            {
                label11.ForeColor = Color.Black;
            }
            else if (selectedDay == "Saturday")
            {
                label12.ForeColor = Color.Black;
            }
            else if (selectedDay == "Sunday")
            {
                label13.ForeColor = Color.Black;
            }

            string data = File.ReadAllText(loginPath);
            Login login = JsonConvert.DeserializeObject<Login>(data);
            loadList();

            if (login != null)
            {
                textBox1.Text = login.UserName;
                textBox2.Text = login.Password;
            }

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
                    File.WriteAllText(currentDayPath, DateTime.Now.ToShortDateString());
                    loadList();
                }
            }
        }

        public static ChromeOptions GetChromeOptionsMin()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalChromeOption("useAutomationExtension", false);
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

        private void saveLoginDetails()
        {
            try
            {
                IWebElement appear = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("account-homepage")));
                Login loginDetails = new Login();
                loginDetails.UserName = textBox1.Text;
                loginDetails.Password = textBox2.Text;
                label19.Enabled = false;
                button8.Enabled = true;

                string json = JsonConvert.SerializeObject(loginDetails);
                File.WriteAllText(loginPath, json);
            }
            catch
            {
                label19.Enabled = true;
                button8.Enabled = false;
            }
        }

        private void saveLoginDetails_Directly()
        {
            if(String.IsNullOrWhiteSpace(textBox1.Text) || String.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please Enter The Username And Password Before Proceeding!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                var login = JsonConvert.DeserializeObject<Login>(File.ReadAllText(loginPath));
                if(login!=null)
                {
                    textBox1.Text = login.UserName;
                    textBox2.Text = login.Password;
                }
            }
            else
            {
                try
                {
                    Login loginDetails = new Login();
                    loginDetails.UserName = textBox1.Text;
                    loginDetails.Password = textBox2.Text;

                    string json = JsonConvert.SerializeObject(loginDetails);
                    File.WriteAllText(loginPath, json);
                    MessageBox.Show("The Login Details Have Been Saved Successfully!","Success",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {

                }
            }
        }
        private void createNecessaryFiles()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "craigListLoginBotFiles");
            if (!File.Exists(path))
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

        private void stopProcesses()
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
        private void button2_Click(object sender, EventArgs e)
        {
            stopProcesses();

            try
            {
                System.Environment.Exit(0);
            }
            catch
            {
                System.Environment.Exit(0);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            switchTo("Login");
        }

        private void switchTo(string switchTo)
        {
            if(switchTo == "Login")
            {
                panel4.Visible = true;
                panel5.Visible = false;
            }
            else if (switchTo == "TimeFrames")
            {
                panel4.Visible = false;
                panel5.Visible = true;
            }

        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            switchTo("Login");
        }

        private void label1_Click(object sender, EventArgs e)
        {
            switchTo("Login");
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            switchTo("TimeFrames");
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            switchTo("TimeFrames");
        }

        private void label2_Click(object sender, EventArgs e)
        {
            switchTo("TimeFrames");
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            panel5.Visible = false;
        }

        private void gradientPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void gradientPanel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to revert the status of every timeframe of the selected day to Incomplete?", "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                var frames = getTimeFrameDetails();

                for (int i = 0; i < frames.Count; i++)
                {
                    if(frames[i].Days.Contains(selectedDay))
                        frames[i].Status = "Incomplete";
                }

                File.WriteAllText(timeFramesPath, JsonConvert.SerializeObject(frames));
                loadList();
            }
        }


        private void createTimeFrame(int count, int number, string startTime,string endTime,string status)
        {
            GradientPanel gradientPanel = new GradientPanel();
            Label label_Status = new Label();
            Label label_EndTime = new Label();
            Label label_StartTime = new Label();
            Label label_No = new Label();
            Button button_Delete = new Button();

            this.panel14.Controls.Add(gradientPanel);

            gradientPanel.Angle = 0F;
            gradientPanel.BottomColor = System.Drawing.Color.White;
            gradientPanel.Controls.Add(button_Delete);
            gradientPanel.Controls.Add(label_Status);
            gradientPanel.Controls.Add(label_EndTime);
            gradientPanel.Controls.Add(label_StartTime);
            gradientPanel.Controls.Add(label_No);
            gradientPanel.Location = new System.Drawing.Point(0, pos);
            gradientPanel.Name = "gradientPanel8";
            gradientPanel.Size = new System.Drawing.Size(920, 57);
            gradientPanel.TabIndex = 48;
            gradientPanel.TopColor = System.Drawing.Color.White;

            button_Delete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(112)))), ((int)(((byte)(51)))), ((int)(((byte)(255)))));
            button_Delete.Font = new System.Drawing.Font("Microsoft YaHei UI Light", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            button_Delete.ForeColor = System.Drawing.Color.White;
            button_Delete.Location = new System.Drawing.Point(740, 16);
            button_Delete.Name = number.ToString();
            button_Delete.Size = new System.Drawing.Size(140, 28);
            button_Delete.TabIndex = 47;
            button_Delete.Text = "Delete";
            button_Delete.UseVisualStyleBackColor = false;
            button_Delete.Click += Button_Delete_Click;

            label_Status.AutoSize = true;
            label_Status.BackColor = System.Drawing.Color.Transparent;
            label_Status.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label_Status.ForeColor = System.Drawing.Color.Black;
            label_Status.Location = new System.Drawing.Point(583, 19);
            label_Status.Name = "label19";
            label_Status.Size = new System.Drawing.Size(168, 20);
            label_Status.TabIndex = 33;
            label_Status.Text = status;
            // 
            // label20
            // 
            label_EndTime.AutoSize = true;
            label_EndTime.BackColor = System.Drawing.Color.Transparent;
            label_EndTime.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label_EndTime.ForeColor = System.Drawing.Color.Black;
            label_EndTime.Location = new System.Drawing.Point(397, 19);
            label_EndTime.Name = "label20";
            label_EndTime.Size = new System.Drawing.Size(70, 20);
            label_EndTime.TabIndex = 32;
            label_EndTime.Text = endTime;
            // 
            // label21/
            // 
            label_StartTime.AutoSize = true;
            label_StartTime.BackColor = System.Drawing.Color.Transparent;
            label_StartTime.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label_StartTime.ForeColor = System.Drawing.Color.Black;
            label_StartTime.Location = new System.Drawing.Point(211, 19);
            label_StartTime.Name = "label21";
            label_StartTime.Size = new System.Drawing.Size(70, 20);
            label_StartTime.TabIndex = 31;
            label_StartTime.Text =  startTime;
            // 
            // label22
            // 
            label_No.AutoSize = true;
            label_No.BackColor = System.Drawing.Color.Transparent;
            label_No.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label_No.ForeColor = System.Drawing.Color.Black;
            label_No.Location = new System.Drawing.Point(66, 19);
            label_No.Name = "label22";
            label_No.Size = new System.Drawing.Size(18, 20);
            label_No.TabIndex = 30;
            label_No.Text = count.ToString();

            pos += 58;
        }

        private void Button_Delete_Click(object sender, EventArgs e)
        {
            try
            {
                var btn = (Button)sender;
                int index = Int32.Parse(btn.Name) - 1;
                var times = getTimeFrameDetails();
                times.RemoveAt(index);

                for (int i = 0; i < times.Count; i++)
                    times[i].No = i + 1;

                saveTimeFrameDetails(times);
                loadList();
            }
            catch
            {

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

        private void button6_Click(object sender, EventArgs e)
        {
                List<TimeFrame> frames = getTimeFrameDetails();
                DateTime timePicked = this.dateTimePicker1.Value;
                timePicked = timePicked.AddMinutes(30);
                string days = selectedDay;
                List<string> daysList = new List<string>(); 


                if (frames.Count > 0 && (frames.Any(x => this.dateTimePicker1.Value.TimeOfDay >= getDateTime(x.StartTime).TimeOfDay && this.dateTimePicker1.Value.TimeOfDay <= getDateTime(x.EndTime).TimeOfDay &&  x.Days.Contains(selectedDay)) || frames.Any(x => timePicked.TimeOfDay >= getDateTime(x.StartTime).TimeOfDay && timePicked.TimeOfDay <= getDateTime(x.EndTime).TimeOfDay && x.Days.Contains(selectedDay))))
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
                    loadList();                    
                }
        }

        private void loadList()
        {
            this.panel14.Controls.Clear();
            pos = 0;
            var count = 1;

            foreach (var frame in getTimeFrameDetails())
            {
                if (frame.Days.Contains(selectedDay))
                {
                    createTimeFrame(count, frame.No, frame.StartTime, frame.EndTime, frame.Status);
                    count += 1;
                }
            }
            /*this.Refresh();
            panel4.Visible = false;
            panel5.Visible = true;*/
        }
        private DateTime getDateTime(string time)
        {
            return DateTime.ParseExact(time, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None);
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

        private void gradientPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {
            selectedDay = "Monday";
            label7.ForeColor = Color.Black;
            label8.ForeColor = Color.FromArgb(179, 180, 190);
            label9.ForeColor = Color.FromArgb(179, 180, 190);
            label10.ForeColor = Color.FromArgb(179, 180, 190);
            label11.ForeColor = Color.FromArgb(179, 180, 190);
            label12.ForeColor = Color.FromArgb(179, 180, 190);
            label13.ForeColor = Color.FromArgb(179, 180, 190);
            loadList();
        }

        private void label8_Click(object sender, EventArgs e)
        {
            selectedDay = "Tuesday";
            label8.ForeColor = Color.Black;
            label7.ForeColor = Color.FromArgb(179, 180, 190);
            label9.ForeColor = Color.FromArgb(179, 180, 190);
            label10.ForeColor = Color.FromArgb(179, 180, 190);
            label11.ForeColor = Color.FromArgb(179, 180, 190);
            label12.ForeColor = Color.FromArgb(179, 180, 190);
            label13.ForeColor = Color.FromArgb(179, 180, 190);
            loadList();
        }

        private void label9_Click(object sender, EventArgs e)
        {
            selectedDay = "Wednesday";
            label9.ForeColor = Color.Black;
            label8.ForeColor = Color.FromArgb(179, 180, 190);
            label7.ForeColor = Color.FromArgb(179, 180, 190);
            label10.ForeColor = Color.FromArgb(179, 180, 190);
            label11.ForeColor = Color.FromArgb(179, 180, 190);
            label12.ForeColor = Color.FromArgb(179, 180, 190);
            label13.ForeColor = Color.FromArgb(179, 180, 190);
            loadList();
        }

        private void label10_Click(object sender, EventArgs e)
        {
            selectedDay = "Thursday";
            label10.ForeColor = Color.Black;
            label8.ForeColor = Color.FromArgb(179, 180, 190);
            label9.ForeColor = Color.FromArgb(179, 180, 190);
            label7.ForeColor = Color.FromArgb(179, 180, 190);
            label11.ForeColor = Color.FromArgb(179, 180, 190);
            label12.ForeColor = Color.FromArgb(179, 180, 190);
            label13.ForeColor = Color.FromArgb(179, 180, 190);
            loadList();
        }

        private void label11_Click(object sender, EventArgs e)
        {
            selectedDay = "Friday";
            label11.ForeColor = Color.Black;
            label8.ForeColor = Color.FromArgb(179, 180, 190);
            label9.ForeColor = Color.FromArgb(179, 180, 190);
            label10.ForeColor = Color.FromArgb(179, 180, 190);
            label7.ForeColor = Color.FromArgb(179, 180, 190);
            label12.ForeColor = Color.FromArgb(179, 180, 190);
            label13.ForeColor = Color.FromArgb(179, 180, 190);
            loadList();
        }

        private void label12_Click(object sender, EventArgs e)
        {
            selectedDay = "Saturday";
            label12.ForeColor = Color.Black;
            label8.ForeColor = Color.FromArgb(179, 180, 190);
            label9.ForeColor = Color.FromArgb(179, 180, 190);
            label10.ForeColor = Color.FromArgb(179, 180, 190);
            label11.ForeColor = Color.FromArgb(179, 180, 190);
            label7.ForeColor = Color.FromArgb(179, 180, 190);
            label13.ForeColor = Color.FromArgb(179, 180, 190);
            loadList();
        }

        private void label13_Click(object sender, EventArgs e)
        {
            selectedDay = "Sunday";
            label13.ForeColor = Color.Black;
            label8.ForeColor = Color.FromArgb(179, 180, 190);
            label9.ForeColor = Color.FromArgb(179, 180, 190);
            label10.ForeColor = Color.FromArgb(179, 180, 190);
            label11.ForeColor = Color.FromArgb(179, 180, 190);
            label12.ForeColor = Color.FromArgb(179, 180, 190);
            label7.ForeColor = Color.FromArgb(179, 180, 190);
            loadList();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            saveLoginDetails_Directly();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            checkPrevDate();
            MessageBox.Show("The date has been updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void panel15_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text=="II")
            {
                var result = MessageBox.Show("Are You Sure You Want To Close The Chrome Process?", "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if(result==DialogResult.Yes)
                {
                    button1.Text = " ▷|";
                    var chromeDriverService = ChromeDriverService.CreateDefaultService();
                    chromeDriverService.HideCommandPromptWindow = true;
                    timer1.Stop();
                    button8.Enabled = false;
                    button4.Enabled = true;
                    button8.Text = "Initiate Reposting";
                    executeTime = "";
                    gradientPanel4.Enabled = true;
                    pause = 0;
                    selectedTimeFrameIndex = -1;
                    stopProcesses();
                }
            }
            else if (String.IsNullOrWhiteSpace(textBox1.Text) || String.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please Enter The Username And Password Before Proceeding!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var login = JsonConvert.DeserializeObject<Login>(File.ReadAllText(loginPath));
                if (login != null)
                {
                    textBox1.Text = login.UserName;
                    textBox2.Text = login.Password;
                }
            }
            else if (getTimeFrameDetails().Count > 0)
            {
                
                try
                {
                    var chromeDriverService = ChromeDriverService.CreateDefaultService();
                    chromeDriverService.HideCommandPromptWindow = true;

                    if (driver != null)
                    {
                        driver.Quit();
                    }

                    driver = new ChromeDriver(chromeDriverService, GetChromeOptionsMin());
                    wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                }
                catch (Exception ex)
                {
                    if(ex.Message.ToLower().Contains("this version of chromedriver only supports"))
                    {
                        MessageBox.Show("Please make sure that the chromedriver and chrome browser versions are up to date!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    }
                    stopProcesses();
                    return;
                }

                button1.Text = "II";
                button4.Enabled = false;
                timer1.Stop();
                button8.Enabled = false;
                button8.Text = "Initiate Reposting";
                gradientPanel4.Enabled = true;
                pause = 1;

                //driver = new ChromeDriver();
                try
                {
                    driver.Url = "https://accounts.craigslist.org/login";
                    Thread.Sleep(2500);
                    Activate();
                    enterLoginDetails();
                }
                catch (Exception ex)
                {

                }
            }
            else if (getTimeFrameDetails().Count == 0)
            {
                MessageBox.Show("Please Create At Least One Time Frame Before Starting The App!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var times = getTimeFrameDetails();
            var currentTime = DateTime.Now;
            checkPrevDate();

            if (executeTime == "")
                button8.Text = "Reposting Initiated: " + currentTime.ToLongTimeString();
            else
                button8.Text = "Reposting Initiated: " + currentTime.ToLongTimeString() + " - Random Time: " + executeTime;

            if (times.Count != 0)
            {
                if (executeTime == "" && frames1.Any(x => currentTime.TimeOfDay >= getDateTime(x.StartTime).TimeOfDay && currentTime.TimeOfDay <= getDateTime(x.EndTime).TimeOfDay && x.Days.Contains(currentTime.DayOfWeek.ToString()) && x.Status != "Completed"))
                {
                    var index = frames1.IndexOf(frames1.Where(x => currentTime.TimeOfDay >= getDateTime(x.StartTime).TimeOfDay && currentTime.TimeOfDay <= getDateTime(x.EndTime).TimeOfDay && x.Days.Contains(currentTime.DayOfWeek.ToString()) && x.Status != "Completed").FirstOrDefault());
                    selectedTimeFrameIndex = index;

                    Random random = new Random();
                    var limit = Convert.ToInt32((getDateTime(frames1[selectedTimeFrameIndex].EndTime).TimeOfDay - currentTime.TimeOfDay).TotalMinutes);

                    if (limit != 0)
                    {
                        int randomNumber = random.Next(1, limit);
                        currentTime = currentTime.AddMinutes(randomNumber);
                    }
                    executeTime = currentTime.ToShortTimeString().Trim();
                }
                else if (currentTime.ToShortTimeString().Trim() == executeTime)
                {
                    executeProcess();
                    executeTime = "";
                    frames1[selectedTimeFrameIndex].Status = "Completed";
                    saveTimeFrameDetails(frames1);
                    loadList();
                    selectedTimeFrameIndex = -1;
                }
            }
        }

        private void executeProcess()
        {
            try
            {
                IList<IWebElement> elements = driver.FindElements(By.ClassName("managebtn")).Where(x => x.GetAttribute("value") == "repost").ToList();
                int count = 0;
                string storedUrl = driver.Url;

                while (count<elements.Count)
                {
                    IWebElement s7dksjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("managebtn")));
                    //elements = driver.FindElements(By.ClassName("managebtn")).Where(x => x.GetAttribute("value") == "repost").ToList();

                    elements[count].Click();

                    IWebElement sdksjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("/html/body/article/section/div[1]/div[1]/input[3]")));
                    sdksjl.Click();
                    
                    IWebElement sdk1sjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("/html/body/article/section/form/div/div/div[4]/div[1]/button")));
                    sdk1sjl.Click();

                    IWebElement sdk1ssjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("/html/body/article/section/div[1]/form/button")));
                    sdk1ssjl.Click();

                    IWebElement sdk231ssjl = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("/html/body/article/section/form/div/div/div[2]/div[1]/button")));
                    sdk231ssjl.Click();

                    driver.Url = storedUrl;

                    count++;
                }
            }
            catch (Exception ex)
            {
                driver.Url = "https://accounts.craigslist.org/login";
            }
        }

        private void label19_Click(object sender, EventArgs e)
        {
            enterLoginDetails();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (getTimeFrameDetails().Count == 0)
            {
                MessageBox.Show("Please Create At Least One Time Frame Before Starting The App!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (pause == 1)
            {
                frames1 = getTimeFrameDetails();
                timer1.Start();
                pause = 0;
                gradientPanel4.Enabled = false;
                if (String.IsNullOrWhiteSpace(File.ReadAllText(currentDayPath)))
                    File.WriteAllText(currentDayPath, DateTime.Now.ToShortDateString());
            }
            else if (pause == 0 && selectedTimeFrameIndex == -1)
            {
                timer1.Stop();
                pause = 1;
                button8.Text = "Reposting Paused";
                gradientPanel4.Enabled = true;
            }
        }



        private void button9_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;
            worker.RunWorkerAsync();
        }
    }
}
