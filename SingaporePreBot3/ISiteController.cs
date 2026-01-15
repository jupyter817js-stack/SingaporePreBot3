using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingaporePreBot3
{
	public interface ISiteController
	{
		bool login();
		BetResult confirmBet(BetData betData);
		AddSlipResult addSlip(BetData betData);
		void removeSlip(BetData betData);
		double getBalance();
		void stop();
	}
}
