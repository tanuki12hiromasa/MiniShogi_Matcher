using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace USI_MultipleMatch
{
	class Player
	{
		public string name;
		public string enginename;
		public string path;
		public List<string> options;
		public Player(string name,string enginename,string path, List<string> options) {
			this.name = name;
			this.enginename = enginename;
			this.path = path;
			this.options = options;
		}
		public Player(string settingpath) {//player.txtからplayer情報を読み込んでPlayerを生成する
			//1行目:name 2行目:ソフト名 3行目:path 4行目~:option
			using (StreamReader reader=new StreamReader(settingpath)) {
				name = reader.ReadLine();
				enginename = reader.ReadLine();
				path = reader.ReadLine();
				options = new List<string>();
				while (!reader.EndOfStream) {
					string option = reader.ReadLine();
					if (option != "") { 
						options.Add(option);
					}
				}
			}
		}
		public Player(string enginepath,string playername) {//enginepathからengineを起動しデフォルトの値を読み込んでPlayerを生成する
			name = playername;
			path = enginepath;
			options = new List<string>();
			using (Process engine = new Process()) {
				engine.StartInfo.UseShellExecute = false;
				engine.StartInfo.RedirectStandardOutput = true;
				engine.StartInfo.RedirectStandardInput = true;
				engine.StartInfo.FileName = path;
				engine.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
				//pathからエンジンを起動してusiコマンドを入力
				engine.Start();
				engine.StandardInput.WriteLine("usi");
				while (true) {
					string usi = engine.StandardOutput.ReadLine();
					var tokens = usi.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					switch (tokens[0]) {
						case "id":
							if (tokens[1] == "name") {
								enginename = tokens[2];
								options.Add("USI_Ponder check false");
								options.Add("USI_Hash spin 256");
							}
							break;
						case "option":
							options.Add($"{tokens[2]} {tokens[4]} {tokens[6]}");
							break;
						case "usiok":
							engine.StandardInput.WriteLine("quit");
							Console.WriteLine($"player {name}'s infomation have been aquired.");
							return;
					}
				}
			}
		}
		public void settingsave(string settingpath)	{//player.txtにPlayerの情報を書き込む
			using (StreamWriter writer = new StreamWriter(settingpath, false)) {
				writer.WriteLine(name);
				writer.WriteLine(enginename);
				writer.WriteLine(path);
				foreach(string option in options) {
					writer.WriteLine(option);
				}
			}
		}

		public void Start(Process process) {//ProcessをPlayerの情報に基づいて開始する
			if (process == null) throw new IOException("Process is Null.");
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.ErrorDialog = false;
			process.StartInfo.FileName = path;
			process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
			process.Start();
			process.StandardInput.WriteLine("usi");
			while (true) { if (process.StandardOutput.ReadLine() == "usiok") break; }
			foreach (string usi in options) process.StandardInput.WriteLine(setoptionusi(usi));
			process.StandardInput.WriteLine("isready");
			while (true) { if (process.StandardOutput.ReadLine() == "readyok") break; }
			process.StandardInput.WriteLine("usinewgame");
		}

		static string setoptionusi(string settingline) {
			var token = settingline.Split(' ');
			return $"setoption name {token[0]} value {token[2]}";
		}

		public void Learn(Process engine, bool player_teban, Result result, List<int> evals, int eval_learn_border = 200) {
			engine.StandardInput.WriteLine("learn");
			
			//手番(s/g)
			if (player_teban) {
				engine.StandardInput.WriteLine("s");
			}
			else {
				engine.StandardInput.WriteLine("g");
			}

			//勝敗(s/g/d)
			switch (result) {
				case Result.SenteWin: engine.StandardInput.WriteLine("s"); break;
				case Result.Repetition:
				case Result.GoteWin: engine.StandardInput.WriteLine("g"); break;
				case Result.Draw: engine.StandardInput.WriteLine("d"); break;
			}

			//学習開始手数 （初形は0とする）
			int startcount = 0;
			for (int n = 0; n < evals.Count; n++) {
				if (Math.Abs(evals[n]) >= eval_learn_border) {
					startcount = n;
					break;
				}
			}
			engine.StandardInput.WriteLine((startcount).ToString());
			Console.WriteLine($"learning start from {startcount} moves.");

			while (true) {
				if (engine.HasExited) {
					Console.WriteLine("ERROR: ENGINE LOST");
					return;
				}
				string str = engine.StandardOutput.ReadLine();
				if (str != null && str != "") Console.WriteLine(str);
				if (str == "learning end.") break;
			}

		}

		public void save_eval(string folderpath) {
			using var proc = new Process();
			Start(proc);
			proc.StandardInput.WriteLine($"saveparam {Path.GetFullPath(folderpath)}");
			while (true) {
				string str = proc.StandardOutput.ReadLine();
				Console.WriteLine(str);
				if (str == "saveparam done.") break;
			}
			proc.StandardInput.WriteLine("quit");
			proc.Close();
		}
	}
}
