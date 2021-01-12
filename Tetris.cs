using System;
using System.Threading;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.ComponentModel;
using System.Data;
using System.Globalization;

public class Tetris {
	const string GAMENAME = "Tetris";
	const int W = 10, L = 20; // 테트리스 가로, 세로
	const int EXTRA = 100; // 테트리스 세로로 안보이는 추가 4칸
	const int STX = 16, STY = 4; // 필드 시작 위치
	const int BSIZE = 4; // 블록 사이즈
	const int ONELINE = 1000; // 한 줄당 점수
	const int ADDLINE = 200; // 줄당 추가 점수
	const double LEVELBONUS = 1.05; // 레벨업당 배율 상승률
	const double COMBOBONUS = 1.2; // 콤보당 배율 상승률

	// Wall Kick Test Data
	const int NUMTEST = 5;
	// ㅡ 블록(시계)
	static int[,,] KICKDATA1a = {
		{{0,0},{1,0},{-2,0},{1,2},{-2,-1}},
		{{0,0},{-2,0},{1,0},{-2,1},{1,-2}},
		{{0,0},{-1,0},{2,0},{-1,-2},{2,1}},
		{{0,0},{2,0},{-1,0},{2,-1},{-1,2}}
	};
	// ㅡ 블록(반시계)
	static int[,,] KICKDATA1b = {
		{{0,0},{-1,0},{2,0},{-1,2},{2,-1}},
		{{0,0},{-2,0},{1,0},{-2,-1},{1,2}},
		{{0,0},{1,0},{-2,0},{1,-2},{-2,1}},
		{{0,0},{2,0},{-1,0},{2,1},{-1,-2}}
	};
	// ㅡ,ㅁ를 제외한 모든 블록(시계)
	static int[,,] KICKDATA2a = {
		{{0,0},{-1,0},{-1,1},{0,-2},{-1,-2}},
		{{0,0},{-1,0},{-1,-1},{0,2},{-1,2}},
		{{0,0},{1,0},{1,1},{0,-2},{1,-2}},
		{{0,0},{1,0},{1,-1},{0,2},{1,2}}
	};
	// ㅡ,ㅁ를 제외한 모든 블록(반시계)
	static int[,,] KICKDATA2b = {
		{{0,0},{1,0},{1,1},{0,-2},{1,-2}},
		{{0,0},{-1,0},{-1,-1},{0,2},{-1,2}},
		{{0,0},{-1,0},{-1,1},{0,-2},{-1,-2}},
		{{0,0},{1,0},{1,-1},{0,2},{1,2}}
	};
	public Tetris() {ProgramStart p = new ProgramStart();}
	class Block {
		public int x, y; // 콘솔상에서 절대위치
		public int type, curRot;
		public int[,,] state = new int[4, BSIZE, BSIZE]; // 회전상태, 블록x, 블록y
		public bool trans;
		public Block() {} // 의도적으로 비움
		public Block(int type) {
			curRot = 0; this.type = type;
			x = STX+6; y = STY-2; trans = false;

			if (type==0) { // ㅗ
				state[0,0,1]=state[0,1,0]=state[0,1,1]=state[0,2,1]=1;
				state[1,1,0]=state[1,1,1]=state[1,1,2]=state[1,2,1]=1;
				state[2,0,1]=state[2,1,1]=state[2,1,2]=state[2,2,1]=1;
				state[3,0,1]=state[3,1,0]=state[3,1,1]=state[3,1,2]=1;
			}
			else if (type==1) { // ㅁ
				for (int i = 0; i < 4; ++i) {
					state[i,1,0]=state[i,2,0]=state[i,1,1]=state[i,2,1]=1;
				}
			}
			else if (type==2) { // ㅡ
				state[0,0,1]= state[0,1,1]=state[0,2,1]=state[0,3,1]=1;
				state[1,2,0]= state[1,2,1]=state[1,2,2]=state[1,2,3]=1;
				state[2,0,2]= state[2,1,2]=state[2,2,2]=state[2,3,2]=1;
				state[3,1,0]= state[3,1,1]=state[3,1,2]=state[3,1,3]=1;
			}
			else if (type==3) { // 뱀모양
				state[0,0,1]= state[0,1,0]=state[0,1,1]=state[0,2,0]=1;
				state[1,1,0]= state[1,1,1]=state[1,2,1]=state[1,2,2]=1;
				state[2,0,2]= state[2,1,1]=state[2,1,2]=state[2,2,1]=1;
				state[3,0,0]= state[3,0,1]=state[3,1,1]=state[3,1,2]=1;
			}
			else if (type==4) { // 뱀모양 반대
				state[0,0,0]= state[0,1,0]=state[0,1,1]=state[0,2,1]=1;
				state[1,1,1]= state[1,1,2]=state[1,2,0]=state[1,2,1]=1;
				state[2,0,1]= state[2,1,1]=state[2,1,2]=state[2,2,2]=1;
				state[3,0,1]= state[3,0,2]=state[3,1,0]=state[3,1,1]=1;
			}
			else if (type==5) { // ㄴ
				state[0,0,0]= state[0,0,1]=state[0,1,1]=state[0,2,1]=1;
				state[1,1,0]= state[1,1,1]=state[1,1,2]=state[1,2,0]=1;
				state[2,0,1]= state[2,1,1]=state[2,2,1]=state[2,2,2]=1;
				state[3,0,2]= state[3,1,0]=state[3,1,1]=state[3,1,2]=1;
			}
			else { // ㄴ 반대
				state[0,0,1]= state[0,1,1]=state[0,2,0]=state[0,2,1]=1;
				state[1,1,0]= state[1,1,1]=state[1,1,2]=state[1,2,2]=1;
				state[2,0,1]= state[2,0,2]=state[2,1,1]=state[2,2,1]=1;
				state[3,0,0]= state[3,1,0]=state[3,1,1]=state[3,1,2]=1;
			}
		}
		public void SetColor() {
			if (type==0) Console.ForegroundColor = ConsoleColor.DarkMagenta;
			else if (type==1) Console.ForegroundColor=ConsoleColor.Yellow;
			else if (type==2) Console.ForegroundColor=ConsoleColor.Cyan;
			else if (type==3) Console.ForegroundColor=ConsoleColor.Green;
			else if (type==4) Console.ForegroundColor=ConsoleColor.Red;
			else if (type==5) Console.ForegroundColor=ConsoleColor.DarkCyan;
			else Console.ForegroundColor=ConsoleColor.White;
		}
	}
	class Field {
		const int INITSPEED = 1000;
		const int INITLINE = 15;
		public int[,] state = new int[W, L+EXTRA];
		// 레벨, 남은 줄 수, 방금없앤 줄 수, 블록 떨어지는 속도, 콤보
		public int level, remainLine, curLine, speed, combo;
		bool canLevelUp; // 레벨업이 허용됐는가
		public double baseBonus, bonus, score;
		public int backColor;
		public string mode, mapName;

		public Field() {} // 의도적으로 비움
		public Field(string _mode) {
			level = 0; remainLine = INITLINE;
			curLine = 0; speed = INITSPEED; combo = 0;
			canLevelUp = true;
			baseBonus = 1; bonus = 1; score = 0;
			mode = _mode;
		}
		public bool CanPut(Block b) {
			for (int i = 0; i < BSIZE; ++i)
				for (int j = 0; j < BSIZE; ++j) {
					if (b.state[b.curRot,i,j] == 0) continue;
					int cx = b.x+2*i, cy = b.y+j;
					if (cx < STX || cx >= STX+2*W || cy >= STY+L ||
						state[(cx-STX)/2, cy-STY+EXTRA] > 0) {
						return false;
					}
				}
			return true;
		}
		public void Put(Block b, bool remove) {
			if (remove) Console.ForegroundColor = ConsoleColor.Black;
			else if (b.trans) Console.ForegroundColor=ConsoleColor.Gray;
			else b.SetColor();
			for (int i = 0; i<BSIZE; ++i)
				for (int j = 0; j<BSIZE; ++j) {
					if (b.state[b.curRot,i,j] == 0) continue;
					int cx = b.x+2*i, cy = b.y+j;
					if (cx >= STX && !b.trans) {
						state[(cx-STX)/2, cy-STY+EXTRA] = (int)Console.ForegroundColor;
					}
					if (cy >= STY) {
						Console.SetCursorPosition(cx, cy);
						if (!b.trans) Console.Write("■");
						else Console.Write("▒ ");
					}
				}
		}
		int RemoveLine() {
			// 없앤 줄 수 반환
			int x, y, uy, cnt;
			int ret = 0;
			for (y = L+EXTRA-1; y >= 0; --y) {
				cnt = 0;
				for (x = 0; x < W; ++x) {
					if (state[x,y] > 0) ++cnt;
				}
				if (cnt == W) break;
			}
			uy = y;
			if (y < 0) return 0;
			while (uy >= 0) {
				cnt = 0;
				for (x = 0; x < W; ++x) {
					if (state[x,uy] > 0) ++cnt;
				}
				if (cnt < W) {
					for (x = 0; x < W; ++x) {
						state[x,y] = state[x,uy];
					}
					--y;
				}
				else ++ret;
				--uy;
			}
			for (uy = 0; uy <= y; ++uy)
				for (x = 0; x < W; ++x)
					state[x,uy] = 0;
			return ret;
		}
		public void Update() {
			// 줄 없애기, 방금 없앤 줄 수 갱신
			curLine = RemoveLine();
			// 점수 갱신
			score += (ONELINE + ADDLINE*(curLine-1)) * curLine * bonus;
			// 콤보 갱신
			if (curLine > 0) ++combo; else combo = 0;
			// 레벨업까지 남은 줄 수 갱신, 레벨업
			if (canLevelUp) {
				remainLine -= curLine;
				if (remainLine <= 0) {
					baseBonus *= LEVELBONUS;
					if (level < 15) speed -= 60;
					else if (level < 20) speed -= 12;
					else if (level < 50) speed -= 1;
					++level; remainLine += INITLINE;
				}
			}
			// 점수 배율 갱신
			if (combo == 0) bonus = baseBonus;
			else bonus *= COMBOBONUS;
		}
		public Block GetTrans(Block b) {
			Block ret = new Block(b.type);
			ret.x = b.x; ret.y = b.y; ret.curRot = b.curRot;
			ret.trans = true;
			while (CanPut(ret)) ++ret.y;
			--ret.y;
			return ret;
		}
		void KickWall(ref Block b, bool clockwise) {
			if (b.type == 1) return; // ㅁ
			for (int t = 0; t < NUMTEST; ++t) {
				Block nb = new Block(b.type);
				nb.curRot = b.curRot;

				if (b.type == 2) { // ㅡ
					if (clockwise) {
						nb.x=b.x+2*KICKDATA1a[b.curRot,t,0];
						nb.y=b.y+KICKDATA1a[b.curRot, t, 1];
					}
					else {
						nb.x=b.x+2*KICKDATA1b[b.curRot, t, 0];
						nb.y=b.y+KICKDATA1b[b.curRot, t, 1];
					}
				}
				else {
					if (clockwise) {
						nb.x=b.x+2*KICKDATA2a[b.curRot, t, 0];
						nb.y=b.y+KICKDATA2a[b.curRot, t, 1];
					}
					else {
						nb.x=b.x+2*KICKDATA2b[b.curRot, t, 0];
						nb.y=b.y+KICKDATA2b[b.curRot, t, 1];
					}
				}
				if (CanPut(nb)) {b = nb; return;}
			}
		}
		public bool BlockMove(ref Block b, string s) {
			// 임시로 필드 state상에서 b의 존재를 지우기 (화면에서는 그대로)
			for (int i = 0; i<BSIZE; ++i)
				for (int j = 0; j<BSIZE; ++j) {
					if (b.state[b.curRot,i,j] == 0) continue;
					int cx = b.x+2*i, cy = b.y+j;
					if (cx >= STX) state[(cx-STX)/2, cy-STY+EXTRA] = 0;
				}
			Block nb = new Block(b.type);
			nb.x = b.x; nb.y = b.y; nb.curRot = b.curRot;
			
			if (s == "LeftArrow") nb.x -= 2;
			else if (s == "RightArrow") nb.x += 2;
			else if (s == "DownArrow") nb.y += 1;
			else if (s == "Spacebar") nb.y = GetTrans(b).y;
			else if (s == "UpArrow" || s == "X") { // 시계방향 회전
				nb.curRot = (nb.curRot+1)%4;
				KickWall(ref nb, true);
			}
			else { // s == "Z", 반시계방향 회전
				nb.curRot=(nb.curRot+3)%4;
				KickWall(ref nb, false);
			}

			if (CanPut(nb)) {
				Put(b, true);
				if (s != "DownArrow" && s != "Spacebar") Put(GetTrans(b), true);
				b = nb;
				if (s!="DownArrow"&&s!="Spacebar") Put(GetTrans(b), false);
				Put(b, false);
				return true;
			}
			else {
				// state상에서 b를 다시 나타내기
				b.SetColor();
				for (int i = 0; i<BSIZE; ++i)
					for (int j = 0; j<BSIZE; ++j) {
						if (b.state[b.curRot, i, j]==0) continue;
						int cx = b.x+2*i, cy = b.y+j;
						if (cx>=STX) state[(cx-STX)/2, cy-STY+EXTRA] = 
							(int)Console.ForegroundColor;
					}
				return false;
			}
		}
	}
	class BlockMaker {
		const int NUMTYPE = 7; // 블록 종류 수
		const int NUMCAND = 3; // 대기 블록 수
		int[] nextCand = new int[NUMCAND];
		int[] group = new int[NUMTYPE];
		int nIdx, gIdx;
		public BlockMaker() {
			for (int i = 0; i < group.Length; ++i) group[i] = i;
			MyFunction.Shuffle(ref group);
			for (int i = 0; i < NUMCAND; ++i) nextCand[i] = group[i];
			nIdx = gIdx = 0;
		}
		void DrawNext(int blockType, int nth) {
			Block b = new Block(blockType);
			b.x = STX + 2*W+2;
			b.y = STY + nth*BSIZE;
			for (int i = 0; i < BSIZE; ++i)
				for (int j = 0; j+1 < BSIZE; ++j) {
					Console.SetCursorPosition(b.x+2*i, b.y+j);
					if (b.state[b.curRot,i,j] == 1) b.SetColor();
					else Console.ForegroundColor = 0;
					Console.Write("■");
				}
		}
		public void Init() {
			// Next네모에 후보블록 출력
			for (int i = 0; i < NUMCAND; ++i)
				DrawNext(nextCand[i], i);
			gIdx = NUMCAND;
		}
		public Block Next() {
			Block ret = new Block(nextCand[nIdx]);
			nextCand[nIdx] = group[gIdx];
			for (int i = 0; i < NUMCAND; ++i)
				DrawNext(nextCand[(nIdx+(i+1)%NUMCAND)%NUMCAND], i);
			nIdx = (nIdx+1)%NUMCAND;
			gIdx = (gIdx+1)%NUMTYPE;
			if (gIdx == 0) MyFunction.Shuffle(ref group);
			return ret;
		}
	}
	class GameStart {
		const int STX0 = STX + 2*W+13, STY0 = STY+7; // 정보창 시작위치
		const string S1 = "플레이 시간: ";
		const string S2 = "레벨: ";
		const string S3 = "남은 줄 수: ";
		const string S4 = "콤보: ";
		const string S5 = "점수 배율: ";
		const string S6 = "점수: ";
		const int LEN1 = 13, LEN2 = 6, LEN3 = 12;
		const int LEN4 = 6, LEN5 = 11, LEN6 = 6;

		// 타이머 쿨타임동안 최대 몇 개의 다른 타이머가 시작될지
		const int MAX = 100;
		bool[] dropTimerEnd = new bool[MAX+1];
		bool trashTimerEnd;
		bool[] lockTimerEnd = new bool[MAX+1];
		bool playTimerEnd;
		int nextTimer;
		Field f;
		int timeCnt;
		public GameStart() {} // 의도적으로 비움
		public GameStart(Field _f) {
			f = _f;
			timeCnt = 0;
			trashTimerEnd = true;
			playTimerEnd = true;
			StartThread();
		}
		public static void DrawOutline1(int color) {
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = (ConsoleColor)color;
			// 테두리
			for (int i = STX-2; i <= STX+2*W; i += 2)
				for (int j = STY-1; j <= STY+L; j += 1) {
					Console.SetCursorPosition(i, j);
					if (i == STX-2 && j == STY-1) Console.Write("┌ ");
					else if (i == STX+2*W && j == STY-1) Console.Write("┐ ");
					else if (i == STX-2 && j == STY+L) Console.Write("└ ");
					else if (i == STX+2*W && j == STY+L) Console.Write("┘ ");
					else {
						if (j == STY-1 || j == STY+L) Console.Write("──");
						else if (i == STX-2 || i == STX+2*W) Console.Write("│ ");
					}
				}
			// next상자
			Console.SetCursorPosition(STX+2*W, STY-1);
			Console.Write("┬── Next──┐ ");
			for (int j = STY; j+1 < STY+BSIZE; ++j) {
				Console.SetCursorPosition(STX+2*W+2*BSIZE+2, j);
				Console.Write("│ ");
			}
			Console.SetCursorPosition(STX+2*W, STY+BSIZE-1);
			Console.Write("├─────────┤ ");
			for (int j = STY+BSIZE; j+1 < STY+2*BSIZE; ++j) {
				Console.SetCursorPosition(STX+2*W+2*BSIZE+2, j);
				Console.Write("│ ");
			}
			Console.SetCursorPosition(STX+2*W, STY+2*BSIZE-1);
			Console.Write("├─────────┤ ");
			for (int j = STY+2*BSIZE; j+1<STY+3*BSIZE; ++j) {
				Console.SetCursorPosition(STX+2*W+2*BSIZE+2, j);
				Console.Write("│ ");
			}
			Console.SetCursorPosition(STX+2*W, STY+3*BSIZE-1);
			Console.Write("├─────────┘ ");

			// 홀드 상자
			Console.SetCursorPosition(STX-2*BSIZE-4, STY-1);
			Console.Write("┌── Hold──┬ ");
			for (int j = STY; j+1 < STY+BSIZE; ++j) {
				Console.SetCursorPosition(STX-2*BSIZE-4, j);
				Console.Write("│ ");
			}
			Console.SetCursorPosition(STX-2*BSIZE-4, STY+BSIZE-1);
			Console.Write("└─────────┤ ");
		}
		void DrawOutline2() {
			// 게임 정보
			Console.ResetColor();
			if (f.mode != "그림없애기") {
				Console.SetCursorPosition(STX0, STY0);
				Console.Write("모드: {0}", f.mode);
				Console.SetCursorPosition(STX0, STY0+2);
				Console.Write(S1);
				Console.SetCursorPosition(STX0, STY0+3);
				Console.Write(S2);
				Console.SetCursorPosition(STX0, STY0+4);
				Console.Write(S3);
				Console.SetCursorPosition(STX0, STY0+5);
				Console.Write(S4);
				Console.SetCursorPosition(STX0, STY0+6);
				Console.Write(S5);
				Console.SetCursorPosition(STX0, STY0+7);
				Console.Write(S6);
			}
			else {
				Console.SetCursorPosition(STX0, STY0);
				Console.Write("맵: {0}", f.mapName);
				Console.SetCursorPosition(STX0, STY0+2);
				Console.Write(S1);
			}
			// 키 도움말
			Console.SetCursorPosition(STX0, STY0+9);
			Console.Write("C: 홀드");
			Console.SetCursorPosition(STX0, STY0+10);
			Console.Write("P: 일시정지 / 해제");
			Console.SetCursorPosition(STX0, STY0+11);
			Console.Write("Q: 종료");
		}
		static void DrawField(Field myField) {
			for (int x = 0; x < W; ++x)
				for (int y = 0; y < L; ++y) {
					Console.ForegroundColor = (ConsoleColor)myField.state[x, y+EXTRA];
					Console.SetCursorPosition(STX+2*x, STY+y);
					Console.Write("■");
				}
		}
		void UpdateDisplay() {
			// 시간만 화면에 재출력
			Console.ResetColor();
			Console.SetCursorPosition(STX0+LEN1, STY0+2);
			Console.Write(MyFunction.TimeExpression(timeCnt));
		}
		bool UpdateDisplay(bool trashExist, int hole) {
			// 줄이 없어졌을 때 변한 정보를 전체적으로 재출력
			Console.ResetColor();
			f.Update();
			if (f.mode != "그림없애기") {
				Console.SetCursorPosition(STX0+LEN2, STY0+3);
				Console.Write(f.level);
				Console.SetCursorPosition(STX0+LEN3, STY0+4);
				Console.Write("{0} ", f.remainLine);
				Console.SetCursorPosition(STX0+LEN4, STY0+5);
				Console.Write(f.combo);
				Console.SetCursorPosition(STX0+LEN6, STY0+7);
				Console.Write((int)f.score);
				// 배율은 둘째자리 까지만 출력
				int tmp = (int)(f.bonus*100);
				Console.SetCursorPosition(STX0+LEN5, STY0+6);
				Console.Write("{0}.", tmp/100);
				if (tmp%100 < 10) Console.Write(0);
				Console.Write("{0} ", tmp%100);
			}
			bool ret = true;
			if (trashExist) {
				int i, j;
				// 이미 끝까지 차있어서 쓰레기줄 생성시 짤리는 경우
				for (i = 0; i < W; ++i) {
					if (f.state[i, EXTRA] > 0) ret= false;
				}
				// 한 칸씩 위로 올리기
				for (i = 0; i < W; ++i)
					for (j = 1; j < L+EXTRA; ++j)
						f.state[i,j-1] = f.state[i,j];
				// 맨 아래에 쓰레기줄 생성
				for (i = 0; i < W; ++i)
					f.state[i,L+EXTRA-1] = (int)ConsoleColor.Gray;
				f.state[hole,L+EXTRA-1] = 0;
			}
			DrawField(f);
			return ret;
		}
		void PlayTimer() {
			Thread.Sleep(1000);
			playTimerEnd = true;
		}
		void DropTimer() {
			// nextTimer가 Sleep도중에 바뀌는 것을 방지
			int tmp = nextTimer;
			Thread.Sleep(f.speed);
			dropTimerEnd[tmp] = true;
		}
		void TrashTimer() {
			if (f.level < 10) Thread.Sleep(15000);
			else if (f.level<20) Thread.Sleep(12000);
			else Thread.Sleep(9000);
			trashTimerEnd = true;
		}
		void LockTimer() {
			int tmp = nextTimer;
			Thread.Sleep(500);
			lockTimerEnd[tmp] = true;
		}
		bool LockDelay(Block b) {
			// b가 더 내려갈 곳이 없는지 판단
			f.Put(b, true);
			f.Put(f.GetTrans(b), true);
			++b.y;
			bool ret = !f.CanPut(b);
			--b.y;
			f.Put(f.GetTrans(b), false);
			f.Put(b, false);
			return ret;
		}
		void StartThread() {
			Message msg = new Message(1, 1);
			Block cur;
			BlockMaker rng = new BlockMaker();
			Random r = new Random();
			int hole = r.Next(0, W);

			Console.ResetColor();
			Console.Clear();
			DrawOutline1(f.backColor);
			DrawOutline2();
			rng.Init();
			UpdateDisplay(); // timeCnt = 0
			UpdateDisplay(false, -1);
			msg.StartCount();

			// 게임 스타트
			bool wantNext = true; // 다음블럭을 가져올 것인가
			bool holdUsed = false; // 게임에서 홀드가 한 번이라도 사용됐는가 (홀드창 블럭 유무)
			bool canHold = true; // 현재 낙하 사이클에 홀드를 쓸 수 있는가 (연속은 금지됨)
			bool curHeld = false; // 현재 블록에 대해 홀드를 작동 시켰는가
			Block hBlock = new Block(1); // 홀드된 블럭
			Block tmp = new Block(1); // cur과 hBlock을 swap하기 위한 임시 객체
			bool gameOver = false;
			bool gameClear = false;
			nextTimer = 1;

			while (true) {
				if (wantNext) cur = rng.Next();
				else cur = new Block(tmp.type);
				f.Put(f.GetTrans(cur), false);
				f.Put(cur, false);

				while (true) { // 현재 블록 cur이 놓일 때까지
					Thread A;
					bool isLockDelay = LockDelay(cur);
					bool recentMove = false;
					string recentS = "";

					if (isLockDelay) {
						A = new Thread(LockTimer);
						lockTimerEnd[nextTimer] = false;
					}
					else {
						A = new Thread(DropTimer);
						dropTimerEnd[nextTimer] = false;
					}
					A.Start();
					while ((!isLockDelay && !dropTimerEnd[nextTimer]) ||
						(isLockDelay && !lockTimerEnd[nextTimer])) {
						// 블록이 한 칸 떨어지기 전 동안 유저의 키 입력에 따른 블록 이동
						if (playTimerEnd) {
							++timeCnt;
							UpdateDisplay();
							Thread T = new Thread(PlayTimer);
							playTimerEnd = false;
							T.Start();
						}
						if (!Console.KeyAvailable) continue;
						ConsoleKeyInfo cki = Console.ReadKey(true); 
						string s = cki.Key.ToString();

						if (s == "P") { // 일시 정지
							msg.ShowMessage("일시정지", false);
							while (true) {
								ConsoleKeyInfo cki2 = Console.ReadKey(true);
								string s2 = cki2.Key.ToString();
								if (s2 == "P") {
									msg.ShowMessage("일시정지", true);
									msg.StartCount();
									break;
								}
							}
						}
						else if (s == "Q") { // 메뉴로 돌아가기
							return;
						}
						else if (s == "C") { // 홀드
							if (!canHold) continue;
							canHold = false; curHeld = true;
							f.Put(hBlock, true);
							tmp = new Block(hBlock.type);
							hBlock = new Block(cur.type);
							hBlock.x = STX-2*BSIZE-2;
							hBlock.y = STY;
							f.Put(hBlock, false);

							if (!holdUsed) {
								holdUsed = true;
								wantNext = true;
							}
							else wantNext = false;
							break;
						}
						else if (s.Contains("Arrow") || s == "Spacebar" || s == "Z" || s == "X") {
							int prev = cur.y;
							recentMove = f.BlockMove(ref cur, s);
							recentS = s;
							// Lock Delay중 움직였을 때 떨어질 곳이 생기는 경우
							if (isLockDelay && !LockDelay(cur)) break;
							// Lock Delay가 아닌 도중 움직인 직후에 Lock Delay가 걸려야 하는 경우
							if (!isLockDelay && LockDelay(cur)) {
								isLockDelay = true; break;
							}
							// Lock Delay중 다음 키를 눌렀을 때는 바로 블록이 놓여지게 함
							if (s == "Spacebar") break;
						}
					}
					nextTimer = nextTimer%MAX+1;
					if (!curHeld) {
						// lockTimer 또는 dropTimer가 다 됨
						if (recentS != "Spacebar" && isLockDelay && recentMove) {
							continue; // Lock Delay 시간 초기화
						}
						int prev = cur.y;
						f.BlockMove(ref cur, "DownArrow");
						if (cur.y == prev) {
							wantNext = true;
							canHold = true;
							break;
						}
					}
					else { // 기존에 있던 cur을 필드 state상에서 지우기
						f.Put(cur, true);
						f.Put(f.GetTrans(cur), true);
						break;
					}
				}
				// 현재 블록에 대한 처리 완료
				// 다음 스폰될 블록이 홀드 블록이라면 게임오버 또는 업데이트 진행x
				if (curHeld) {curHeld = false; continue;}
				// 게임오버 검사
				gameOver = false;
				if (f.mode == "어려움" && trashTimerEnd) {
					if (r.Next(0, 9876)%13 <= 5) hole = r.Next(0, W);
					bool res = UpdateDisplay(true, hole);
					if (!res) {gameOver = true; break;} // 게임 오버
					trashTimerEnd = false;
					Thread B = new Thread(TrashTimer);
					B.Start();
				}
				else {
					UpdateDisplay(false, -1);
					for (int i = 0; i < W; ++i)
						if (f.state[i, EXTRA-1] > 0)
							gameOver = true;
					if (gameOver) break;
				}
				// 게임 클리어 검사 (그림없애기 모드만 해당)
				if (f.mode != "그림없애기") continue;
				gameClear = true;
				for (int i = 0; i < W; ++i)
					for (int j = 0; j < L+EXTRA; ++j)
						if (f.state[i,j] == (int)ConsoleColor.Gray) 
							gameClear = false;
				if (gameClear) break;
			}
			// 게임이 terminate 되었음
			if (f.mode == "그림없애기" && gameClear) { // 게임 클리어 안내
				msg.ShowMessage("게임클리어", false);
				Thread.Sleep(1000);
				msg.ShowMessage("▶ 게임클리어(Spacebar/Enter로 진행)", false);
				MyFunction.WaitForExit();
				RecordManager rm = new RecordManager(GAMENAME);
				rm.UpdateRecord(f.mapName, "시간", timeCnt, true);
			}
			else { // 게임 오버 안내
				msg.ShowMessage("게임오버", false);
				Thread.Sleep(1000);
				msg.ShowMessage("▶ 게임오버(Spacebar/Enter로 진행)", false);
				MyFunction.WaitForExit();
				if (f.mode == "그림없애기") return;
				RecordManager rm = new RecordManager(GAMENAME);
				rm.UpdateRecord(f.mode, "점수", (int)f.score, false);
			}
		}
	}
	class ProgramStart {
		int backColor;
		public ProgramStart() {
			backColor = 2; // 디폴트 배경색
			while (true) { // 메뉴 선택창
				Console.ResetColor();
				Console.Clear();
				Console.SetCursorPosition(2, 1);
				Console.Write("★테트리스★");

				MenuSelector m = new MenuSelector(2, 3);
				m.Add("시작");
				m.Add("랭킹");
				m.Add("도움말");
				m.Add("테두리색 변경");
				m.Add("종료");

				int res = m.ShowMenu();
				if (res == 0) {
					Console.Clear();
					Console.SetCursorPosition(2, 1);
					Console.Write("※모드 선택※");
					MenuSelector m2 = new MenuSelector(2, 3);
					m2.Add("표준");
					m2.Add("어려움");
					m2.Add("그림없애기");
					m2.Add("취소");

					int res2 = m2.ShowMenu();
					if (res2 == 0) {Option1("표준");}
					else if (res2 == 1) {Option1("어려움");}
					else if (res2 == 2) {Option1("그림없애기");}
					else continue;
				}
				else if (res == 1) {Option2();}
				else if (res == 2) {Option3();}
				else if (res == 3) {Option4();}
				else break;
			}
		}
		void Option1(string mode) {
			if (mode != "그림없애기") {
				Field myField = new Field(mode);
				myField.backColor = backColor;
				GameStart g = new GameStart(myField);
			}
			else { // mode = "그림없애기"
				MapManager m = new MapManager(GAMENAME, W, L);
				while (true) {
					Field myField = new Field(mode);
					myField.backColor = backColor;
					
					string mapName = m.SelectMap("시간");
					if (m.quit) return;
					myField.mapName = mapName;

					// Field에 map정보를 불러오기
					string[] buffer = new string[1];
					m.ReadLabel(mapName, L, ref buffer);
					for (int i = m.labelLine+1; i <= m.labelLine+L; ++i) {
						for (int j = 0; j < W; ++j) {
							if (buffer[i][j] == '■') {
								myField.state[j, i-m.labelLine-1+EXTRA] = (int)ConsoleColor.Gray;
							}
						}
					}
					GameStart g = new GameStart(myField);
				}
			}
		}
		void Option2() { // 랭킹
			RecordManager r = new RecordManager(GAMENAME);
			string mode = "표준";
			r.ShowRecord("표준", "점수");
			Console.SetCursorPosition(50, 22);
			Console.Write("<방향키로 페이지 넘기기>");

			while (Console.KeyAvailable) Console.ReadKey(true);
			while (true) {
				ConsoleKeyInfo cki = Console.ReadKey(true);
				string p = cki.Key.ToString();
				if (p == "LeftArrow" || p == "RightArrow") {
					if (mode == "표준") mode = "어려움";
					else mode = "표준";
					r.ShowRecord(mode, "점수");
					Console.SetCursorPosition(50, 22);
					Console.Write("<방향키로 페이지 넘기기>");
				}
				else if (p == "Spacebar" || p == "Enter") break;
			}
		}
		void Option3() { // 도움말
			Console.Clear();
			Console.SetCursorPosition(2, 1);
			Console.Write("※도움말※");
			Console.SetCursorPosition(2, 3);
			Console.Write("↔: 이동");
			Console.SetCursorPosition(2, 4);
			Console.Write("↑, X: 시계방향 회전");
			Console.SetCursorPosition(2, 5);
			Console.Write("Z: 반시계방향 회전");
			Console.SetCursorPosition(2, 6);
			Console.Write("↓: 한 칸씩 내리기");
			Console.SetCursorPosition(2, 7);
			Console.Write("Spacebar: 한 번에 내리기");

			Console.SetCursorPosition(2, 8);
			Console.Write("C: 홀드");
			Console.SetCursorPosition(2, 9);
			Console.Write("P: 일시 정지/해제");
			Console.SetCursorPosition(2, 10);
			Console.Write("Q: 메뉴로 돌아가기");

			Console.SetCursorPosition(2, 12);
			Console.Write("레벨이 높아질수록 블록이 떨어지는 속도가 빨라집니다");
			Console.SetCursorPosition(2, 13);
			Console.Write("15줄 없앨 때마다 레벨이 1씩 오릅니다");
			Console.SetCursorPosition(2, 14);
			Console.Write("홀드 버튼으로 블록 하나를 킵해두고 불러올 수 있습니다 (연속 사용 불가)");

			Console.SetCursorPosition(2, 16);
			Console.Write("1줄 제거당 기본 점수는 {0}점 입니다", ONELINE);
			Console.SetCursorPosition(2, 17);
			Console.Write("한 번에 줄을 많이 없애면 1줄 제거당 점수가 {0}점씩 추가로 상승합니다", ADDLINE);
			Console.SetCursorPosition(2, 18);
			Console.Write("연속으로 줄을 없애면 점수 배율이 {0}배씩 증가합니다", COMBOBONUS);
			Console.SetCursorPosition(2, 19);
			Console.Write("레벨이 1 상승할 때마다 점수 배율이 {0}배씩 증가합니다", LEVELBONUS);
			Console.SetCursorPosition(2, 20);
			Console.Write("제어판 > 작은아이콘 보기 > 키보드 > 재입력 시간 짧음시 이동이 부드러워집니다");
			Console.SetCursorPosition(50, 23);
			Console.Write("▶ 뒤로가기(Spacebar/Enter)");
			MyFunction.WaitForExit();
		}
		void Option4() { // 테두리색 변경
			Console.ResetColor();
			Console.Clear();

			Console.SetCursorPosition(STX+2*W+16, STY+7);
			Console.Write("※테두리 색 변경※");
			Console.SetCursorPosition(STX+2*W+16, STY+8);
			Console.Write("↔: 색깔 변경");
			Console.SetCursorPosition(STX+2*W+16, STY+9);
			Console.Write("Spacebar: 결정");

			Console.ResetColor();
			Console.SetCursorPosition(STX+2*W+20, STY+11);
			Console.Write("◀   ▶");

			Console.BackgroundColor = (ConsoleColor)backColor;
			Console.SetCursorPosition(STX+2*W+23, STY+11);
			Console.Write(" ");
			GameStart.DrawOutline1(backColor);

			while (Console.KeyAvailable) Console.ReadKey(true);
			while (true) {
				ConsoleKeyInfo cki = Console.ReadKey(true);
				string s = cki.Key.ToString();

				if (s=="LeftArrow"||s=="RightArrow") {
					if (s=="LeftArrow") backColor = (backColor+9)%10;
					else backColor = (backColor+1)%10;
					Console.BackgroundColor=(ConsoleColor)backColor;
					Console.SetCursorPosition(STX+2*W+23, STY+11);
					Console.Write(" ");
					GameStart.DrawOutline1(backColor);
				}
				else if (s=="Spacebar"||s=="Enter") break;
			}
		}
	}
}
