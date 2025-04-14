namespace TaskManagerData.Models;

public class Comment
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
    public int TaskId { get; set; }
    public Task Task { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public int? ParentCommentId { get; set; }
    public Comment ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}