using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TaskManagerWebsite.Models
{
    [Table("User")]
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
    }

}