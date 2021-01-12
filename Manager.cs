using System;
using System.Threading;
using System.IO;
using System.Runtime.CompilerServices;

public class FileManager {
	protected string[] buffer = new string[1];
	protected string fileName, game;
	protected int stLine, edLine;
	public int labelLine;
	public FileManager() {} // 의도적으로 비움
	public FileManager(string _fileName, string _game) {
		fileName = _fileName;
		game = _game;
		ReadGame();
	}
	void CreateGame() {
		CryptoGrapher.ReadFile(ref buffer, fileName);
		string[] tmp = new string[buffer.Length+2];
		int i = 0;
		for (i = 0; i < buffer.Length; ++i) tmp[i] = buffer[i];
		tmp[i++] = "■"+game;
		tmp[i] = "//End";
		CryptoGrapher.WriteFile(tmp, fileName);
	}
	void ReadGame() { // stLine, edLine 알아내기
		CryptoGrapher.ReadFile(ref buffer, fileName);
		stLine = -1;
		for (int i = 0; i < buffer.Length; ++i) {
			if (buffer[i] == "■"+game) {
				stLine = i; break;
			}
		}
		if (stLine == -1) { // 해당 게임이 존재하지 않음
			stLine = buffer.Length;
			edLine = buffer.Length+1;
			CreateGame();
		}
		else {
			for (int i = stLine; i < buffer.Length; ++i) {
				if (buffer[i] == "//End") {
					edLine = i; break;
				}
			}
		}
	}
	void CreateLabel(string label, int numLine) {
		CryptoGrapher.ReadFile(ref buffer, fileName);
		int i, j;
		string[] newBuffer = new string[buffer.Length+numLine+1];

		for (i = 0; i < edLine; ++i) newBuffer[i] = buffer[i];
		newBuffer[i++] = "//"+label;
		for (; i <= edLine+numLine; ++i) newBuffer[i] = "Empty";
		newBuffer[i++] = "//End";
		j = edLine+1;
		for (; i < newBuffer.Length; ++i) newBuffer[i] = buffer[j++];
		edLine += numLine+1;
		CryptoGrapher.WriteFile(newBuffer, fileName);
	}
	protected void DeleteLabel(string label, int numLine) {
		ReadLabel(label, numLine);
		string[] newBuffer = new string[buffer.Length-numLine-1];
		int i;

		for (i = 0; i < labelLine; ++i) newBuffer[i] = buffer[i];
		for (i = labelLine+numLine+1; i < buffer.Length; ++i) {
			newBuffer[i-numLine-1] = buffer[i];
		}
		CryptoGrapher.WriteFile(newBuffer, fileName);
	}
	protected bool ReadLabel(string label, int numLine) {
		return ReadLabel(label, numLine, ref buffer);
	}
	public bool ReadLabel(string label, int numLine, ref string[] buffer) { // labelLine 알아내기
		CryptoGrapher.ReadFile(ref buffer, fileName);
		int i;
		labelLine = -1;
		for (i = stLine; i <= edLine; ++i) {
			if (buffer[i] == "//"+label) {
				labelLine = i; break;
			}
		}
		if (labelLine == -1) { // 해당 식별자가 존재하지 않음
			labelLine = edLine;
			CreateLabel(label, numLine);
			CryptoGrapher.ReadFile(ref buffer, fileName);
			return false;
		}
		return true;
	}
}
public class RecordManager : FileManager {
	const int MAXRANK = 10;
	const int MAXLEN = 9; // 기록, 유저 이름의 최대 길이
	const string FILENAME = "Record";
	public RecordManager() : base() {} // 의도적으로 비움
	public RecordManager(string game) : base(FILENAME, game) {}
	public void ShowRecord(string label, string digitMean) {
		ReadLabel(label, MAXRANK);
		Console.ResetColor();
		Console.Clear();

		const int X0 = 0, X1 = 10, X2 = 23, X3 = 34;
		Console.BackgroundColor = ConsoleColor.Yellow;
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.SetCursorPosition(X0, 0);
		Console.Write("< "+label+" >");
		Console.ResetColor();
		Console.SetCursorPosition(X0, 1);
		Console.Write("┌─────────────────────────────────┐");
		Console.SetCursorPosition(X0, 2);
		Console.Write("│");
		Console.SetCursorPosition(X3, 2);
		Console.Write("│");

		Console.SetCursorPosition(X1, 2);
		Console.Write(digitMean);
		Console.SetCursorPosition(X2, 2);
		Console.Write("아이디");
		Console.SetCursorPosition(X0, 3);
		Console.Write("├─────────────────────────────────┤");

		for (int i = labelLine+1; i <= labelLine+MAXRANK; ++i) {
			string digit, userName;
			if (buffer[i] == "Empty") {
				digit = userName = "-";
			}
			else {
				string[] res = buffer[i].Split(' ');
				digit = res[0];
				userName = "";
				if (res.Length == 2) userName = res[1];
			}
			int rank = i-labelLine;
			int j = 4+2*(i-labelLine-1);
			Console.SetCursorPosition(X0, j);
			Console.Write("│");
			Console.SetCursorPosition(X0, j+1);
			Console.Write("│");
			Console.SetCursorPosition(X0+3, j);
			if (rank < 10) Console.Write(" ");
			Console.Write("{0}.", rank);
			Console.SetCursorPosition(X1, j);
			if (digit != "-" && digitMean == "시간") {
				Console.Write(MyFunction.TimeExpression(int.Parse(digit)));
			}
			else Console.Write(digit);
			Console.SetCursorPosition(X2, j);
			Console.Write(userName);
			Console.SetCursorPosition(X3, j);
			Console.Write("│");
			Console.SetCursorPosition(X3, j+1);
			Console.Write("│");
		}
		Console.SetCursorPosition(X0, 3+2*MAXRANK);
		Console.Write("└─────────────────────────────────┘");
		Console.SetCursorPosition(50, 23);
		Console.Write("<Spacebar/Enter로 뒤로가기>");
	}
	public void UpdateRecord(string label, string digitMean, int newDigit, bool reverse) {
		ReadLabel(label, MAXRANK);
		string[] prev = new string[buffer.Length];
		int i;
		for (i = 0; i < buffer.Length; ++i) prev[i] = buffer[i];

		int curRank = -1;
		for (i = labelLine+1; i <= labelLine+MAXRANK; ++i) {
			int rank = i-labelLine;
			int digit = 0;
			if (buffer[i] != "Empty") digit = int.Parse(buffer[i].Split(' ')[0]);
			else if (reverse) digit = int.MaxValue;
			if ((!reverse && digit <= newDigit) || (reverse && digit >= newDigit)) { 
				curRank = rank; break;
			}
		}
		if (curRank == -1) return; // 랭킹 안에 들지 못함

		// base.buffer수정, stLine/edLine/labelLine이 바뀌지 않는다
		for (i = labelLine+MAXRANK; i > labelLine+curRank; --i) {
			buffer[i] = buffer[i-1];
		}
		buffer[labelLine+curRank] = newDigit.ToString();

		CryptoGrapher.WriteFile(buffer, FILENAME);
		ShowRecord(label, digitMean);
		// 아이디 입력 도중 창을 닫으면 점수를 등록하지 않는다
		CryptoGrapher.WriteFile(prev, FILENAME);

		// 유저 아이디 입력받기
		Console.SetCursorPosition(46, 23);
		Console.CursorVisible=true;
		Console.Write("<아이디 입력 후 Spacebar/Enter>");
		
		int x0 = 0, x = 23;
		int y = 4+2*(curRank-1);
		Console.BackgroundColor = ConsoleColor.Magenta;
		Console.SetCursorPosition(x0+3, y);
		if (curRank < 10) Console.Write(" ");
		Console.Write("{0}.", curRank);

		Console.ResetColor();
		Console.SetCursorPosition(x, y);
		int cur = x;
		char[] userName = new char[MAXLEN];

		while (Console.KeyAvailable) Console.ReadKey(true);
		while (true) {
			ConsoleKeyInfo cki = Console.ReadKey(true);
			string s = cki.Key.ToString();

			if (s == "Enter" || s == "Spacebar") {
				if (cur == x) continue;
				Console.CursorVisible=false;
				break;
			}
			else if (s == "Backspace") {
				if (cur == x) continue;
				Console.SetCursorPosition(--cur, y);
				Console.Write(" ");
			}
			else {
				if (s.Length > 1 || s[0] < 'A' || s[0] > 'Z') continue;
				if (cur >= x+MAXLEN) continue;
				Console.SetCursorPosition(cur, y);
				Console.Write(s);
				userName[cur-x] = s[0];
				++cur;
			}
		}
		string tmp = "";
		for (i = x; i < cur; ++i) tmp += userName[i-x];
		buffer[labelLine+curRank] += " "+tmp;
		CryptoGrapher.WriteFile(buffer, FILENAME);
	}
	public void ModifyRecord(string label, string newLabel) { // label 변경하기
		ReadLabel(label, MAXRANK);
		buffer[labelLine] = "//"+newLabel;
		CryptoGrapher.WriteFile(buffer, FILENAME);
	}
	public void DeleteRecord(string label) {
		DeleteLabel(label, MAXRANK);
	}
	public static void OverWrite() {
		const string FILENAME1 = "Record1";
		Console.ResetColor();
		Console.Clear();
		Console.SetCursorPosition(2, 1);
		Console.Write("※도움말※");
		Console.SetCursorPosition(2, 3);
		Console.Write("1. 외부에서 {0}.dll 파일을 구해옵니다", FILENAME);
		Console.SetCursorPosition(2, 4);
		Console.Write("2. 파일 이름을 {0}.dll로 바꾸고 폴더 안에 넣어주세요", FILENAME1);
		Console.SetCursorPosition(2, 5);
		Console.Write("3. 덮어쓰기가 완료되면 Record1.dll을 폴더에서 빼내주세요");
		Console.SetCursorPosition(2, 7);
		Console.Write("♠ W를 누르면 덮어쓰기를 시작합니다 ♠");
		Console.SetCursorPosition(50, 10);
		Console.Write("뒤로가기(Spacebar/Enter)");

		while (Console.KeyAvailable) Console.ReadKey(true);
		while (true) {
			ConsoleKeyInfo cki = Console.ReadKey(true);
			string s = cki.Key.ToString();
			if (s == "W") break;
			else if (s == "Spacebar" || s == "Enter") return;
		}
		// 덮어쓰기 시작
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.SetCursorPosition(2, 8);
		Console.Write("♠ 덮어쓰기중.. ♠");
		Thread.Sleep(1000);

		// 덮어쓰기 실패
		if (!File.Exists(FILENAME1+".dll")) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.SetCursorPosition(2, 8);
			Console.Write("♠ 실패: Record1.dll을 찾을 수 없습니다 ♠");
			MyFunction.WaitForExit();
			return;
		}
		// 덮어쓰기 진행
		string[] res = new string[1];
		CryptoGrapher.ReadFile(ref res, FILENAME);
		string[] buffer = new string[1];
		CryptoGrapher.ReadFile(ref buffer, FILENAME1);
		FileManager fm = new FileManager();

		for (int i = 0; i < buffer.Length; ++i) {
			if (buffer[i][0] == '■') {
				string game = buffer[i].Substring(1);
				CryptoGrapher.WriteFile(res, FILENAME);
				fm = new FileManager(FILENAME, game);
				CryptoGrapher.ReadFile(ref res, FILENAME);
			}
			else if (buffer[i] == "Empty" || buffer[i] == "//End") {
				continue;
			}
			else if (buffer[i][0] == '/') {
				string label = buffer[i].Substring(2);
				CryptoGrapher.WriteFile(res, FILENAME);
				fm.ReadLabel(label, MAXRANK, ref res);
			}
			else { // 현재 label의 점수에 대해
				string[] tmp = buffer[i].Split(' ');
				int newDigit = int.Parse(tmp[0]);
				string userName = tmp[1];
				int curRank = -1;

				for (int j = fm.labelLine+1; j <= fm.labelLine+MAXRANK; ++j) {
					int rank = j-fm.labelLine;
					int digit = 0;
					string name = "-";
					if (res[j] != "Empty") {
						string[] tmp2 = res[j].Split(' ');
						digit = int.Parse(tmp2[0]);
						name = tmp2[1];
					}
					if (digit <= newDigit) {
						if (digit != newDigit || userName != name) curRank = rank;
						break;
					}
				}
				if (curRank == -1) continue;
				for (int j = fm.labelLine+MAXRANK; j > fm.labelLine+curRank; --j) {
					res[j] = res[j-1];
				}
				res[fm.labelLine+curRank] = newDigit.ToString()+" "+userName;
			}
		}
		// 덮어쓰기 종료
		CryptoGrapher.WriteFile(res, FILENAME);
		Console.ForegroundColor = ConsoleColor.Green;
		Console.SetCursorPosition(2, 8);
		Console.Write("♠ 덮어쓰기 성공 ♠");
		MyFunction.WaitForExit();
	}
}
public class MapManager : FileManager {
	const string FILENAME = "Map";
	int w, l;
	public int cur;
	public bool quit;
	public MapManager() : base() {} // 의도적으로 비움
	public MapManager(string game, int _w, int _l) : base(FILENAME, game) {
		w = _w; l = _l; cur = 0; quit = false;
	}
	public void ShowMap(string label, int stx, int sty) {
		ShowMap(label, stx, sty, false);
	}
	public void ShowMap(string label, int stx, int sty, bool isMaking) {
		// stx, sty는 절대좌표
		// (x,y) = (1,1)~(w,l), 절대좌표는 x*2
		// buffer상에서 (x,y) = (y+y0, x+x0)
		ReadLabel(label, l);
		Console.ResetColor();
		Console.Clear();

		int p0 = stx+2*w+6, q0 = sty+3;
		if (isMaking) {
			Console.SetCursorPosition(p0, q0);
			Console.Write("게임: "+game);
			Console.SetCursorPosition(p0, q0+1);
			Console.Write("< "+label+" >");
			Console.SetCursorPosition(p0, q0+3);
			Console.Write("W: 저장하고 나가기");
			Console.SetCursorPosition(p0, q0+4);
			Console.Write("Q: 저장 안하고 나가기");
			Console.SetCursorPosition(p0, q0+5);
			Console.Write("조작: 방향키, 스페이스");
		}
		else {
			Console.SetCursorPosition(p0, q0);
			Console.Write("< "+label+" >");
			Console.SetCursorPosition(p0, q0+2);
			Console.Write("Spacebar/Enter: 시작");
			Console.SetCursorPosition(p0, q0+3);
			Console.Write("R: 랭킹 보기");
			Console.SetCursorPosition(p0, q0+4);
			Console.Write("↔: 페이지 이동");
			Console.SetCursorPosition(p0, q0+5);
			Console.Write("Q: 나가기");
		}
		int x, y;
		for (x = stx/2; x < stx/2+w+2; ++x)
			for (y = sty; y < sty+l+2; ++y)
				if (x==stx/2 || y==sty || x==stx/2+w+1 || y==sty+l+1) {
					Console.SetCursorPosition(2*x, y);
					Console.Write("▩ ");
				}
		string p = new string(' ', w);
		for (int i = labelLine+1; i <= labelLine+l; ++i) {
			if (buffer[i] == "Empty") buffer[i] = p;
			Console.SetCursorPosition(stx+2, sty+i-labelLine);
			string q = "";
			for (int j = 0; j < buffer[i].Length; ++j) {
				if (buffer[i][j] == ' ') q += "  ";
				else q +="■";
			}
			Console.Write(q);
		}
	}
	public void RenameMap(string label, string newLabel) { // Map상의 label 수정
		bool res = ReadLabel(label, l);
		if (!res) {DeleteMap(label); return;}
		buffer[labelLine] = "//"+newLabel;
		CryptoGrapher.WriteFile(buffer, FILENAME);
		// Record상의 label 수정
		RecordManager r = new RecordManager(game);
		r.ModifyRecord(label, newLabel);
	}
	public void ModifyMap(string label) {
		ShowMap(label, 0, 0, true);
		bool save = false;
		// (x,y) = (1,1)~(w,l), 절대좌표는 x*2
		// buffer상에서 (x,y) = (y+y0, x+x0)
		int x0 = -1, y0 = labelLine;
		int x = 1, y = 1;

		Console.SetCursorPosition(2, 1);
		if (buffer[1+labelLine][0] == '■') {
			Console.BackgroundColor = ConsoleColor.Red;
			Console.Write("■");
		}
		else {
			Console.BackgroundColor=ConsoleColor.Green;
			Console.Write("  ");
		}
		// 키 입력받기
		while (Console.KeyAvailable) Console.ReadKey(true);
		while (true) {
			ConsoleKeyInfo cki = Console.ReadKey(true);
			string s = cki.Key.ToString();

			if (s == "W") {save = true; break;}
			else if (s == "Q") break;
			else if (s == "Spacebar") {
				Console.SetCursorPosition(x*2, y);
				if (buffer[y+y0][x+x0] == '■') {
					buffer[y+y0] = MyFunction.Substitute(buffer[y+y0], x+x0, ' ');
					Console.BackgroundColor=ConsoleColor.Green;
					Console.Write("  ");
				}
				else {
					buffer[y+y0]=MyFunction.Substitute(buffer[y+y0], x+x0, '■');
					Console.BackgroundColor=ConsoleColor.Red;
					Console.Write("■");
				}
			}
			else if (s.Contains("Arrow")) {
				int nx = x, ny = y;
				if (s == "UpArrow") --ny;
				else if (s == "DownArrow") ++ny;
				else if (s == "LeftArrow") --nx;
				else ++nx;

				if (nx<1 || nx>w || ny<1 || ny>l) continue;
				Console.SetCursorPosition(x*2, y);
				Console.BackgroundColor=ConsoleColor.Black;
				if (buffer[y+y0][x+x0]=='■') Console.Write("■");
				else Console.Write("  ");

				x = nx; y = ny;
				Console.SetCursorPosition(x*2, y);
				if (buffer[y+y0][x+x0]=='■') {
					Console.BackgroundColor=ConsoleColor.Red;
					Console.Write("■");
				}
				else {
					Console.BackgroundColor=ConsoleColor.Green;
					Console.Write("  ");
				}
			}
		}
		if (save) CryptoGrapher.WriteFile(buffer, FILENAME);
	}
	public void DeleteMap(string label) { // 관련 정보들 지우기
		DeleteLabel(label, l);
		RecordManager r = new RecordManager(game);
		r.DeleteRecord(label);
	}
	public string SelectMap(string digitMean) {
		quit = false;
		int numLabel = (edLine-stLine-1)/(1+l);
		if (numLabel == 0) {quit = true; return "";}
		// 모든 label 알아내기
		CryptoGrapher.ReadFile(ref buffer, FILENAME);
		string[] labels = new string[numLabel];
		for (int i = 0; i < numLabel; ++i) {
			int labelLine = stLine+1+i*(1+l);
			labels[i] = buffer[labelLine].Substring(2);
		}
		while (Console.KeyAvailable) Console.ReadKey(true);
		while (true) {
			ShowMap(labels[cur], 0, 0);
			ConsoleKeyInfo cki = Console.ReadKey(true);
			string s = cki.Key.ToString();

			if (s == "Enter" || s == "Spacebar") return labels[cur];
			else if (s == "R") {
				RecordManager r = new RecordManager(game);
				r.ShowRecord(labels[cur], digitMean);
				MyFunction.WaitForExit();
			}
			else if (s == "LeftArrow") cur = (cur-1+numLabel)%numLabel;
			else if (s == "RightArrow") cur = (cur+1)%numLabel;
			else if (s == "Q") {quit = true; return "";}
		}
	}
}