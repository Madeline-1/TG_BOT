using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_BOT
{
    public class Constants
    {
        public static string TimeAddress = "https://.../api/songLyrics/GetTime?songId=";
        public static string FavoritesAddress = "https://.../api/songLyrics/GetFavorites";
        public static string AddFavoriteAddress = "https://.../api/songLyrics/AddToFavorites";
        public static string RemoveFavoriteAddress = "https://.../api/songLyrics/RemoveFavorite?index=";
        public static string UpdateFavoriteAddress = "https://.../api/songLyrics/UpdateFavorite?index=";
        public static string ApiKey = "МійКлюч";
        public static string ApiHost = "spotify23.p.rapidapi.com";
    }
}
