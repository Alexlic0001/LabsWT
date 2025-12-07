namespace LabUI.Models
{
    public class ResponseData<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }

   
}