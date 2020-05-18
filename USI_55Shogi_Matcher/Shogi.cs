using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace USI_MultipleMatch
{
	class Kyokumen
	{
		enum Koma
		{
			s_Fu, s_Gin, s_Kaku, s_Hi, s_Kin, s_Ou,
			s_nFu, s_nGin, s_nKaku, s_nHi,
			g_Fu, g_Gin, g_Kaku, g_Hi, g_Kin, g_Ou,
			g_nFu, g_nGin, g_nKaku, g_nHi,
			KomaNum,
			None,
			Nari = s_nFu - s_Fu,
			Sengo = g_Fu - s_Fu,
			s_Min = s_Fu, s_Nari = s_nFu, 
			g_Min = g_Fu, g_Nari = g_nFu,
		}
		public Kyokumen() {
			teban = true;
			s_mochi = new int[5] { 0, 0, 0, 0, 0 };
			g_mochi = new int[5] { 0, 0, 0, 0, 0 };
			bammen = new Koma[5, 5] {
				{ Koma.g_Ou,Koma.g_Fu,Koma.None,Koma.None,Koma.s_Hi },
				{Koma.g_Kin,Koma.None,Koma.None,Koma.None,Koma.s_Kaku },
				{ Koma.g_Gin,Koma.None,Koma.None,Koma.None,Koma.s_Gin },
				{ Koma.g_Kaku,Koma.None,Koma.None,Koma.None,Koma.s_Kin },
				{ Koma.g_Hi,Koma.None,Koma.None,Koma.s_Fu,Koma.s_Ou }
			};
		}
		public Kyokumen(Kyokumen kyokumen,string usimove) {
			bammen = new Koma[5,5];
			s_mochi = new int[5];
			g_mochi = new int[5];
			teban = kyokumen.teban;
			Array.Copy(kyokumen.bammen, bammen, kyokumen.bammen.Length);
			Array.Copy(kyokumen.s_mochi, s_mochi, kyokumen.s_mochi.Length);
			Array.Copy(kyokumen.g_mochi, g_mochi, kyokumen.g_mochi.Length);
			proceed(usimove);
		}
		public void proceed(string usimove) {
			var to = usitovec(usimove[2], usimove[3]);
			if (usimove[1] != '*') {
				//移動
				var from = usitovec(usimove[0], usimove[1]);
				if (bammen[to.x, to.y] != Koma.None) {
					Koma m = bammen[to.x, to.y];
					if (teban) s_mochi[komatomochi(m)]++;
					else g_mochi[komatomochi(m)]++;
				}
				if (usimove.Length > 4 && usimove[4] == '+')
					bammen[to.x, to.y] = prom(bammen[from.x, from.y]);
				else
					bammen[to.x, to.y] = bammen[from.x, from.y];
				bammen[from.x, from.y] = Koma.None;
			}
			else {
				//駒打ち
				Koma koma = uchiusitokoma(usimove[0], teban);
				bammen[to.x, to.y] = koma;
				if (teban) s_mochi[komatomochi(koma)]--;
				else g_mochi[komatomochi(koma)]--;
			}
			teban = !teban;
		}

		Koma[,] bammen;
		int[] s_mochi;
		int[] g_mochi;
		bool teban;

		public static bool operator==(Kyokumen rhs,Kyokumen lhs) {
			if( rhs.teban == lhs.teban && rhs.s_mochi.SequenceEqual(lhs.s_mochi) &&
				rhs.g_mochi.SequenceEqual(lhs.g_mochi)) {
				for(int x = 0; x < 5; x++) {
					for(int y = 0; y < 5; y++) {
						if (rhs.bammen[x, y] != lhs.bammen[x, y])
							return false;
					}
				}
				return true;
			}
			return false;
		}
		public static bool operator!=(Kyokumen rhs,Kyokumen lhs) {
			return !(rhs == lhs);
		}
		static (int x,int y) usitovec(char a,char b) {
			return (a - '1', b - 'a');
		}
		static Koma prom(Koma k) {
			return (Koma)((int)k + (int)Koma.Nari);
		}
		static int komatomochi(Koma k) {
			int komanum = (int)k;
			if (komanum >= (int)Koma.g_Nari) {
				return komanum - (int)Koma.g_Nari;
			}
			else if(komanum >= (int)Koma.g_Min) {
				return komanum - (int)Koma.g_Min;
			}
			else if(komanum >= (int)Koma.s_Nari) {
				return komanum - (int)Koma.s_Nari;
			}
			else {
				return komanum;
			}
		}
		static Koma uchiusitokoma(char koma,bool teban) {
			if (teban) {
				return koma switch
				{
					'P' => Koma.s_Fu,
					'S' => Koma.s_Gin,
					'G' => Koma.s_Kin,
					'B' => Koma.s_Kaku,
					'R' => Koma.s_Hi,
					_ => Koma.None
				};
			}
			else {
				return koma switch
				{
					'P' => Koma.g_Fu,
					'S' => Koma.g_Gin,
					'G' => Koma.g_Kin,
					'B' => Koma.g_Kaku,
					'R' => Koma.g_Hi,
					_ => Koma.None
				};
			}
		}
		public static string usimove_to_csamove(string usi,Kyokumen resultkyokumen) {
			StringBuilder sb = new StringBuilder();
			if (!resultkyokumen.teban) {
				sb.Append('+');
			} 
			else {
				sb.Append('-');
			}
			if (usi[1] == '*') {
				sb.Append("00");
			}
			else {
				sb.Append(usi[0]);
				sb.Append((usi[1] - 'a' + 1).ToString());
			}
			int tox = usi[2] - '1';
			int toy = usi[3] - 'a';
			sb.Append(usi[2]);
			sb.Append((toy + 1).ToString());
			sb.Append(KomaToCsa(resultkyokumen.bammen[tox, toy]));
			return sb.ToString();
		}
		static string KomaToCsa(Koma k) {
			if ((int)k >= (int)Koma.g_Fu) k = (Koma)((int)k - (int)Koma.Sengo);
			return k switch
			{
				Koma.s_Fu => "FU",
				Koma.s_Gin => "GI",
				Koma.s_Kin => "KI",
				Koma.s_Kaku => "KA",
				Koma.s_Hi => "HI",
				Koma.s_Ou => "OU",
				Koma.s_nFu => "TO",
				Koma.s_nGin => "NG",
				Koma.s_nKaku => "UM",
				Koma.s_nHi => "RY",
				_ => ""
			};
		}
	}
}
