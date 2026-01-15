using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingaporePreBot3
{
    public class BetItem
    {
        public string id { get; set; }
        public string sport { get; set; }
        public string league { get; set; }
        public string competitionName { get; set; }
        public DateTime time { get; set; }
        public string strTime
        {
            get { return time.ToString("yyyy-MM-dd HH:mm:ss"); }
        }
        public string match { get; set; }
        public string market { get; set; }
        public int minute { get; set; }
        public int score1 { get; set; }
        public int score2 { get; set; }
        public double odd { get; set; }
        public string bookie { get; set; }
        public string value { get; set; }
        public int amount { get; set; }
        public string gameId { get; set; }
        public string strScore
        {
            get { return $"{score1}:{score2}"; }
        }
    }
}
