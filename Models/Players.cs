namespace GameNightWithFriends.Models
{
    public class Player
    {
        //Primary key
        public int Id { get; set; }
        public string Name { get; set; }

        //Foreign Key
        public int GameNightId { get; set; }

        //Gives back the associated GameNight for this Player.
        //A player attends ONE GameNight (so an Object/Class goes here)
        public GameNight GameNight { get; set; }
    }
}