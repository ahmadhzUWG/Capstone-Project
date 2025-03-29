namespace TaskManagerWebsite.Models
{
    /// <summary>
    /// Task that will be added to a stage on a project board
    /// </summary>
    public class Task
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the creator user identifier.
        /// </summary>
        public int CreatorUserId { get; set; }
    }
    
}
