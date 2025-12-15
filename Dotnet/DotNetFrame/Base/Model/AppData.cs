using DotNet.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Base.Model
{
    internal static class AppData
    {
        #region Fields

        private static SQLite _db = null;
        private static Dictionary<string, string> _lang = null;

        #endregion Fields

        static AppData()
        {
            _db = new SQLite();
            InitLanguage();
        }

        internal static void InitLanguage()
        {
            string langCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            string query = $"SELECT KEY, {langCode.ToUpper()} FROM LANG";
            DataTable dt = _db.ExecuteQuery(query).Tables[0];
            Dictionary<string, string> dic = dt.AsEnumerable().ToDictionary(row => row.Field<string>(0), row => row.Field<string>(1));

            if (_lang == null)
                _lang = dic;
            else
            {
                foreach (var pair in dic)
                    _lang[pair.Key] = pair.Value;
            }
        }

        public static string Lang(string key, bool showKey = true)
        {
            if (_lang.ContainsKey(key))
                return _lang[key];
            else
            {
                if (showKey) return key;
                else return "Empty Text";
            }
        }
    }
}
