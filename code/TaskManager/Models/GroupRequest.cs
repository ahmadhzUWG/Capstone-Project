namespace TaskManagerWebsite.Models
{
    public class GroupRequest
    {
        public int Id { get; set; }
        public int SenderId { get; set; } 
        public int GroupId { get; set; }     
        public Group Group { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public bool? Response { get; set; }  
    }
}
