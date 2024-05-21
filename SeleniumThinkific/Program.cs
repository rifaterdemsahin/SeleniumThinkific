using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using SeleniumExtras.WaitHelpers;

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

            // Retrieve email, password, and browser from configuration
            string email = configuration["Login:Email"];
            string password = configuration["Login:Password"];
            string browser = configuration["Browser"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Logger.Log("Email or password are not set in the configuration file.");
                return;
            }

            IWebDriver driver = null;
            try
            {
                // Initialize the WebDriver based on the specified browser
                driver = InitializeWebDriver(browser, configuration);
                if (driver == null)
                {
                    Logger.Log("Unsupported browser specified in the configuration file.");
                    return;
                }

                driver.Manage().Window.Size = new System.Drawing.Size(1273, 672);

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
                driver?.Quit();
            }

            Logger.Log("Program ended!");
        }

        static IWebDriver InitializeWebDriver(string browser, IConfigurationRoot configuration)
        {
            if (browser.Equals("Chrome", StringComparison.OrdinalIgnoreCase))
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArgument($"user-data-dir={configuration["Chrome:UserDataDir"]}");
                options.AddArgument($"profile-directory={configuration["Chrome:ProfileDirectory"]}");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");

                var service = ChromeDriverService.CreateDefaultService();
                return new ChromeDriver(service, options, TimeSpan.FromMinutes(3));
            }
            else if (browser.Equals("Firefox", StringComparison.OrdinalIgnoreCase))
            {
                FirefoxOptions options = new FirefoxOptions();
                options.AddArgument($"-profile");
                options.AddArgument(configuration["Firefox:ProfilePath"]);
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");

                var service = FirefoxDriverService.CreateDefaultService();
                return new FirefoxDriver(service, options, TimeSpan.FromMinutes(3));
            }

            return null;
        }

        static void PerformLogin(IWebDriver driver, string email, string password)
        {
            try
            {
                Logger.Log("Performing login...");
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                // Enter email and password, and click the sign-in button
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("user[email]"))).SendKeys(email);
                driver.FindElement(By.Id("user[password]")).SendKeys(password);
                driver.FindElement(By.CssSelector(".button-primary")).Click();

                // Wait and re-enter email and password if needed
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("user[email]"))).SendKeys(email);
                driver.FindElement(By.Id("user[password]")).SendKeys(password);
                wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".button-primary"))).Click();

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
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var sections = wait.Until(d => d.FindElements(By.CssSelector("h4[data-qa='accordion-title']")));

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
                {
                    if (!CheckSectionExists(driver, section.Name))
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
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Click on the "Add chapter" button
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[data-qa='add-chapter__btn']"))).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@data-qa='chapter-name__input']"))).SendKeys(section.Name);

            // Input section details
            IWebElement sectionInputField = driver.FindElement(By.XPath("//input[@data-qa='chapter-name__input']"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // Clear the field using JavaScript
            js.ExecuteScript("arguments[0].value='';", sectionInputField);

            // Add a small delay
            Thread.Sleep(100);

            // Ensure the element is properly focused
            sectionInputField.Click();
            sectionInputField.SendKeys(Keys.Control + "a");
            sectionInputField.SendKeys(Keys.Delete);

            // Send new keys
            sectionInputField.SendKeys(section.Name);

            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[data-qa='actions-bar__save-button']"))).Click();
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

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("label[for='lesson-draft-status']"))).Click();

            // Input video details
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[data-qa='lesson-form__name']"))).SendKeys(video.Name);
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@data-qa='actions-bar__save-button']"))).Click();
            Thread.Sleep(5000);

            try
            {
                // Handle the toast message if it appears
                IWebElement toastMessage = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div[class^='Toast_toast__message_']")));
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
