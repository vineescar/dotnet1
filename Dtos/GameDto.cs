namespace GameStore.Api.Dtos;

public record class GameDto(
    int Id, 
    string Name,
    string Genre, // Fixed typo here
    decimal Price,
    DateOnly ReleaseDate // Added missing property
);
