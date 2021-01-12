using System;
using System.Threading;
using System.IO;
public class ClimbLadder {
  const int BOUNDX = 80, BOUNDY = 24;
  const int MAXCAND = 13;
  int numCand, numPrize;
  int gap; // 네모칸 사이의 거리
  char[,] ladderLine = new char[BOUNDX, BOUNDY];
  // 윗쪽, 아랫쪽 사각형 사용 여부
  bool[] upUsed = new bool[MAXCAND+1]; // 1-based
  bool[] downUsed = new bool[MAXCAND+1];
  // isWin[i] = i번째 윗 사각형이 당첨인가
  bool[] isWin = new bool[MAXCAND+1];
  public ClimbLadder() {
    Console.ResetColor();
    Console.Clear();
    StartThread();
  }
  void GetUserInput() {
    string s;
    bool goodInput;
    Console.ResetColor();

    // numCand 입력받기
    while (true) {
      Console.Clear();
      Console.SetCursorPosition(1, 1);
      Console.Write("지원자 수를 입력하세요(2 ~ {0}): ", MAXCAND);
      s = Console.ReadLine();

      goodInput=true;
      for (int i = 0; i<s.Length; ++i)
        if (s[i]<'0'||s[i]>'9') {
          goodInput=false;
          break;
        }
      if (goodInput) {
        numCand=int.Parse(s);
        if (numCand<2||numCand>13) goodInput=false;
      }
      if (goodInput) break;
    }
    // numPrize 입력받기
    while (true) {
      Console.Clear();
      Console.SetCursorPosition(1, 1);
      Console.WriteLine("지원자 수를 입력하세요(2 ~ 13): {0}", numCand);
      Console.Write(" 당첨자 수를 입력하세요(1 ~ 지원자 수): ");
      s=Console.ReadLine();

      goodInput=true;
      for (int i = 0; i<s.Length; ++i)
        if (s[i]<'0'||s[i]>'9') {
          goodInput=false;
          break;
        }
      if (goodInput) {
        numPrize=int.Parse(s);
        if (numPrize>numCand||numPrize<1) goodInput=false;
      }
      if (goodInput) break;
    }
    Console.Write("\n 사다리 생성중..");
    Thread.Sleep(1000);
    Console.Clear();
  }
  void GetRandomPrize() {
    // isWin 갱신
    int[] A = new int[numCand];
    for (int i = 0; i<numPrize; ++i) A[i]=1;
    MyFunction.Shuffle(ref A);
    for (int i = 0; i<numCand; ++i)
      if (A[i]==1)
        isWin[i+1]=true;
  }
  void Adjust() {
    // 배열 초기화 및 출력 사이즈 조정
    int[] A = { 34, 15, 9, 6, 4, 3, 2, 1, 1, 0, 0, 0 };
    gap=A[numCand-2];

    ladderLine=new char[BOUNDX, BOUNDY];
    upUsed=new bool[MAXCAND+1];
    downUsed=new bool[MAXCAND+1];
    isWin=new bool[MAXCAND+1];
    GetRandomPrize();
  }
  int GetABX(int nth) {
    return ((gap+3)*nth-(gap+2))*2;
  }
  void DrawOutline() {
    Console.ResetColor();
    for (int cur = 1; cur<=numCand; ++cur) {
      // 현재 네모의 가운데 절대좌표 x
      int abx = GetABX(cur);
      Console.SetCursorPosition(abx-2, 0);
      Console.Write("┌───┐");
      Console.SetCursorPosition(abx-2, 1);
      Console.Write("│   │");
      Console.SetCursorPosition(abx-2, 2);
      Console.Write("└─┬─┘");

      for (int j = 3; j<=18; ++j) {
        Console.SetCursorPosition(abx, j);
        Console.Write("│");
        ladderLine[abx, j]='│';
      }
      Console.SetCursorPosition(abx-2, 19);
      Console.Write("┌─┴─┐");
      Console.SetCursorPosition(abx-2, 20);
      Console.Write("│   │");
      Console.SetCursorPosition(abx-2, 21);
      Console.Write("└───┘");
      Console.SetCursorPosition(abx, 20);
      Console.Write(cur);
    }
    Console.SetCursorPosition(1, 23);
    Console.Write("방향키 이동 / Spacebar: 선놓기(지우기) / Enter: 결정 / R: 재시작 / Q: 종료");
  }
  void DrawUserLine(int abx, int aby, bool clearLine, bool remove) {
    if (remove) {
      Console.BackgroundColor=ConsoleColor.Black;
      Console.ForegroundColor=ConsoleColor.Black;
    }
    else Console.ForegroundColor=ConsoleColor.Gray;

    if (clearLine) {
      if (!remove) {
        if (ladderLine[abx-2, aby]=='┤'||
            ladderLine[abx+2*(gap+2), aby]=='├') {
          // 인접한 선과는 같은 높이에 놓을 수 없음
          return;
        }
        for (int i = 0; i<gap+2; ++i) {
          Console.SetCursorPosition(abx+2*i, aby);
          Console.Write("──");
          ladderLine[abx+2*i, aby]='─';
        }
        Console.SetCursorPosition(abx-2, aby);
        Console.Write("├─");
        ladderLine[abx-2, aby]='├';
        Console.SetCursorPosition(abx+2*(gap+2), aby);
        Console.Write("┤");
        ladderLine[abx+2*(gap+2), aby]='┤';
      }
      else { // remove
        for (int i = 0; i<gap+2; ++i) {
          Console.SetCursorPosition(abx+2*i, aby);
          Console.Write("──");
          ladderLine[abx+2*i, aby]=(char)0;
        }
        Console.ForegroundColor=ConsoleColor.Gray;
        Console.SetCursorPosition(abx-2, aby);
        Console.Write("│ ");
        ladderLine[abx-2, aby]='│';
        Console.SetCursorPosition(abx+2*(gap+2), aby);
        Console.Write("│ ");
        ladderLine[abx+2*(gap+2), aby]='│';
      }
    }
    else { // !clearLine
      for (int i = 0; i<gap+2; ++i) {
        Console.SetCursorPosition(abx+2*i, aby);
        Console.Write("--");
      }
    }
  }
  void MoveUserLine(ref int abx, ref int aby, string s) {
    // 갈 곳에 선이 없으면 점선 표시
    // 이미 선이 있으면 Magenta색 배경 표시
    int x = abx/2, y = aby; // 특수문자 길이=1로 가정했을 때 좌표
    int nx = x, ny = y;

    if (s=="UpArrow") ny=(y+9)%14+4;
    else if (s=="DownArrow") ny=(y-3)%14+4;
    else {
      int MOD = numCand-1;
      int m = gap+3;
      int tmpX = -1;

      if (s=="LeftArrow") tmpX=((x-2)/m-1+MOD)%MOD;
      else tmpX=((x-2)/m+1)%MOD;
      nx=tmpX*m+2;
    }
    int nabx = nx*2, naby = ny;
    bool curClear = ladderLine[abx, aby]!=0;
    bool nextClear = ladderLine[nabx, naby]!=0;

    Console.BackgroundColor=ConsoleColor.Black;
    DrawUserLine(abx, aby, curClear, !curClear);
    if (nextClear) Console.BackgroundColor=ConsoleColor.Magenta;
    DrawUserLine(nabx, naby, nextClear, false);

    abx=nabx; aby=naby;
  }
  int FollowLadder(int selectNum, bool win, bool remove) {
    int abx = GetABX(selectNum);
    int i, j;

    if (remove) Console.ResetColor();
    else {
      upUsed[selectNum]=true;
      // 해당 윗 사각형을 색칠
      Console.BackgroundColor=ConsoleColor.Black;
      if (win) Console.ForegroundColor=ConsoleColor.Green;
      else Console.ForegroundColor=ConsoleColor.White;

      for (i=-2; i<=2; i+=2)
        for (j=-1; j<=1; ++j) {
          if (i==0&&j==0) continue;
          Console.SetCursorPosition(abx+i, 1+j);
          Console.Write("■");
        }
    }
    // 선 자취 따라가기 (모든 변수 값은 절대좌표 기준)
    int dir = 0; // 현재 진행방향 0=아래, 1=왼, 2=오
    int x = abx, y = 3;
    while (y<19) {
      Console.SetCursorPosition(x, y);
      if (!remove) {
        Console.Write("■");
        Thread.Sleep(30); // 사다리 따라가는 속도
      }
      else Console.Write(ladderLine[x, y]);

      // 방향 수정
      if (ladderLine[x, y]=='├') {
        if (dir==0) dir=2;
        else dir=0;
      }
      else if (ladderLine[x, y]=='┤') {
        if (dir==0) dir=1;
        else dir=0;
      }
      // 방향 수정에 따른 다음 좌표 계산
      if (dir==0) ++y;
      else if (dir==1) x-=2;
      else x+=2;
    }
    int ret = selectNum-(abx-x)/(2*gap+6);
    if (!remove) {
      downUsed[ret]=true;
      // 매치된 사각형을 색칠
      for (i=-2; i<=2; i+=2)
        for (j=-1; j<=1; ++j) {
          if (i==0&&j==0) continue;
          Console.SetCursorPosition(x+i, 20+j);
          Console.Write("■");
        }
    }
    return ret;
  }
  int SelectUpRec() {
    // 선택할 곳이 반드시 있음을 가정
    int cur = 1;
    while (upUsed[cur]) ++cur;
    int abx = GetABX(cur);

    Console.BackgroundColor=ConsoleColor.Cyan;
    Console.SetCursorPosition(abx, 1);
    Console.Write("  ");

    while (Console.KeyAvailable)
      Console.ReadKey(true);
    while (true) {
      ConsoleKeyInfo cki = Console.ReadKey(true);
      string s = cki.Key.ToString();

      if (s=="LeftArrow"||s=="RightArrow") {
        Console.BackgroundColor=ConsoleColor.Black;
        Console.SetCursorPosition(abx, 1);
        Console.Write("  ");

        while (true) {
          if (s[0]=='L') cur=(cur-2+numCand)%numCand+1;
          else cur=cur%numCand+1;
          if (!upUsed[cur]) break;
        }
        abx=GetABX(cur);
        Console.BackgroundColor=ConsoleColor.Cyan;
        Console.SetCursorPosition(abx, 1);
        Console.Write("  ");
      }
      else if (s=="Q") return -1;
      else if (s=="R") return 0;
      else if (s=="Spacebar") {
        Console.BackgroundColor=ConsoleColor.Black;
        Console.SetCursorPosition(abx, 1);
        Console.Write("  ");
        break;
      }
    }
    return cur;
  }
  void StartThread() {
    while (true) {
      GetUserInput(); // 지원자 수, 당첨자 수 입력 받기
      Adjust(); // 지원자 수에 따른 전체적 크기 조정
      DrawOutline(); // 기초 사다리 그리기

      bool restart = false;
      bool quit = false;
      int curABX = 4, curABY = 4;
      DrawUserLine(curABX, curABY, false, false);

      while (true) {
        while (Console.KeyAvailable) Console.ReadKey(true);
        ConsoleKeyInfo cki = Console.ReadKey(true);
        string s = cki.Key.ToString();
        bool curClear = ladderLine[curABX, curABY]!=0;

        if (s.Contains("Arrow"))
          MoveUserLine(ref curABX, ref curABY, s);
        else if (s=="Spacebar") {
          Console.ResetColor();
          DrawUserLine(curABX, curABY, true, curClear);
        }
        else if (s=="Enter"||s=="R"||s=="Q") {
          if (s=="R") restart=true;
          else if (s=="Q") quit=true;
          else { // Enter
            if (curClear) Console.BackgroundColor=ConsoleColor.Black;
            DrawUserLine(curABX, curABY, curClear, !curClear);
          }
          break;
        }
      }
      // 사다리 미완성, 도중에 나옴
      if (quit) break;
      if (restart) continue;

      // 사다리가 완성됨, 도움말 수정
      Console.ResetColor();
      Console.SetCursorPosition(1, 22);
      Console.Write("※남은 당첨수:  / ★축★당첨자: ");

      int x1 = 16, x2 = 35; // 윗줄 빈칸의 절대x좌표
      Console.SetCursorPosition(x1, 22);
      Console.Write(numPrize);

      Console.SetCursorPosition(0, 23);
      for (int i = 0; i<BOUNDX; ++i) Console.Write(" ");
      Console.SetCursorPosition(1, 23);
      Console.Write("방향키 이동 / Spacebar: 선택 / R: 재시작 / Q: 종료");

      while (numPrize>0) {
        int selectNum = -1, match = -1;
        selectNum=SelectUpRec();
        if (selectNum==0) { restart=true; break; }
        else if (selectNum==-1) { quit=true; break; }

        Console.ResetColor();
        Console.SetCursorPosition(GetABX(selectNum), 1);

        if (isWin[selectNum]) {
          Console.Write("축");
          Console.SetCursorPosition(x1, 22);
          Console.Write(--numPrize);
        }
        else Console.Write("꽝");
        Thread.Sleep(1000);

        match=FollowLadder(selectNum, isWin[selectNum], false);
        Thread.Sleep(1000);
        match=FollowLadder(selectNum, isWin[selectNum], true);

        if (isWin[selectNum]) {
          Console.SetCursorPosition(x2, 22);
          Console.Write(match);
          if (match<10) x2+=2;
          else x2+=3;
        }
      }
      while (Console.KeyAvailable)
        Console.ReadKey(true);
      while (!quit&&!restart) {
        ConsoleKeyInfo cki = Console.ReadKey(true);
        string s = cki.Key.ToString();
        if (s=="Q") quit=true;
        else if (s=="R") restart=true;
      }
      if (quit) break;
      if (restart) continue;
    }
  }
}