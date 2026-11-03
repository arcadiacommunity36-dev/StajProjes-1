using System;

namespace SimpleOrderSystem.Entities
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastUpdatedUserId { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public bool Status { get; set; } = true;
        public bool IsDeleted { get; set; }
    }
}
