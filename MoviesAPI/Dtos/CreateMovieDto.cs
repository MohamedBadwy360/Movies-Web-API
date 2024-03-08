namespace MoviesAPI.Dtos
{
    public class CreateMovieDto : BaseMovieDto
    {
        public IFormFile Poster { get; set; }
    }
}
