namespace MoviesAPI.Dtos
{
    public class BaseMovieDto
    {
        public int Id { get; set; }
        [MaxLength(250)]
        public string Title { get; set; }
        public int Year { get; set; }
        public double Rate { get; set; }
        [MaxLength(2500)]
        public string StoreLine { get; set; }
        public byte GenreId { get; set; }
        public string GenreName { get; set; }
    }
}
