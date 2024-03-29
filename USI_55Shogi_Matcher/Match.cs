﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace USI_MultipleMatch 
{
	enum Result
	{
		SenteWin, GoteWin, Repetition, Draw
	}
	class Match 
	{
		public static Result match(string matchname, uint byoyomi, Player b, Player w, string startusi = "startpos", string kifupath = @"./kifu.txt", int waittime = 100) {//開始局面のusiはsfen部分から
			while (true) {
				using (Process sente = new Process())
				using (Process gote = new Process()) {
					try {
						Console.Write($"waiting setup {b.name}...");
						b.Start(sente);
						Console.WriteLine(" readyok.");
						Console.Write($"waiting setup {w.name}...");
						w.Start(gote);
						Console.WriteLine(" readyok.");
						List<string> kifu = new List<string>();
						List<int> evals = new List<int>();
						int startmove = 1;
						List<Kyokumen> history = new List<Kyokumen>();
						string go = $"go btime 0 wtime 0 byoyomi {byoyomi}";
						var starttime = DateTime.Now;
						string startsfen = "startpos";
						if (startusi != "startpos") {
							string[] tokens = startusi.Split(' ', StringSplitOptions.RemoveEmptyEntries);
							if (tokens[0] != "startpos") throw new NotImplementedException();
							else {
								history.Add(new Kyokumen());
								for (int i = 2; i < tokens.Length; i++) {
									kifu.Add(tokens[i]);
									history.Add(new Kyokumen(history[history.Count - 1], tokens[i]));
								}
								startmove = history.Count;
							}
						}
						else {
							history.Add(new Kyokumen());
						}
						Console.WriteLine($"start at {startusi}");
						Console.Write($"{starttime.ToString(Kifu.TimeFormat)} {matchname}:");
						while (true) {
							if (kifu.Count % 2 == 0) {//先手
								var (move, eval) = GetMove(sente, position(startsfen, kifu), go);
								kifu.Add(move);
								evals.Add(eval);
								Console.Write($" b:{move}({eval})");
								if (move == "resign") {
									Result result = Result.GoteWin;
									Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
									sendGameOver(sente, gote, result);
									return result;
								}
								else if (move == "win") {
									Result result = Result.SenteWin;
									Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
									sendGameOver(sente, gote, result);
									return result;
								}
								var nextKyokumen = new Kyokumen(history[history.Count - 1], move);
								if (CheckRepetition(nextKyokumen, history)) {
									Result result = Result.Repetition;
									Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
									sendGameOver(sente, gote, result);
									return result;
								}
								if (CheckEndless(history.Count)) {
									Result result = Result.Draw;
									Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
									sendGameOver(sente, gote, result);
									return result;
								}
								history.Add(nextKyokumen);
							}
							else {//後手
								var (move, eval) = GetMove(gote, position(startsfen, kifu), go);
								kifu.Add(move);
								evals.Add(-eval);
								Console.Write($" w:{move}({-eval})");
								if (move == "resign") {
									Result result = Result.SenteWin;
									Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
									sendGameOver(sente, gote, result);
									return result;
								}
								else if (move == "win") {
									Result result = Result.GoteWin;
									Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
									sendGameOver(sente, gote, result);
									return result;
								}
								var nextKyokumen = new Kyokumen(history[history.Count - 1], move);
								if (CheckRepetition(nextKyokumen, history)) {
									Result result = Result.Repetition;
									Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
									sendGameOver(sente, gote, result);
									return result;
								}
								if (CheckEndless(history.Count)) {
									Result result = Result.Draw;
									Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
									sendGameOver(sente, gote, result);
									return result;
								}
								history.Add(nextKyokumen);
							}
						}
					}
					catch (Exception e) {
						Console.WriteLine(e);
						if(!sente.HasExited) sente.StandardInput.WriteLine("quit");
						if(!gote.HasExited) gote.StandardInput.WriteLine("quit");
					}
					finally {
						if (!sente.HasExited) sente.StandardInput.WriteLine("quit");
						if (!gote.HasExited) gote.StandardInput.WriteLine("quit");
						if (!sente.WaitForExit(waittime)) {
							try {
								sente.Kill();
							}
							catch (Exception)				
							{
								//プロセスは既に終了しているので何もしない（waitが終わってkillを呼ぶまでの一瞬の間にプロセスが終了した場合に例外が発生する）
							}
						}
						if (!gote.WaitForExit(waittime)) {
							try {
								gote.Kill();
							}
							catch (Exception) {
								//プロセスは既に終了しているので何もしない
							}
						}
					}
				}
			}
		}

		public static Result match(string matchname, uint byoyomi, Player b, Player w, out List<string> _kifu, out List<int> _evals, string startusi = "startpos", string kifupath = @"./kifu.txt") {

			while (true) {
				using (Process sente = new Process())
				using (Process gote = new Process()) {
					try {
						Console.Write($"waiting setup {b.name}...");
						b.Start(sente);
						Console.WriteLine(" readyok.");
						Console.Write($"waiting setup {w.name}...");
						w.Start(gote);
						Console.WriteLine(" readyok.");
						var kifu = new List<string>();
						var evals = new List<int>();
						int startmove = 1;
						List<Kyokumen> history = new List<Kyokumen>();
						string go = $"go btime 0 wtime 0 byoyomi {byoyomi}";
						var starttime = DateTime.Now;
						string startsfen = "startpos";
						if (startusi != "startpos") {
							string[] tokens = startusi.Split(' ', StringSplitOptions.RemoveEmptyEntries);
							if (tokens[0] != "startpos") throw new NotImplementedException();
							else {
								history.Add(new Kyokumen());
								for (int i = 2; i < tokens.Length; i++) {
									kifu.Add(tokens[i]);
									evals.Add(0);
									history.Add(new Kyokumen(history[history.Count - 1], tokens[i]));
								}
								startmove = history.Count;
							}
						}
						else {
							history.Add(new Kyokumen());
						}
						Console.WriteLine($"start at {startusi}");
						Console.Write($"{starttime.ToString(Kifu.TimeFormat)} {matchname}:");
						Result result;
						while (true) {
							if (kifu.Count % 2 == 0) {//先手
								var (move, eval) = GetMove(sente, position(startsfen, kifu), go);
								Console.Write($" b:{move}({eval})");
								if (move == "resign") {
									result = Result.GoteWin;
									break;
								}
								else if (move == "win") {
									result = Result.SenteWin;
									break;
								}
								var nextKyokumen = new Kyokumen(history[history.Count - 1], move);
								if (CheckRepetition(nextKyokumen, history)) {
									result = Result.Repetition;
									break;
								}
								if (CheckEndless(history.Count)) {
									result = Result.Draw;
									break;
								}
								kifu.Add(move);
								evals.Add(eval);
								history.Add(nextKyokumen);
							}
							else {//後手
								var (move, eval) = GetMove(gote, position(startsfen, kifu), go);
								Console.Write($" w:{move}({-eval})");
								if (move == "resign") {
									result = Result.SenteWin;
									break;
								}
								else if (move == "win") {
									result = Result.GoteWin;
									break;
								}
								var nextKyokumen = new Kyokumen(history[history.Count - 1], move);
								if (CheckRepetition(nextKyokumen, history)) {
									result = Result.Repetition;
									break;
								}
								if (CheckEndless(history.Count)) {
									result = Result.Draw;
									break;
								}
								kifu.Add(move);
								evals.Add(-eval);
								history.Add(nextKyokumen);
							}
						}
						Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
						sendGameOver(sente, gote, result);
						_kifu = kifu;
						_evals = evals;
						return result;
					}
					catch (Exception e) {
						Console.WriteLine(e);
						if (!sente.HasExited) sente.StandardInput.WriteLine("quit");
						if (!gote.HasExited) gote.StandardInput.WriteLine("quit");
					}
					finally {
						if (!sente.WaitForExit(100)) {
							try {
								sente.Kill();
							}
							catch (Exception) {
								//プロセスは既に終了しているので何もしない（waitが終わってkillを呼ぶまでの一瞬の間にプロセスが終了した場合に例外が発生する）
							}
						}
						if (!gote.WaitForExit(100)) {
							try {
								gote.Kill();
							}
							catch (Exception) {
								//プロセスは既に終了しているので何もしない
							}
						}
					}
				}
			}
		}


		public static Result match_and_learn(string matchname, uint byoyomi, Player b, Player w, bool sentelearn, bool gotelearn, string startusi = "startpos", string kifupath = @"./kifu.txt") {

			while (true) {
				using (Process sente = new Process())
				using (Process gote = new Process()) {
					try {
						Console.Write($"waiting setup {b.name}...");
						b.Start(sente);
						Console.WriteLine(" readyok.");
						Console.Write($"waiting setup {w.name}...");
						w.Start(gote);
						Console.WriteLine(" readyok.");
						var kifu = new List<string>();
						var evals = new List<int>();
						int startmove = 1;
						List<Kyokumen> history = new List<Kyokumen>();
						string go = $"go btime 0 wtime 0 byoyomi {byoyomi}";
						var starttime = DateTime.Now;
						string startsfen = "startpos";
						if (startusi != "startpos") {
							string[] tokens = startusi.Split(' ', StringSplitOptions.RemoveEmptyEntries);
							if (tokens[0] != "startpos") throw new NotImplementedException();
							else {
								history.Add(new Kyokumen());
								for (int i = 2; i < tokens.Length; i++) {
									kifu.Add(tokens[i]);
									evals.Add(0);
									history.Add(new Kyokumen(history[history.Count - 1], tokens[i]));
								}
								startmove = history.Count;
							}
						}
						else {
							history.Add(new Kyokumen());
						}
						Console.WriteLine($"start at {startusi}");
						Console.Write($"{starttime.ToString(Kifu.TimeFormat)} {matchname}:");
						Result result;
						while (true) {
							if (kifu.Count % 2 == 0) {//先手
								var (move, eval) = GetMove(sente, position(startsfen, kifu), go);
								Console.Write($" b:{move}({eval})");
								if (move == "resign") {
									result = Result.GoteWin;
									break;
								}
								else if (move == "win") {
									result = Result.SenteWin;
									break;
								}
								var nextKyokumen = new Kyokumen(history[history.Count - 1], move);
								if (CheckRepetition(nextKyokumen, history)) {
									result = Result.Repetition;
									break;
								}
								if (CheckEndless(history.Count)) {
									result = Result.Draw;
									break;
								}
								kifu.Add(move);
								evals.Add(eval);
								history.Add(nextKyokumen);
							}
							else {//後手
								var (move, eval) = GetMove(gote, position(startsfen, kifu), go);
								Console.Write($" w:{move}({-eval})");
								if (move == "resign") {
									result = Result.SenteWin;
									break;
								}
								else if (move == "win") {
									result = Result.GoteWin;
									break;
								}
								var nextKyokumen = new Kyokumen(history[history.Count - 1], move);
								if (CheckRepetition(nextKyokumen, history)) {
									result = Result.Repetition;
									break;
								}
								if (CheckEndless(history.Count)) {
									result = Result.Draw;
									break;
								}
								kifu.Add(move);
								evals.Add(-eval);
								history.Add(nextKyokumen);
							}
						}
						Kifu.FoutKifu(starttime, matchname, b, w, byoyomi, kifu, startmove, evals, result, kifupath);
						sendOnlyGameOver(sente, gote, result);
						if (sentelearn) b.Learn(sente, true, result, evals);
						if (gotelearn) w.Learn(gote, false, result, evals);
						sendQuit(sente);
						sendQuit(gote);
						return result;
					}
					catch (Exception e) {
						Console.WriteLine(e);
						if (!sente.HasExited) sente.StandardInput.WriteLine("quit");
						if (!gote.HasExited) gote.StandardInput.WriteLine("quit");
					}
					finally {
						if (!sente.WaitForExit(100)) {
							try {
								sente.Kill();
							}
							catch (Exception) {
								//プロセスは既に終了しているので何もしない（waitが終わってkillを呼ぶまでの一瞬の間にプロセスが終了した場合に例外が発生する）
							}
						}
						if (!gote.WaitForExit(100)) {
							try {
								gote.Kill();
							}
							catch (Exception) {
								//プロセスは既に終了しているので何もしない
							}
						}
					}
				}
			}
		}

		static string position(string startsfen, List<string> kifu) {
			if (kifu.Count > 0) {
				StringBuilder sb = new StringBuilder("position ").Append(startsfen);
				sb.Append(" moves");
				foreach(string move in kifu) {
					sb.Append(" ").Append(move);
				}
				return sb.ToString();
			}
			return $"position {startsfen}";
		}

		static bool CheckRepetition(Kyokumen kyokumen, List<Kyokumen> history) {
			int count = 0;
			foreach (Kyokumen his in history) {
				if (kyokumen == his) {
					count++;
				}
			}
			return count >= 3;
		}
		static bool CheckEndless(int moves) {
			return moves > Program.drawMoves;
		}
		static (string move, int eval) GetMove(Process player, string position, string gobyoyomi) {
			int eval = 0;
			player.StandardInput.WriteLine(position);
			System.Threading.Tasks.Task.Delay(1).Wait();
			player.StandardInput.WriteLine(gobyoyomi);
			while (true) {
				if (player.HasExited) throw new System.IO.IOException("usi engine has crashed");
				string[] usi = player.StandardOutput.ReadLine()?.Split(' ');
				if (usi == null || usi.Length == 0) { continue; }
				else if (usi[0] == "info") {
					for (int i = 1; i < usi.Length - 1; i++) {
						if (usi[i] == "cp") {
							eval = int.Parse(usi[i + 1]);
							break;
						}
						else if (usi[i] == "mate") {
							if (usi[i + 1] == "+") eval = 90000;
							else if (usi[i + 1] == "-") eval = -90000;
							else {
								int mate = int.Parse(usi[i + 1]);
								if (mate >= 0) eval = 100000 - mate;
								else eval = -100000 - mate;
							}
							break;
						}
					}
				}
				else if (usi[0] == "bestmove") {
					return (usi[1], eval);
				}
			}
		}
		static void sendGameOver(Process sente,Process gote,Result result) {
			switch (result) {
				case Result.SenteWin:
					sente.StandardInput.WriteLine("gameover win");
					sente.StandardInput.WriteLine("quit");
					gote.StandardInput.WriteLine("gameover lose");
					gote.StandardInput.WriteLine("quit");
					break;
				case Result.GoteWin:
					sente.StandardInput.WriteLine("gameover lose");
					sente.StandardInput.WriteLine("quit");
					gote.StandardInput.WriteLine("gameover win");
					gote.StandardInput.WriteLine("quit");
					break;
				case Result.Draw:
				case Result.Repetition:
					sente.StandardInput.WriteLine("gameover draw");
					sente.StandardInput.WriteLine("quit");
					gote.StandardInput.WriteLine("gameover draw");
					gote.StandardInput.WriteLine("quit");
					break;
			}
		}
		static void sendOnlyGameOver(Process sente, Process gote, Result result) {
			switch (result) {
				case Result.SenteWin:
					sente.StandardInput.WriteLine("gameover win");
					gote.StandardInput.WriteLine("gameover lose");
					break;
				case Result.GoteWin:
					sente.StandardInput.WriteLine("gameover lose");
					gote.StandardInput.WriteLine("gameover win");
					break;
				case Result.Draw:
				case Result.Repetition:
					sente.StandardInput.WriteLine("gameover draw");
					gote.StandardInput.WriteLine("gameover draw");
					break;
			}
		}
		static void sendQuit(Process engine) {
			engine.StandardInput.WriteLine("quit");
		}
	}
}
