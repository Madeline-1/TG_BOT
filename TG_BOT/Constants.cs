using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_BOT
{
    public class Constants
    {
        public static string TimeAddress = "https://localhost:7030/api/songLyrics/GetTime?songId=";
        public static string FavoritesAddress = "https://localhost:7030/api/songLyrics/GetFavorites";
        public static string AddFavoriteAddress = "https://localhost:7030/api/songLyrics/AddToFavorites";
        public static string RemoveFavoriteAddress = "https://localhost:7030/api/songLyrics/RemoveFavorite?index=";
        public static string UpdateFavoriteAddress = "https://localhost:7030/api/songLyrics/UpdateFavorite?index=";
        public static string ApiKey = "821ada0fcdmsh1b0d11162b8aeb6p1e23a7jsnb2a981386ca7";
        public static string ApiHost = "spotify23.p.rapidapi.com";
    }
}
