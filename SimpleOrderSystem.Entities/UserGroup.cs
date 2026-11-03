namespace SimpleOrderSystem.Entities
{
    public class UserGroup : BaseEntity
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
    }
}
