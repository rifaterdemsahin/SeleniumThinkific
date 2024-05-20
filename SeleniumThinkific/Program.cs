using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;

namespace SeleniumCourseLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program started!");
            // Initialize the ChromeDriver
            IWebDriver driver = new ChromeDriver();
            driver.Manage().Window.Size = new System.Drawing.Size(1273, 672);

            try
            {
                // Get the course data
                var folderPath = @"F:\source\SeleniumThinkific\SeleniumThinkific\data\";
                var courses = CourseData.GetCourses(folderPath);




                // Loop through each course
                foreach (var course in courses)
                {
                    // Open the URL
                    driver.Navigate().GoToUrl(course.MainUrl);

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

                        // Wait for the "Successfully created a new chapter" toast message
                        wait.Until(drv => drv.FindElement(By.CssSelector(".Toast_toast__message__176 > span")));

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
            }
            finally
            {
                // Close the browser
                driver.Quit();
            }
            }
            }
}
