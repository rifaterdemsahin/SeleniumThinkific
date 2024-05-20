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
            options.AddArgument(@"user-data-dir=C:\Users\Pexabo\AppData\Local\Google\Chrome\User Data\Profile 15"); // Replace with your Chrome user data path
            options.AddArgument(@"profile-directory=Profile 15"); // Replace with your profile directory


            // Initialize the ChromeDriver
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Window.Size = new System.Drawing.Size(1273, 672);

            driver.Navigate().GoToUrl("https://courses.devops.engineering/users/sign_in");

            // Set the window size
            driver.Manage().Window.Size = new System.Drawing.Size(1936, 1048);

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
                    // Click on the "Bulk importer" link
                    driver.FindElement(By.LinkText("Bulk importer")).Click();


                    // Initialize WebDriverWait
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    int sectionInputCounter = 5;
                    int videoInputCounter = 15; // Assuming different IDs for videos


                    // Loop through the sections and videos
                    foreach (var section in course.Sections)
                    {
                        // Click on the "Add chapter" button
                        driver.FindElement(By.CssSelector(".add-chapter_uOzhc")).Click();

                        try
                        {
                            // Wait for the "Successfully created a new chapter" toast message
                            wait.Until(drv => drv.FindElement(By.CssSelector(".Toast_toast__message__176 > span")));
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex.Message);
                        }


                        // Input section details
                        string sectionInputId = $"input-{sectionInputCounter++}";
                        IWebElement sectionInputField = driver.FindElement(By.Id(sectionInputId));
                        sectionInputField.Clear();
                        sectionInputField.SendKeys(section.Name);

                        // Loop through the videos in each section
                        foreach (var video in section.Videos)
                        {
                            // Click on the "Add video" button
                            driver.FindElement(By.CssSelector(".add-video-button")).Click(); // Assuming there's a button to add videos

                            // Wait for the video input field to appear
                            wait.Until(drv => drv.FindElement(By.CssSelector(".video-input-field"))); // Adjust selector as needed

                            // Input video details
                            string videoInputId = $"input-{videoInputCounter++}";
                            IWebElement videoInputField = driver.FindElement(By.Id(videoInputId));
                            videoInputField.Clear();
                            videoInputField.SendKeys(video.Name);
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
