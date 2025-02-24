namespace TaskManagerWebsite.Models
{
    public class GroupProject
    {

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}