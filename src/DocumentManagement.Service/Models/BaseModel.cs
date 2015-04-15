namespace DocumentManagement.Service.Models
{
    public abstract class BaseModel
    {
        public int Id { get; set; }
        public bool HasErrors { get; set; }
    }
}
