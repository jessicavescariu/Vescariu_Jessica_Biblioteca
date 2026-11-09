
namespace Lab1_MVC.Models
{
    public class RecommendationLog
    {
        public int ID { get; set; }

        public int ReaderId { get; set; }

        public DateTime CreatedAt { get; set; }

        // ID-urile cărților recomandate, salvate ca JSON
        public string BookIds { get; set; } = "";
    }
}
