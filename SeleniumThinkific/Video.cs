using System.Collections.Generic;

namespace SeleniumCourseLoader
{
    public class Video
    {
        public string Name { get; set; }
    }

    public class Section
    {
        public string Name { get; set; }
        public string ChapterId { get; set; }
        
        public List<Video> Videos { get; set; }

        public Section()
        {
            Videos = new List<Video>();
        }
    }

    public class CourseClass
    {
        public string MainUrl { get; set; }
        public string Name { get; set; }
        public List<Section> Sections { get; set; }

        public CourseClass()
        {
            Sections = new List<Section>();
        }
        }
}
