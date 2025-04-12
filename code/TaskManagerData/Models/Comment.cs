using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerData.Models
{
    public class Comment
    {
        public int Id { get; set; }

        // Foreign key linking to the task or entity being commented on
        public int TaskId { get; set; }
        public virtual Task Task { get; set; }

        // Foreign key linking to the user who wrote the comment
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // The text of the comment
        public string Content { get; set; }

        // Timestamp capturing when the comment was added
        public DateTime Timestamp { get; set; }
    }

}
