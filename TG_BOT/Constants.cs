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
        public static string ApiKey = "МійКлюч";
        public static string ApiHost = "spotify23.p.rapidapi.com";
    }
}
