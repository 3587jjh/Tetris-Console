using System;
using System.Threading;

public class MyFunction {
	public static void Shuffle(ref int[] A) {
		Random r = new Random();
		for (int iter = 0; iter < 3; ++iter) {
			for (int i = A.Length-1; i > 0; --i) {
				int j = r.Next(0, i+1);
				int tmp = A[i]; A[i] = A[j]; A[j] = tmp;
			}
		}
	}
	public static string Substitute(string s, int start, char ch) {
		string ret = "";
		for (int i = 0; i < s.Length; ++i) {
			if (i == start) ret += ch;
			else ret += s[i];
		}
		return ret;
	}
	public static void WaitForExit() {
		while (Console.KeyAvailable) Console.ReadKey(true);
		while (true) {
			ConsoleKeyInfo cki = Console.ReadKey(true);
			string p = cki.Key.ToString();
			if (p == "Spacebar" || p == "Enter") return;
		}
	}
	public static string TimeExpression(int timeCnt) {
		// timeCnt = 초단위
		int min = timeCnt/60, sec = timeCnt%60;
		string ret = "";
		if (min < 10) ret += "0";
		ret += min.ToString() + "분 ";
		if (sec < 10) ret += "0";
		ret += sec.ToString() + "초 ";
		return ret;
	}
}
public class Pair {
	public int x, y;
	public Pair() {x = y = 0;}
	public Pair(int _x, int _y) {x = _x; y = _y;}
}
public class MenuSelector {
	int x, y;
	const int MAX = 20; // 최대 옵션 개수
	string[] option = new string[MAX];
	int numOption;
	public MenuSelector() {x = y = numOption = 0;}
	public MenuSelector(int _x, int _y) {x = _x; y = _y; numOption = 0;}
	public void Add(string s) {
		if (numOption >= MAX) return;
		option[numOption++] = s;
	}
	public int ShowMenu() {
		Console.ResetColor();
		Console.SetCursorPosition(x, y);
		Console.Write("☞");

		for (int j = y; j < y+numOption; ++j) {
			Console.SetCursorPosition(x+3, j);
			Console.Write(option[j-y]);
		}
		int curY = y, nextY = -1;
		while (Console.KeyAvailable) Console.ReadKey(true);
		while (true) {
			ConsoleKeyInfo cki = Console.ReadKey(true);
			string s = cki.Key.ToString();
			// y, y+1, ..., y+numOption
			// 0, 1, ..., numOption
			if (s == "UpArrow" || s == "DownArrow") {
				if (s=="UpArrow") nextY = (curY-y-1+numOption)%numOption+y;
				else nextY = (curY-y+1)%numOption+y;
				Console.SetCursorPosition(x, curY);
				Console.Write("  ");
				Console.SetCursorPosition(x, nextY);
				Console.Write("☞");
				curY = nextY;
			}
			else if (s == "Spacebar" || s == "Enter") break;
		}
		return curY-y;
	}
}
public class Message {
	int x, y;
	public Message() {x = y = 0;}
	public Message(int _x, int _y) {x = _x; y = _y;}
	public void ShowMessage(string s, bool remove) {
		if (remove) {
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.Black;
		}
		else {
			Console.ForegroundColor=ConsoleColor.Blue;
			Console.BackgroundColor=ConsoleColor.Yellow;
		}
		Console.SetCursorPosition(x, y);
		Console.Write(s);
		Console.ResetColor();
	}
	public void StartCount() {
		// 시작할 때 또는 일시정지 해제 때 사용
		string s = "";
		for (int cnt = 3; cnt >= 1; --cnt) {
			s = (char)(cnt+'0')+"초 후 진행";
			ShowMessage(s, false);
			Thread.Sleep(1000);
		}
		ShowMessage(s, true);
		while (Console.KeyAvailable) Console.ReadKey(true);
	}
}