namespace LoviBackend.Models.DbSets
{
    public class AudioBookReader
    {
        public int AudioBookId { get; set; }
        public AudioBook AudioBook { get; set; } = null!;

        public int CreatorId { get; set; }
        public Creator Creator { get; set; } = null!;
    }
}
