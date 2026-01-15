using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SingaporePreBot3
{
    class Utils
    {
        private static NumberStyles style = NumberStyles.Any;
        private static CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");
        private static string relationPath = $"{Global.LogDir}\\relation.txt";
        public static double GetDecimalOdds(double odds)
        {
            return GetHKOdds(odds) + 1;
        }
        public static double GetHKOdds(double odds)
        {
            if (odds < 0)
            {
                odds = GetFloatDiv(-1, odds);
                odds = GetFloatDiv(GetFloor(GetFloatMul(odds, 100), 0), 100);
            }
            return odds;
        }

        public static double GetFloatDiv(double arg1, double arg2)
        {
            double t1 = 0, t2 = 0;
            double r1, r2;
            try { t1 = arg1.ToString().Split('.')[1].Length; } catch (Exception ex) { }
            try { t2 = arg2.ToString().Split('.')[1].Length; } catch (Exception ex) { }

            r1 = double.Parse(arg1.ToString().Replace(".", ""));

            r2 = double.Parse(arg2.ToString().Replace(".", ""));

            return (r1 / r2) * Math.Pow(10, t2 - t1);

        }
        public static double GetFloor(double odds, double count)
        {
            double multiple = 1;
            if (count != 0)
                multiple = Math.Pow(10, count);
            return odds >= 0 ? Math.Floor(odds * multiple + 0.0001) / multiple : -(Math.Floor(Math.Abs(odds) * multiple + 0.0001) / multiple);
        }

        public static double GetFloatMul(double arg1, double arg2)
        {
            var m = 0;
            string s1 = arg1.ToString(), s2 = arg2.ToString();
            try { m += s1.Split('.')[1].Length; } catch (Exception ex) { }
            try { m += s2.Split('.')[1].Length; } catch (Exception ex) { }
            return double.Parse(s1.Replace(".", "")) * double.Parse(s2.Replace(".", "")) / Math.Pow(10, m);
        }
        public static long getTick()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            long timestamp = (long)t.TotalMilliseconds;
            return timestamp;
        }
        public static void sendTelegram(string chatId, string content, string typeToken)
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUrl = string.Format("https://api.telegram.org/bot{0}/sendMessage", typeToken);
                HttpResponseMessage message = client.PostAsync(requestUrl, new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("chat_id", chatId),
                new KeyValuePair<string, string>("text", content),
                new KeyValuePair<string, string>("parse_mode", "HTML"),
                new KeyValuePair<string, string>("disable_web_page_preview", "false"),
            })).Result;
                string ret = message.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
        public static string GeneratePassword()
        {
            int Length = 16;
            string allowedChars = "abcdefghijkmnopqrstuvwxyz";
            string allowedNonAlphaNum = "0123456789";
            Random rd = new Random();

            char[] pass = new char[Length];
            int[] pos = new int[Length];
            int i = 0, j = 0, temp = 0;
            bool flag = false;

            //Random the position values of the pos array for the string Pass
            while (i < Length - 1)
            {
                j = 0;
                flag = false;
                temp = rd.Next(0, Length);
                for (j = 0; j < Length; j++)
                    if (temp == pos[j])
                    {
                        flag = true;
                        j = Length;
                    }

                if (!flag)
                {
                    pos[i] = temp;
                    i++;
                }
            }

            //Random the AlphaNumericChars
            for (i = 0; i < Length - 8; i++)
                pass[i] = allowedChars[rd.Next(0, allowedChars.Length)];

            //Random the NonAlphaNumericChars
            for (i = Length - 8; i < Length; i++)
                pass[i] = allowedNonAlphaNum[rd.Next(0, allowedNonAlphaNum.Length)];

            //Set the sorted array values by the pos array for the rigth posistion
            char[] sorted = new char[Length];
            for (i = 0; i < Length; i++)
                sorted[i] = pass[pos[i]];

            string Pass = new String(sorted);

            return Pass;
        }
        private static AesManaged CreateAes()
        {
            string seckey = "0pWstMXG9vB^STnp";
            string iv = "xwtlG2ir7Z0K7u@5";
            var aes = new AesManaged();
            aes.Key = System.Text.Encoding.UTF8.GetBytes(seckey); //UTF8-Encoding
            aes.IV = System.Text.Encoding.UTF8.GetBytes(iv);//UT8-Encoding
            return aes;
        }
        public static string decrypt(string text)
        {
            using (var aes = CreateAes())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(text)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }

            }
        }

        public static string encrypt(string text)
        {
            try
            {
                using (AesManaged aes = CreateAes())
                {
                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                                sw.Write(text);
                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string Compress(string source)
        {
            byte[] sourceArray = Encoding.UTF8.GetBytes(source);
            MemoryStream memoryStream = new MemoryStream();
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(sourceArray, 0, sourceArray.Length);
            }
            memoryStream.Position = 0;
            byte[] temporaryArray = new byte[memoryStream.Length];
            memoryStream.Read(temporaryArray, 0, temporaryArray.Length);
            byte[] targetArray = new byte[temporaryArray.Length + 4];
            Buffer.BlockCopy(temporaryArray, 0, targetArray, 4, temporaryArray.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(sourceArray.Length), 0, targetArray, 0, 4);
            return Convert.ToBase64String(targetArray);
        }
        public static int ParseToInt(string str)
        {
            int val = 0;
            int.TryParse(str, out val);
            return val;
        }

        public static double ParseToDouble(string str)
        {
            double val = 0;
            double.TryParse(str, style, culture, out val);
            return val;
        }

        public static long ParseToLong(string str)
        {
            long val = 0;
            long.TryParse(str, out val);
            return val;
        }

        public static DateTime ParseToDateTime(string str, string format = "MM-dd hh:mm tt")
        {
            DateTime val = DateTime.Now;
            if (format == "MM-dd hh:mm tt")
                DateTime.TryParse(str, out val);
            else
                DateTime.TryParseExact(str, format, null, System.Globalization.DateTimeStyles.None, out val);
            return val;
        }
        public static string getMarketName(string type, bool isCorner, bool isHome, double line)
        {
            string ret = string.Empty;
            try
            {
                string tmp = string.Empty;
                switch (type)
                {
                    case "1":
                        tmp = isHome ? $"FTAH1({line})" : $"FTAH2({line})";
                        break;
                    case "3":
                        tmp = isHome ? $"FTTO({line})" : $"FTTO({line})";
                        break;
                    case "7":
                        tmp = isHome ? $"HTAH1({line})" : $"HTAH2({line})";
                        break;
                    case "8":
                        tmp = isHome ? $"HTTO({line})" : $"HTTO({line})";
                        break;
                }
                if (string.IsNullOrEmpty(tmp))
                    return tmp;
                return $"{(isCorner ? "C" : "")}{tmp}";
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return ret;
        }
        public static double getOdd(double val, int digit)
        {
            double ret = 0;
            if (val == 0)
                return ret;
            if (val > 0)
                val += 1;
            else
                val = 1 / (-1 * val) + 1;
            double times = Math.Pow(10, digit);
            int tmp = (int)(val * times);
            ret = tmp / times;
            return ret;
        }
        public static double FractionToDouble(string fraction)
        {
            double result;

            if (double.TryParse(fraction, out result))
            {
                return result;
            }

            string[] split = fraction.Split(new char[] { ' ', '/' });

            if (split.Length == 2 || split.Length == 3)
            {
                int a, b;

                if (int.TryParse(split[0], out a) && int.TryParse(split[1], out b))
                {
                    if (split.Length == 2)
                    {
                        return 1 + Math.Floor((double)1000 * a / b) / 1000;
                    }

                    int c;

                    if (int.TryParse(split[2], out c))
                    {
                        return a + (double)b / c;
                    }
                }
            }

            throw new FormatException("Not a valid fraction. => " + fraction);
        }
        public static void DoSplitTeams(string text, ref string home, ref string away)
        {
            string[] teamSeps = new string[] { " vs ", " v ", " - ", " x ", " @ " };
            try
            {
                string[] teams = new string[] { };
                for (int i = 0; i < teamSeps.Length; i++)
                {
                    teams = text.Split(new string[] { teamSeps[i] }, StringSplitOptions.None);
                    if (teams.Length == 2) break;
                }
                if (teams.Length != 2)
                {
                    teams = text.Split(new string[] { " " }, StringSplitOptions.None);
                    teams[0] = teams[0] + ' ' + teams[1];
                    teams[1] = teams[2] + ' ' + teams[3];
                }
                home = teams[0].Trim();
                away = teams[1].Trim();
                return;
            }
            catch
            {
            }
        }
        public static bool isSameMatch(string mLeagueName, string mHome, string mAway, int mTime, string eLeagueName, string eHome, string eAway, int eTime)
        {
            if (Math.Abs(mTime - eTime) > 1)
                return false;

            if (string.IsNullOrEmpty(mLeagueName) || string.IsNullOrEmpty(eLeagueName) || string.IsNullOrEmpty(mHome) || string.IsNullOrEmpty(mAway) || string.IsNullOrEmpty(eHome) || string.IsNullOrEmpty(eAway))
                return false;

            if (((mLeagueName.IndexOf("WOMEN", StringComparison.OrdinalIgnoreCase) >= 0) && (eLeagueName.IndexOf("WOMEN", StringComparison.OrdinalIgnoreCase) == -1))
                || ((mLeagueName.IndexOf("WOMEN", StringComparison.OrdinalIgnoreCase) == -1) && (eLeagueName.IndexOf("WOMEN", StringComparison.OrdinalIgnoreCase) >= 0)))
                return false;

            if (!string.IsNullOrEmpty(mLeagueName))
                mLeagueName = parseTeamName(mLeagueName);
            if (!string.IsNullOrEmpty(eLeagueName))
                eLeagueName = parseTeamName(eLeagueName);
            if (!string.IsNullOrEmpty(mHome))
                mHome = parseTeamName(mHome);
            if (!string.IsNullOrEmpty(mAway))
                mAway = parseTeamName(mAway);
            if (!string.IsNullOrEmpty(eHome))
                eHome = parseTeamName(eHome);
            if (!string.IsNullOrEmpty(eAway))
                eAway = parseTeamName(eAway);

            if (mLeagueName == eLeagueName && mHome == eHome && mAway == eAway)
                return true;
            double leagueDistance = 1;
            double homeDistance = 1;
            double awayDistance = 1;

            if (mLeagueName.Contains(eLeagueName) || eLeagueName.Contains(mLeagueName))
                leagueDistance = 0;
            else
                leagueDistance = getDistanceTwoTeam(mLeagueName, eLeagueName);

            if (mHome.Contains(eHome) || eHome.Contains(mHome))
                homeDistance = 0;
            else
                homeDistance = getDistanceTwoTeam(mHome, eHome);

            if (mAway.Contains(eAway) || eAway.Contains(mAway))
                awayDistance = 0;
            else
                awayDistance = getDistanceTwoTeam(mAway, eAway);

            double totalDist = homeDistance + awayDistance;
            if (totalDist < 0.21 || homeDistance == 0 && awayDistance < 0.33 || awayDistance == 0 && homeDistance < 0.33)
            {
                if (leagueDistance > 0.5)
                    return false;
                else
                return true;
            }
            return false;
        }
        public static double getDistanceTwoTeam(string one, string two)
        {
            double ret = 0;
            if (string.IsNullOrEmpty(one) || string.IsNullOrEmpty(two))
                return 1;
            if (one.Contains("Dutch League"))
                one = one.Replace("Dutch League", "NETHERLANDS");
            if (one.Contains("NETHERLANDS"))
                one = one.Replace("NETHERLANDS", "Dutch League");
            try
            {
                List<string> oneList = one.Split(new string[] { " ", "-", "/", "and", "&" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                List<string> twoList = two.Split(new string[] { " ", "-", "/", "and", "&" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                
                double nativeVal = getDistanceTwoWords(one, two);
                double oneTwo = nativeVal, twoOne = nativeVal;
                if (oneList.Count > 1)
                    oneTwo = oneList.Select(o => twoList.Select(t => getDistanceTwoWords(o, t)).Min()).Average();
                if (twoList.Count > 1)
                    twoOne = twoList.Select(o => oneList.Select(t => getDistanceTwoWords(o, t)).Min()).Average();
                ret = Math.Min(oneTwo, twoOne);
            }
            catch (Exception ex)
            {

            }
            return ret;
        }
        public static double getDistanceTwoWords(string one, string two)
        {
            double ret = 0;
            if (string.IsNullOrEmpty(one) || string.IsNullOrEmpty(two))
                return 1;
            
            if (one.StartsWith(two) || two.StartsWith(one))
            {
                if (one.Length > 2 && two.Length > 2)
                    ret = 0;
                else
                    ret = 0.02 * Math.Abs(one.Length - two.Length);
            }
            else
                ret = JaroWinklerDistance.distance(one, two);
            return ret;
        }
        public static string removeDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        public static string parseTeamName(string team)
        {
            string ret = string.Empty;
            if (string.IsNullOrEmpty(team))
                return ret;
            try
            {
                team = removeDiacritics(team);
                ret = Regex.Replace(team, @"(CD +)|(De +)|(Club +)|( +II)", "").Trim();
                ret = Regex.Replace(ret, @" *\([^\)]*\) *", "").Trim();
                ret = ret.Replace(" / ", "/").Trim();
                ret = ret.ToLower().Replace("  ", " ").Trim();
                ret = Regex.Replace(ret, @"^((cf )|(ca ))|(( cf)|( u\d+)|( olympic))$", "").Trim();
                //ret = Regex.Replace(ret, @"^((fc )|(cf )|(ca ))|(( fc)|( cf)|( u\d+)|( olympic))$", "").Trim();
                ret = Regex.Replace(ret, @"( de )|(sc )|(football )|( reserves)|( women)", "").Trim();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return ret;
        }
        public static double calcProfit(double dOddA, double dOddsB)
        {
            return (dOddA * dOddsB - (dOddA + dOddsB)) / (dOddA + dOddsB);
        }
        public static string trySolvingCaptcha(string googleKey, string pageUrl)
        {
            string verifyCode = string.Empty;

            try
            {
                HttpClient httpClient = new HttpClient();
                string sendUrl = $"http://2captcha.com/in.php?key={Setting.Instance.captchaKey}&method=userrecaptcha&googlekey={googleKey}&pageurl={pageUrl}";
                HttpResponseMessage responseMessageMain = httpClient.GetAsync(sendUrl).Result;
                responseMessageMain.EnsureSuccessStatusCode();

                string sendUrlString = responseMessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(sendUrlString))
                    return verifyCode;

                if (!sendUrlString.Contains("OK|"))
                    return verifyCode;

                string captchaId = sendUrlString.Replace("OK|", string.Empty);
                if (string.IsNullOrEmpty(captchaId))
                    return verifyCode;

                string verifyUrl = string.Format("http://2captcha.com/res.php?key={0}&action=get&id={1}", Setting.Instance.captchaKey, captchaId);

                int requestCount = 0;
                while (requestCount < 20)
                {
                    Thread.Sleep(15000);
                    requestCount++;

                    HttpResponseMessage responseMessageVerify = httpClient.GetAsync(verifyUrl).Result;
                    responseMessageVerify.EnsureSuccessStatusCode();

                    string verifyUrlString = responseMessageVerify.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(verifyUrlString))
                        continue;

                    if (!verifyUrlString.Contains("OK|") && verifyUrlString.Contains("CAPCHA_NOT_READY"))
                        continue;

                    verifyCode = verifyUrlString.Replace("OK|", string.Empty);
                    break;
                }

                return verifyCode;
            }
            catch (Exception e)
            {
                return verifyCode;
            }
        }
        public static string trySolvingCaptchaByImg(byte[] aArray)
        {
            string verifyCode = string.Empty;

            try
            {
                string base64Content = Convert.ToBase64String(aArray);
                HttpClient httpClient = new HttpClient();
                //string sendUrl = $"http://2captcha.com/in.php?key={Setting.Instance.captchaKey}&method=userrecaptcha&googlekey={googleKey}&pageurl={pageUrl}";
                HttpResponseMessage responseMessageMain = httpClient.PostAsync("http://2captcha.com/in.php", new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("key", Setting.Instance.captchaKey),
                    new KeyValuePair<string, string>("method", "base64"),
                    new KeyValuePair<string, string>("body", base64Content),
                })).Result;
                responseMessageMain.EnsureSuccessStatusCode();

                string sendUrlString = responseMessageMain.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(sendUrlString))
                    return verifyCode;

                if (!sendUrlString.Contains("OK|"))
                    return verifyCode;

                string captchaId = sendUrlString.Replace("OK|", string.Empty);
                if (string.IsNullOrEmpty(captchaId))
                    return verifyCode;

                string verifyUrl = string.Format("http://2captcha.com/res.php?key={0}&action=get&id={1}", Setting.Instance.captchaKey, captchaId);

                int requestCount = 0;
                while (requestCount < 20)
                {
                    Thread.Sleep(3000);
                    requestCount++;

                    HttpResponseMessage responseMessageVerify = httpClient.GetAsync(verifyUrl).Result;
                    responseMessageVerify.EnsureSuccessStatusCode();

                    string verifyUrlString = responseMessageVerify.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(verifyUrlString))
                        continue;

                    if (!verifyUrlString.Contains("OK|") && verifyUrlString.Contains("CAPCHA_NOT_READY"))
                        continue;

                    verifyCode = verifyUrlString.Replace("OK|", string.Empty).Trim();
                    break;
                }

                return verifyCode;
            }
            catch (Exception e)
            {
                return verifyCode;
            }
        }
        public static double convertStake(double old)
        {
            double ret = 0;
            double times = Math.Pow(10, Setting.Instance.roundType);
            double tmp = Math.Round(old / times);
            ret = tmp * times;
            return ret;
        }
    }
}
