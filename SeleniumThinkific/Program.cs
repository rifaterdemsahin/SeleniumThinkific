using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

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

            // Initialize the Chrome options chrome://version/
            ChromeOptions options = new ChromeOptions();
            options.AddArgument(@"user-data-dir=C:\Users\Pexabo\AppData\Local\Google\Chrome\User Data"); // Replace with your Chrome user data path
            options.AddArgument(@"profile-directory=Profile 15"); // Replace with your profile directory


            // Initialize the ChromeDriver
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Window.Size = new System.Drawing.Size(1273, 672);

            driver.Navigate().GoToUrl("https://courses.devops.engineering/users/sign_in");

            // Set the window size
            driver.Manage().Window.Size = new System.Drawing.Size(1936, 1048);

            try
            {
                // Enter email
                driver.FindElement(By.Id("user[email]")).SendKeys(email);

                // Enter password
                driver.FindElement(By.Id("user[password]")).SendKeys(password);

                // Click the Sign In button
                driver.FindElement(By.CssSelector(".button-primary")).Click();

                // Wait for the login to complete 
                //thinkific works in 2 presses lol

                Thread.Sleep(5000);
                // Enter email
                driver.FindElement(By.Id("user[email]")).SendKeys(email);

                // Enter password
                driver.FindElement(By.Id("user[password]")).SendKeys(password);
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);

            }


            // Click the Sign In button
            try
            {
                driver.FindElement(By.CssSelector(".button-primary")).Click();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

            try
            {
                // Get the course data
                var folderPath = @"F:\source\SeleniumThinkific\SeleniumThinkific\data\";
                var courses = CourseData.GetCourses(folderPath);
                // Loop through each course
                foreach (var course in courses)
                {
                Console.WriteLine(course.MainUrl);
                    // Open the URL
                    driver.Navigate().GoToUrl(course.MainUrl);


                    Thread.Sleep(5000);

                    // Loop through the sections and videos
                    foreach (var section in course.Sections)
                    {
                        // Click on the "Add chapter" button
                        driver.FindElement(By.CssSelector("button[data-qa='add-chapter__btn']")).Click();


                        // Input section details
                        IWebElement sectionInputField = driver.FindElement(By.XPath("//input[@data-qa='chapter-name__input']"));
                        // Add a small delay if necessary
                        System.Threading.Thread.Sleep(500);

                        // Use JavaScript to clear the input field
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("arguments[0].value='';", sectionInputField);

                        // Add a small delay if necessary
                        System.Threading.Thread.Sleep(500);


                        sectionInputField.Clear();


                        sectionInputField.SendKeys(section.Name);
                        IWebElement saveButton = driver.FindElement(By.CssSelector("button[data-qa='actions-bar__save-button']"));
                        saveButton.Click();
                        Thread.Sleep(5000);
                        string url = driver.Url;
                        string[] parts = url.Split('/');
                        string chapterId = parts[parts.Length - 2]; // Second last part is the chapter ID

                        Console.WriteLine("Chapterid:"+ chapterId); // Output: 12727700
                        Thread.Sleep(5000);


                        // Loop through the videos in each section
                        foreach (var video in section.Videos)
                        {
                            driver.Navigate().GoToUrl(course.MainUrl+ "/chapters/"+ chapterId + "/contents/new_video_lesson");
                            Thread.Sleep(5000);

                            // Locate the label using the CSS selector
                            IWebElement labelForCheckbox = driver.FindElement(By.CssSelector("label[for='lesson-draft-status']"));

                            // Click the label to toggle the checkbox
                            labelForCheckbox.Click();

                            // Perform other actions or assertions if needed



                            // Wait for the video input field to appear
                            IWebElement videoInputField = driver.FindElement(By.CssSelector("input[data-qa='lesson-form__name']"));

                            videoInputField.Clear();
                            videoInputField.SendKeys(video.Name);

                            IWebElement saveVideoButton = driver.FindElement(By.XPath("//button[@data-qa='actions-bar__save-button']"));
                            saveVideoButton.Click();
                            Thread.Sleep(5000);
                        }
                    }
                }

                Console.WriteLine("Test completed successfully!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Test failed: {e.Message}");
                Console.Read();
            }
            finally
            {
                // Close the browser
                driver.Quit();
            }
            }
            }
}
