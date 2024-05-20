using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SeleniumCourseLoader
{
    public static class CourseData
    {
        public static List<CourseClass> GetCourses(string folderPath)
        {
            var courses = new List<CourseClass>();

            // Get paths of all JSON files in the specified folder
            var jsonFiles = Directory.GetFiles(folderPath, "*.json");

            // Iterate through each JSON file
            foreach (var jsonFile in jsonFiles)
            {
                // Read the JSON data from the file
                var json = File.ReadAllText(jsonFile);

                // Deserialize the JSON data into CourseClass object
                var course = JsonConvert.DeserializeObject<CourseClass>(json);

                // Add the course to the list
                courses.Add(course);
            }

            return courses;
        }
    }
}