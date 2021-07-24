using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace USI_MultipleMatch
{
	class LearnTeam
	{
		public string teamname;

		public int batchnum;
		public int backup_span;
		public int ruiseki_count;

		Player player;
		Learner learner;
		List<Player> opponents;

		public LearnTeam(string teamname) {
			this.teamname = teamname;
			backup_span = 100;
			ruiseki_count = 0;
		}

		public bool load() {
			string teamfolder = "./learnteam/" + teamname;
			if (System.IO.File.Exists(teamfolder + "/setting.txt")) {
				int teamnum;
				using (StreamReader reader = new StreamReader(teamfolder + "/setting.txt")) {
					//チーム数、バッチ数
					teamnum = int.Parse(reader.ReadLine());
					batchnum = int.Parse(reader.ReadLine());
					backup_span = int.Parse(reader.ReadLine());
					ruiseki_count = int.Parse(reader.ReadLine());
				}

				learner = new Learner($"{teamfolder}/Learner.txt");
				player = new Player($"{teamfolder}/L-Player.txt");
				for(int i = 1; i <= teamnum; i++) {
					opponents.Add(new Player($"{teamfolder}/Player{i}.txt"));
				}

				return true;
			}
			else {
				return false;
			}
		}

		public void save_settingfile() {
			string teamfolder = "./learnteam/" + teamname;
			using (StreamWriter writer = new StreamWriter($"{teamfolder}/setting.txt")) {
				writer.WriteLine(opponents.Count);
				writer.WriteLine(batchnum);
				writer.WriteLine(backup_span);
				writer.WriteLine(ruiseki_count);
			}
		}


		public void setting() {
			string teamfolder = "./learnteam/" + teamname;
			if (System.IO.File.Exists(teamfolder + "/setting.txt")) {
				load();

				int opponentnum = opponents.Count;
				while (true) {
					Console.WriteLine("add Player? (y/n) > ");
					string ans = Console.ReadLine();
					if (ans != "y") break;

					opponentnum++;
					Console.Write($"Opponent Player {opponentnum} path? > ");
					string oPlayerpath = Console.ReadLine();
					var p = new Player(oPlayerpath, $"Player{opponentnum}");
					opponents.Add(p);
					p.settingsave($"{teamfolder}/Player{opponentnum}.txt");
				}

				save_settingfile();

			}
			else {
				Console.Write("Learn-Player Learner path? > ");
				string learnerpath = Console.ReadLine();
				learner = new Learner(learnerpath, "Leaner");
				learner.settingsave($"{teamfolder}/Learner.txt");

				Console.Write("Learn-Player Player path? > ");
				string lPlayerpath = Console.ReadLine();
				player = new Player(lPlayerpath, "L-Player");
				player.settingsave($"{teamfolder}/L-Player.txt");

				int opponentnum = 0;
				do {
					opponentnum++;
					Console.Write($"Opponent Player {opponentnum} path? > ");
					string oPlayerpath = Console.ReadLine();
					var p = new Player(oPlayerpath, $"Player{opponentnum}");
					opponents.Add(p);
					p.settingsave($"{teamfolder}/Player{opponentnum}.txt");

					Console.WriteLine("add Player? (y/n) > ");
					string ans = Console.ReadLine();
					if (ans != "y") break;
				} while (true);

				save_settingfile();
			}
		}

		public void versus() {

		}
	}
}
