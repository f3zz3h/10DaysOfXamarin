using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace TenDaysOfXamarin.Model
{
    public class Experience
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
