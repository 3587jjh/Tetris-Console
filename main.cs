using System;
using System.Threading;
using System.IO;

public class SecretProject {
	public SecretProject() {
		if (LocalTest()) {
			ShowProgram();
		}
	}
	bool LocalTest() {
		Console.ResetColor();
		Console.Clear();
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.SetCursorPosition(1, 1);
		Console.Write("로딩중..");
		Thread.Sleep(1000);

		const string want1 = @"C:\Windows\새 텍스트 문서.txt";
		const string want2 = @"Record.dll";
		const string want3 = @"Map.dll";
		Console.SetCursorPosition(1, 1);

		if (File.Exists(want2) && File.Exists(want3)) { // && File.Exists(want1)
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write("로딩 성공");
			Thread.Sleep(1000);
			return true;
		}
		Console.ForegroundColor = ConsoleColor.Red;
		if (!File.Exists(want1)) Console.Write("로딩 실패: 실행이 불가한 PC입니다");
		else if (!File.Exists(want2)) Console.Write("로딩 실패: {0}을 찾을 수 없습니다", want2);
		else Console.Write("로딩 실패: {0}을 찾을 수 없습니다", want3);
		MyFunction.WaitForExit();
		return false;
	}
	void ShowProgram() {
		while (true) {
			Console.ResetColor();
			Console.Clear();
			Console.SetCursorPosition(2, 1);
			Console.Write("■모음집 (ver.190101)■");

			MenuSelector m = new MenuSelector(2, 3);
			m.Add("테트리스");
			m.Add("사다리 타기");
			m.Add("외부 기록 덮어쓰기");
			m.Add("종료");

			int res = m.ShowMenu();
			if (res == 0) {Tetris t = new Tetris();}
			else if (res == 1) {ClimbLadder c = new ClimbLadder();}
			else if (res == 2) {RecordManager.OverWrite();}
			else break;
		}
	}
}
public class MainProgram {
	public static void Main() {
		Console.CursorVisible = false;
		// MapManager m = new MapManager("Tetris", 10, 20);
		// m.ModifyMap("악마의 눈물");
		SecretProject myProject = new SecretProject();
	}
}