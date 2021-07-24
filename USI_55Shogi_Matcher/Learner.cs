using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace USI_MultipleMatch
{
	class Learner
	{
		public string name;
		public List<string> options;
		public string learner_path;
		public Learner(string settingpath) {
			//1行目:name 2行目:path 3行目~:option
			using (StreamReader reader = new StreamReader(settingpath)) {
				name = reader.ReadLine();
				learner_path = reader.ReadLine();
				options = new List<string>();
				while (!reader.EndOfStream) {
					string option = reader.ReadLine();
					if (option != "") {
						options.Add(option);
					}
				}
			}
		}
		public Learner(string learner_path,string name) {
			this.name = name;
			this.learner_path = learner_path;
			options = new List<string>();
			using (Process engine = new Process()) {
				engine.StartInfo.UseShellExecute = false;
				engine.StartInfo.RedirectStandardOutput = true;
				engine.StartInfo.RedirectStandardInput = true;
				engine.StartInfo.FileName = learner_path;
				engine.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(learner_path);
				//pathからエンジンを起動してusiコマンドを入力
				engine.Start();
				engine.StandardInput.WriteLine("usi");
				while (true) {
					string usi = engine.StandardOutput.ReadLine();
					var tokens = usi.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					switch (tokens[0]) {
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
		public void settingsave(string settingpath) {//player.txtにPlayerの情報を書き込む
			using (StreamWriter writer = new StreamWriter(settingpath, false)) {
				writer.WriteLine(name);
				writer.WriteLine(learner_path);
				foreach (string option in options) {
					writer.WriteLine(option);
				}
			}
		}
		public void Start(Process process) {
			if (process == null) throw new IOException("Process is Null.");
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.ErrorDialog = false;
			process.StartInfo.FileName = learner_path;
			process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(learner_path);
			process.Start();
			foreach (string usi in options) process.StandardInput.WriteLine(setoptionusi(usi));
			process.StandardInput.WriteLine("init");
			while (true) { if (process.StandardOutput.ReadLine() == "readyok") break; }



		}
		static string setoptionusi(string settingline) {
			var token = settingline.Split(' ');
			return $"setoption name {token[0]} value {token[2]}";
		}
	}
}
