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
            Console.WriteLine("Program started!");

            // Build configuration
            var builder = new ConfigurationBuilder()
                .AddJsonFile("F:\\source\\SeleniumThinkific\\SeleniumThinkific\\appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();

            // Retrieve email and password from configuration
            string email = configuration["Login:Email"];
            string password = configuration["Login:Password"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Email or password are not set in the configuration file.");
                return;
            }

            // Initialize the Chrome options
            ChromeOptions options = new ChromeOptions();
            options.AddArgument(@"user-data-dir=C:\Users\Pexabo\AppData\Local\Google\Chrome\User Data"); // Replace with your Chrome user data path
            options.AddArgument(@"profile-directory=Profile 15"); // Replace with your profile directory

            // Initialize the ChromeDriver
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Window.Size = new System.Drawing.Size(1273, 672);

            try
            {
                // Navigate to login page
                driver.Navigate().GoToUrl("https://courses.devops.engineering/users/sign_in");

                // Perform login
                PerformLogin(driver, email, password);

                // Process courses
                ProcessCourses(driver);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed: {ex.Message}");
            }
            finally
            {
                // Close the browser
                driver.Quit();
            }

            Console.WriteLine("Program ended!");
        }

        static void PerformLogin(IWebDriver driver, string email, string password)
        {
            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ProcessCourses(IWebDriver driver)
        {
            // Get the course data
            var folderPath = @"F:\source\SeleniumThinkific\SeleniumThinkific\data\";
            var courses = CourseData.GetCourses(folderPath);

            // Loop through each course
            foreach (var course in courses)
            {
                Console.WriteLine(course.MainUrl);
                driver.Navigate().GoToUrl(course.MainUrl);
                Thread.Sleep(5000);

                // Loop through the sections and videos
                foreach (var section in course.Sections)
                {
                    AddChapter(driver, section);

                    // Loop through the videos in each section
                    foreach (var video in section.Videos)
                    {
                        AddVideo(driver, course.MainUrl, section, video);
                    }
                }
            }

            Console.WriteLine("Test completed successfully!");
        }

        static void AddChapter(IWebDriver driver, Section section)
        {
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

            Console.WriteLine("Chapter ID: " + chapterId); // Output the chapter ID
            Thread.Sleep(5000);

            section.ChapterId = chapterId; // Store the chapter ID for later use
        }

        static void AddVideo(IWebDriver driver, string mainUrl, Section section, Video video)
        {
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
                Console.WriteLine(ex.Message);
            }
        }
    }
}
