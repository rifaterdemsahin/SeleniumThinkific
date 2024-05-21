using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;

namespace SeleniumCourseLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.InitLogFile();
            Logger.Log("Program started!");

            // Build configuration
            var builder = new ConfigurationBuilder()
                .AddJsonFile("F:\\source\\SeleniumThinkific\\SeleniumThinkific\\appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();

            // Retrieve email and password from configuration
            string email = configuration["Login:Email"];
            string password = configuration["Login:Password"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Logger.Log("Email or password are not set in the configuration file.");
                return;
            }

            //var chrome_driver_path = "C:\Program Files\Google\Chrome\Application\chrome.exe";

            // Initialize the Chrome options
            ChromeOptions options = new ChromeOptions();
            options.AddArgument(@"user-data-dir=C:\Users\Pexabo\AppData\Local\Google\Chrome\User Data"); // Replace with your Chrome user data path
            options.AddArgument(@"profile-directory=Profile 15"); // Replace with your profile directory
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu")
                
                
                ;

            //options.AddArgument("--headless");

            // Initialize the ChromeDriver with a command timeout
            var service = ChromeDriverService.CreateDefaultService();
            var driver = new ChromeDriver(service, options, TimeSpan.FromMinutes(3)); // Set a 3-minute timeout for commands
            driver.Manage().Window.Size = new System.Drawing.Size(1273, 672);

            try
            {
                // Navigate to login page
                Logger.Log("Navigating to login page...");
                driver.Navigate().GoToUrl("https://courses.devops.engineering/users/sign_in");

                // Perform login
                PerformLogin(driver, email, password);

                // Process courses
                ProcessCourses(driver);
            }
            catch (Exception ex)
            {
                Logger.Log($"Test failed: {ex.Message}");
            }
            finally
            {
                // Close the browser
                driver.Quit();
            }

            Logger.Log("Program ended!");
        }

        static void PerformLogin(IWebDriver driver, string email, string password)
        {
            try
            {
                Logger.Log("Performing login...");
                // Enter email and password, and click the sign-in button
                driver.FindElement(By.Id("user[email]")).SendKeys(email);
                driver.FindElement(By.Id("user[password]")).SendKeys(password);
                driver.FindElement(By.CssSelector(".button-primary")).Click();

                // Wait and re-enter email and password if needed
                Thread.Sleep(5000);
                driver.FindElement(By.Id("user[email]")).SendKeys(email);
                driver.FindElement(By.Id("user[password]")).SendKeys(password);
                Thread.Sleep(5000);
                driver.FindElement(By.CssSelector(".button-primary")).Click();
                Logger.Log("Login performed successfully.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Login failed: {ex.Message}");
            }
        }
        static bool CheckSectionExists(IWebDriver driver, string sectionTitle)
        {
            try
            {
                // Locate the section by its title
                var sections = driver.FindElements(By.CssSelector("h4[data-qa='accordion-title']"));

                foreach (var section in sections)
                {
                    if (section.Text.Contains(sectionTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking for the section: {ex.Message}");
                return false;
            }
        }

        static void ProcessCourses(IWebDriver driver)
        {
            Logger.Log("Processing courses...");
            // Get the course data
            var folderPath = @"F:\source\SeleniumThinkific\SeleniumThinkific\data\";
            var courses = CourseData.GetCourses(folderPath);

            // Loop through each course
            foreach (var course in courses)
            {
                Logger.Log($"Processing course: {course.MainUrl}");
                driver.Navigate().GoToUrl(course.MainUrl);
                Thread.Sleep(5000);

                // Loop through the sections and videos
                foreach (var section in course.Sections)
                { if (CheckSectionExists(driver,section.Name) == false)
                    {
                        AddChapter(driver, section);

                        // Loop through the videos in each section
                        foreach (var video in section.Videos)
                        {
                            AddVideo(driver, course.MainUrl, section, video);
                        }
                    }

                }
            }

            Logger.Log("Courses processed successfully.");
        }

        static void AddChapter(IWebDriver driver, Section section)
        {
            Logger.Log($"Adding chapter: {section.Name}");
            // Click on the "Add chapter" button
            driver.FindElement(By.CssSelector("button[data-qa='add-chapter__btn']")).Click();
            Thread.Sleep(1000);

            // Input section details
            IWebElement sectionInputField = driver.FindElement(By.XPath("//input[@data-qa='chapter-name__input']"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].value='';", sectionInputField);
            Thread.Sleep(500);

            sectionInputField.Clear();
            sectionInputField.SendKeys(section.Name);

            IWebElement saveButton = driver.FindElement(By.CssSelector("button[data-qa='actions-bar__save-button']"));
            saveButton.Click();
            Thread.Sleep(5000);

            string url = driver.Url;
            string[] parts = url.Split('/');
            string chapterId = parts[parts.Length - 2]; // Second last part is the chapter ID

            Logger.Log($"Chapter ID: {chapterId}"); // Output the chapter ID
            Thread.Sleep(5000);

            section.ChapterId = chapterId; // Store the chapter ID for later use
        }

        static void AddVideo(IWebDriver driver, string mainUrl, Section section, Video video)
        {
            Logger.Log($"Adding video: {video.Name} to section: {section.Name}");
            driver.Navigate().GoToUrl(mainUrl + "/chapters/" + section.ChapterId + "/contents/new_video_lesson");
            Thread.Sleep(7000);

            // Locate and click the label to toggle the draft checkbox
            IWebElement labelForCheckbox = driver.FindElement(By.CssSelector("label[for='lesson-draft-status']"));
            labelForCheckbox.Click();
            Thread.Sleep(500);

            // Input video details
            IWebElement videoInputField = driver.FindElement(By.CssSelector("input[data-qa='lesson-form__name']"));
            videoInputField.Clear();
            videoInputField.SendKeys(video.Name);

            IWebElement saveVideoButton = driver.FindElement(By.XPath("//button[@data-qa='actions-bar__save-button']"));
            saveVideoButton.Click();
            Thread.Sleep(5000);

            try
            {
                // Handle the toast message if it appears
                IWebElement toastMessage = driver.FindElement(By.CssSelector("div[class^='Toast_toast__message_']"));
                toastMessage.Click();
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error handling toast message: {ex.Message}");
            }
        }
    }

    public static class Logger
    {
        private static string logFilePath = "logfile.txt";

        public static void InitLogFile()
        {
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            using (StreamWriter sw = File.CreateText(logFilePath))
            {
                sw.WriteLine($"Log file created on {DateTime.Now}");
            }
        }

        public static void Log(string message)
        {
            string logMessage = $"{DateTime.Now}: {message}";
            Console.WriteLine(logMessage);
            using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine(logMessage);
            }
        }
    }
}
