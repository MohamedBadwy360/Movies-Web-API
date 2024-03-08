using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private List<string> _allowedExtensions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1_048_576;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _context.Movies
                                .OrderByDescending(m => m.Rate)
                                .Include(m => m.Genre)
                                .Select(m => new MovieDetailsDto
                                {
                                    Id = m.Id,
                                    GenreId = m.GenreId,
                                    GenreName = m.Genre.Name,
                                    Poster = m.Poster,
                                    Rate = m.Rate,
                                    StoreLine = m.StoreLine,
                                    Title = m.Title,
                                    Year = m.Year
                                })
                                .ToListAsync();

            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _context.Movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);

            if (movie is null)
            {
                return BadRequest($"No movie with id {id} !");
            }

            var dto = new MovieDetailsDto
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                GenreName = movie.Genre.Name,
                Poster = movie.Poster,
                Rate = movie.Rate,
                StoreLine = movie.StoreLine,
                Title = movie.Title,
                Year = movie.Year
            };

            return Ok(dto);
        }

        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte GenreId)
        {
            var movies = await _context.Movies
                                .Where(m => m.GenreId == GenreId)
                                .OrderByDescending(m => m.Rate)
                                .Include(m => m.Genre)
                                .Select(m => new MovieDetailsDto
                                {
                                    Id = m.Id,
                                    GenreId = m.GenreId,
                                    GenreName = m.Genre.Name,
                                    Poster = m.Poster,
                                    Rate = m.Rate,
                                    StoreLine = m.StoreLine,
                                    Title = m.Title,
                                    Year = m.Year
                                })
                                .ToListAsync();

            return Ok(movies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] CreateMovieDto dto)
        {
            if (!_allowedExtensions.Contains(Path.GetExtension(dto.Poster.FileName)))
            {
                return BadRequest("Only jpg and png images are allowed!");
            }
            
            if (dto.Poster.Length > _maxAllowedPosterSize)
            {
                return BadRequest("Max allowed size is 1MB !");
            }

            bool isValidGenreId = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!isValidGenreId)
            {
                return BadRequest("Invalid GenreId !");
            }

            using var datastream = new MemoryStream();
            await dto.Poster.CopyToAsync(datastream);

            var movie = new Movie
            {
                GenreId = dto.GenreId,
                Poster = datastream.ToArray(),
                Rate = dto.Rate,
                StoreLine = dto.StoreLine,
                Title = dto.Title,
                Year = dto.Year
            };

            await _context.Movies.AddAsync(movie);
            _context.SaveChanges();
            return Ok(movie);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] UpdateMovieDto dto)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie is null)
            {
                return BadRequest($"No movie was found with id {id} !");
            }

            var isValidGenreId = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!isValidGenreId)
            {
                return BadRequest("Invalid GenreId");
            }

            if (dto.Poster is not null)
            {
                if (!_allowedExtensions.Contains(Path.GetExtension(dto.Poster.FileName)))
                {
                    return BadRequest("Only jpg and png images are allowed!");
                }
                if (dto.Poster.Length > _maxAllowedPosterSize)
                {
                    return BadRequest("Max allowed size is 1MB !");
                }

                using var dataStream = new MemoryStream();
                await dto.Poster.CopyToAsync(dataStream);
                movie.Poster = dataStream.ToArray();
            }

            movie.Title = dto.Title;
            movie.StoreLine = dto.StoreLine;
            movie.Year = dto.Year;
            movie.Rate = dto.Rate;
            movie.GenreId = dto.GenreId;

            _context.SaveChanges();
            return Ok(movie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie is null)
            {
                return BadRequest($"No Movie with id {id} !");
            }

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return Ok(movie);
        }
    }
}
