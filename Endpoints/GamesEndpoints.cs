using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
const string GetGameEndpointName ="GetGame";
private static readonly List<GameDto> games = new List<GameDto>
{
    new GameDto(1, "Street Fighter II", "Fighting", 19.999M, new DateOnly(1992, 7, 15)),
    new GameDto(2, "Cricket", "Sports", 12.999M, new DateOnly(1993, 8, 20)),
    new GameDto(3, "Badminton", "Sports", 10.999M, new DateOnly(1994, 9, 25))
};

public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app){

    var group=app.MapGroup("games").WithParameterValidation();

    //Get /games
        group.MapGet("/", async (GameStoreContext dbContext) => 
            await dbContext.Games
                     .Include(game => game.Genre)
                     .Select(game => game.ToGameSummaryDto())
                     .AsNoTracking()
                     .ToListAsync());

    //Get /games/1
    group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
    {
        Game? game = await dbContext.Games.FindAsync(id);

        return game is null ? 
            Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
    })
    .WithName(GetGameEndpointName);


    // Post /games
    group.MapPost("/", (CreateGameDto newGame, GameStoreContext dbContext) =>
    {
        Game game = newGame.ToEntity();
        game.Genre=dbContext.Genres.Find(newGame.GenreId);

        dbContext.Games.Add(game);
        dbContext.SaveChanges();

        return Results.CreatedAtRoute(GetGameEndpointName, new { id = game.Id }, game.ToGameDetailsDto());
    });


    //Put /games
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }

            dbContext.Entry(existingGame)
                     .CurrentValues
                     .SetValues(updatedGame.ToEntity(id));

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

    //Delete /games/1
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                     .Where(game => game.Id == id)
                     .ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}
