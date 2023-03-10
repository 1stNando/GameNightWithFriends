using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameNightWithFriends.Models;

namespace GameNightWithFriends.Controllers
{
    // All of these routes will be at the base URL:     /api/GameNights
    // That is what "api/[controller]" means below. It uses the name of the controller
    // in this case GameNightsController to determine the URL
    [Route("api/[controller]")]
    [ApiController]
    public class GameNightsController : ControllerBase
    {
        // This is the variable you use to have access to your database
        //Underscore means private detail not to be used outside of this class.
        private readonly DatabaseContext _context;

        // Constructor!!! that receives a reference to your database context
        // and stores it in _context for you to use in your API methods.
        //Dependency Injection!!!
        public GameNightsController(DatabaseContext context)
        {   //save a copy of the _context
            _context = context;
        }

        // GET: api/GameNights
        //
        // Returns a list of all your GameNights
        //
        [HttpGet]
        //Task wraps the thing we are returning, the ACTION RESULT, gives 
        //a status code and data. ie 200.http. In this case the ienumerable returned GameNight. A more abstract type of List<>.
        public async Task<ActionResult<IEnumerable<GameNight>>> GetGameNights()
        {
            // Uses the database context in `_context` to request all of the GameNights, sort
            // them by row id and return them as a JSON array.

            //.ToListAsync() allows for MANY users to be able to access the same URL
            //and be able to work on many requests from different users at the same time.
            //await makes the whole GET request
            //Added Include() to also be able to return the different players inside the game requested.
            return await _context.GameNights.OrderBy(row => row.Id).Include(gameNight => gameNight.Players).ToListAsync();
        }

        // GET: api/GameNights/5
        //
        // Fetches and returns a specific gameNight by finding it by id. The id is specified in the
        // URL. In the sample URL above it is the `5`.  The "{id}" in the [HttpGet("{id}")] is what tells dotnet
        // to grab the id from the URL. It is then made available to us as the `id` argument to the method.
        //
        [HttpGet("{id}")]
        //note this does not return a "List<>" but instead a singe "GameNight".
        public async Task<ActionResult<GameNight>> GetGameNight(int id)
        {
            // Find the gameNight in the database using `FindAsync` to look it up by id
            var gameNight = await _context.GameNights.FindAsync(id);

            // If we didn't find anything, we receive a `null` in return
            if (gameNight == null)
            {
                // Return a `404` response to the client indicating we could not find a gameNight with this id. Not found comes from Controller functionality. 
                return NotFound();
            }

            // Return the gameNight as a JSON object.
            return gameNight;
        }

        // PUT: api/GameNights/5
        //
        // Update an individual gameNight with the requested id. The id is specified in the URL
        // In the sample URL above it is the `5`. The "{id} in the [HttpPut("{id}")] is what tells dotnet
        // to grab the id from the URL. It is then made available to us as the `id` argument to the method.
        //
        // In addition the `body` of the request is parsed and then made available to us as a GameNight
        // variable named gameNight. The controller matches the keys of the JSON object the client
        // supplies to the names of the attributes of our GameNight POCO class. This represents the
        // new values for the record.
        //
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGameNight(int id, GameNight gameNight)
        {
            // If the ID in the URL does not match the ID in the supplied request body, return a bad request
            if (id != gameNight.Id)
            {
                return BadRequest();
            }

            //Later added a code to make sure one players games can not be added.
            if (gameNight.MinimumNumberOfPlayers < 2)
            {
                var badRequestMessage = new { Message = "You need at least two players!" };
                return BadRequest(badRequestMessage);
            }

            //More advanced EF:
            // Tell the database to consider everything in gameNight to be _updated_ values. When
            // the save happens the database will _replace_ the values in the database with the ones from gameNight
            _context.Entry(gameNight).State = EntityState.Modified;

            try
            {
                // Try to save these changes.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Ooops, looks like there was an error, so check to see if the record we were
                // updating no longer exists.
                if (!GameNightExists(id))
                {
                    // If the record we tried to update was already deleted by someone else,
                    // return a `404` not found
                    return NotFound();
                }
                else
                {
                    // Otherwise throw the error back, which will cause the request to fail
                    // and generate an error to the client.
                    throw;
                }
            }

            // Return a copy of the updated data
            return Ok(gameNight);
        }

        // POST: api/GameNights
        //
        // Creates a new gameNight in the database.
        //
        // The `body` of the request is parsed and then made available to us as a GameNight
        // variable named gameNight. The controller matches the keys of the JSON object the client
        // supplies to the names of the attributes of our GameNight POCO class. This represents the
        // new values for the record.
        //
        [HttpPost]
        public async Task<ActionResult<GameNight>> PostGameNight(GameNight gameNight)
        {
            //Later added a code to make sure one players games can not be added.
            //often called A GUARD CLAUS.
            if (gameNight.MinimumNumberOfPlayers < 2)
            {
                var badRequestMessage = new { Message = "You need at least two players!" };
                return BadRequest(badRequestMessage);
            }

            // Indicate to the database context we want to add this new record
            _context.GameNights.Add(gameNight);
            await _context.SaveChangesAsync();

            // Return a response that indicates the object was created (status code `201`) and some additional
            // headers with details of the newly created object.
            return CreatedAtAction("GetGameNight", new { id = gameNight.Id }, gameNight);
        }

        // DELETE: api/GameNights/5
        //
        // Deletes an individual gameNight with the requested id. The id is specified in the URL
        // In the sample URL above it is the `5`. The "{id} in the [HttpDelete("{id}")] is what tells dotnet
        // to grab the id from the URL. It is then made available to us as the `id` argument to the method.
        //
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGameNight(int id)
        {
            // Find this gameNight by looking for the specific id
            var gameNight = await _context.GameNights.FindAsync(id);
            if (gameNight == null)
            {
                // There wasn't a gameNight with that id so return a `404` not found
                return NotFound();
            }

            // Tell the database we want to remove this record
            _context.GameNights.Remove(gameNight);

            // Tell the database to perform the deletion
            await _context.SaveChangesAsync();

            // Return a copy of the deleted data, '200' action result
            return Ok(gameNight);
        }

        // Adding Players to a game night
        // POST /api/GameNights/5/Players
        [HttpPost("{id}/Players")]
        public async Task<ActionResult<Player>> CreatePlayerForGameNight(int id, Player player)
        //                                       |       |
        //                                       |       Player deserialized from JSON from the body
        //                                       |
        //                                       GameNight ID comes from the URL
        {
            //First, lets find the game night (by using the ID)
            var gameNight = await _context.GameNights.FindAsync(id);

            //This is like another GUARD CLAUSE...
            //If the game doesn't exist: return a 404 Not found.
            if (gameNight == null)
            {
                return NotFound();
            }

            // Associate the player to the given name night.
            player.GameNightId = gameNight.Id;

            // Add the player to the database
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Return the new player to the response of the API
            return Ok(player);
        }


        // Private helper method that looks up an existing gameNight by the supplied id
        private bool GameNightExists(int id)
        {
            return _context.GameNights.Any(gameNight => gameNight.Id == id);
        }
    }
}
