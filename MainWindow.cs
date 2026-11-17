using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using NetCoreAudio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CyberpunkMac
{
	public class Enemy
	{
		public Rect Bounds;
		public int Type;
	}

	public class FlyingAV
	{
		public double X, Y, Speed;
		public Color LightColor;
	}

	public class GameCanvas : Control
	{
		Player bgmPlayer = new Player();
		bool isMusicPlaying = false, isMuted = false;
		string currentMp3Path = "";

		bool isInMenu = true, isGameOver = false, isPaused = false;
		int gameOverCause = 0;

		Rect playButtonRect = new Rect(300, 210, 200, 50);
		Rect menuButtonRect = new Rect(280, 175, 240, 38);
		Rect exitButtonRect = new Rect(325, 225, 150, 34);

		double groundY = 330, playerX = 80, playerY = 266, playerWidth = 56, playerHeight = 64;
		int animCounter = 0, runFrame = 0;
		double verticalSpeed = 0, gravity = 3, jumpStrength = -26;
		bool isJumping = false;

		List<Enemy> enemies = new List<Enemy>();
		List<FlyingAV> avTraffic = new List<FlyingAV>();
		List<Point> rainDrops = new List<Point>();
		double searchlightAngle = 0;
		double gameSpeed = 15;
		int spawnTimer = 0, avSpawnTimer = 25;
		Random rand = new Random();

		int score = 0, highScore = 0;
		string saveFilePath;
		DispatcherTimer gameTimer = new DispatcherTimer();

		bool altActive = false;
		double altX = -100, altY = 105;
		int nextAltMilestone = 250;
		int nextSmasherMilestone = 730;
		bool spawnSmasherNext = false;

		bool satSpawned = false, satActive = false;
		double satX = 0, satY = 0;
		int satFlashAlpha = 0, johnnyCommentTimer = 0;

		bool davidSpawned = false;
		int davidTimer = 0;

		bool porscheSpawned = false, spawnPorscheNext = false;
		bool vtechSpawned = false, spawnVtechNext = false;
		bool rogueSpawned = false, spawnRogueNext = false;

		Typeface monoFont = new Typeface("Menlo, Courier New, monospace", FontStyle.Normal, FontWeight.Bold);

		string[] johnnyRun1 = { "..HHHHHHH.....", ".HHHHHHHHH....", ".HHSSSSSHH....", ".HSGGSGGSH....", ".HSSSSSSSH....", "..HSSSSSH.....", ".BBYRRRYBB....", "ABBRRRRRBBMMMM", "ABBRYRYRBBS.M.", "ABBRRRRRBB....", "A.PPPPPP......", ".PP....PP.....", "PP......PP....", "PP......PP....", "KK......KK....", "KKA.....KKA..." };
		string[] johnnyRun2 = { "..HHHHHHH.....", ".HHHHHHHHH....", ".HHSSSSSHH....", ".HSGGSGGSH....", ".HSSSSSSSH....", "..HSSSSSH.....", ".BBYRRRYBB....", "ABBRRRRRBBMMMM", "ABBRYRYRBBS.M.", "ABBRRRRRBB....", "A.PPPPPP......", "...PPPP.......", "..PP..PP......", "..PP..PP......", "..KK..KK......", "..KKA.KKA....." };
		string[] johnnyJump = { "..HHHHHHH.....", ".HHHHHHHHH....", ".HHSSSSSHH....", ".HSGGSGGSH....", ".HSSSSSSSH....", "..HSSSSSH.....", ".BBYRRRYBB....", "ABBRRRRRBBMMMM", "ABBRYRYRBBS.M.", "ABBRRRRRBB....", "A.PPPPPP......", ".PP....PP.....", ".KK....KK.....", ".KKA...KKA....", "..............", ".............." };
		string[] arasakaSprite = { "...KKKK...", "..KKKKKK..", "..KRRRRK..", "..KKKKKK..", "...KKKK...", ".KKKKKKKK.", "KKKWKKKKKK", "KKKKKKKKKK", "K.KKKKKK.K", "K.KKKKKK.K", "..KK..KK..", "..KK..KK..", "..KK..KK..", ".KKK..KKK." };
		string[] ncpdSprite = { "...NNNN...", "..NNNNNN..", "..NCCCCN..", "..NNNNNN..", "...NNNN...", ".NNNNNNNN.", "NNYNNNNNNN", "NNCCCCCCNN", "N.NNNNNN.N", "N.NNNNNN.N", "..NN..NN..", "..NN..NN..", "..NN..NN..", ".NNN..NNN." };
		string[] smasherSprite = { "....DDDDDD....", "...DRRRRRRD...", "...DDDDDDDD...", ".DDDDDDDDDDDD.", "DDDDDRRRRDDDDD", "DDDDDRRRRDDDDD", "ADDDDDDDDDDDDA", "ADDD.DDDD.DDDA", "ADDD.DDDD.DDDA", "A...DDDDDD...A", "....DDDDDD....", "...DDD..DDD...", "...DDD..DDD...", "..ADDD..DDDA..", "..DDDD..DDDD..", "..DDDD..DDDD..", "..DDDD..DDDD..", ".DDDDD..DDDDD.", "DDDDDD..DDDDDD" };
		string[] rogueSprite = { "..WWWWWWW..", ".WWWWWWWWW.", ".WWSSSSSWW.", ".WSKKSKKSW.", ".WSSSSSSSW.", "..WSSSSSW..", ".BBBRRRBBB.", "SBBBRRRBBBS", "SBBBBBBBBBS", "S.PPPPPP.S.", "..PP..PP...", "..PP..PP...", "..PP..PP...", "..KK..KK...", ".KKK..KKK.." };
		string[] altCyberSprite = { "....QQQQQQ....", "...QQZZZZQQ...", "..QQZWWWWZQQ..", ".QQZZZ..ZZZQQ.", "QQ.ZZWWWWZZ.QQ", "Q..ZZZZZZZZ..Q", "Q...ZZZZZZ...Q", ".Q...ZZZZ...Q.", "..Q.ZZZZZZ.Q..", "..QZZZZZZZZQ..", ".QZZ.ZZZZ.ZZQ.", ".QZ..ZZZZ..ZQ.", "Q....ZZZZ....Q", "....ZZ..ZZ....", "...ZZ....ZZ...", "...Z......Z...", "..Z........Z..", ".............." };
		string[] porscheSprite = { "......AAAAAAAA..........", ".....ACCCCCCCCAA........", "....ACCCCCCCCCCCAA......", ".AAAAAAAAAAAAAAAAAAAA...", "RRRRRRRRRRRRRRRRRRRRRR..", "EEEEEEEEEEEEEEEEEEEEEE..", "EEEEKKKKKEEEEEEEKKKKKE..", "....KAAAK.......KAAAK...", "....KKKKK.......KKKKK..." };
		string[] vtechSprite = { ".......KKKKKKKKKK.........", ".....KKCCCCCCCCCCKK.......", "...KKCCCCCCCCCCCCCCKK.....", ".KKKKKKKKKKKKKKKKKKKKKKR..", "KKKKKKKKKKKKKKKKKKKKKKKR..", "KKKKKKKKKKKKKKKKKKKKKKKR..", "KKKKKKKKK.....KKKKKKKKK...", "....KYYYK.........KYYYK...", "....KKKKK.........KKKKK..." };
		string[] samuraiLogoSprite = { "YY............YY", "YYY..........YYY", ".YYY..RRRR..YYY.", "..YYRRRRRRRRYY..", "...RRRRRRRRRR...", "..RRKRRRRRRKRR..", ".RRRWWKRRKWWMRR.", ".RRRRRRRRRRRRRR.", ".RRRRRRKKRRRRRR.", "..RRRRRRRRRRRR..", "..RRYYRRRRYYRR..", "...RYYKKKKYYR...", "...RRRRRRRRRR...", "....RRRRRRRR....", ".....RRRRRR....." };
		string[] satSprite = { "CC....AAAA....CC", "CC..AAAAAAAA..CC", "CC.AAARRRRAAA.CC", "CC..AAAAAAAA..CC", "CC....AAAA....CC" };
		string[] davidSprite = { "..HHH..", ".HSSSH.", "..SSS..", "YYYYYYY", "YKYCYKY", "YKYKYKY", ".KKKKK.", ".KK.KK.", ".KK.KK.", "KK...KK" };

		public GameCanvas()
		{
			Focusable = true;
			Width = 800;
			Height = 400;

			for (int i = 0; i < 65; i++)
				rainDrops.Add(new Point(rand.Next(0, 800), rand.Next(0, (int)groundY)));

			saveFilePath = Path.Combine(AppContext.BaseDirectory, "highscore.txt");
			LoadHighScore();
			StartBackgroundMusic();

			gameTimer.Interval = TimeSpan.FromMilliseconds(16);
			gameTimer.Tick += GameLoop;
			gameTimer.Start();
		}

		private async void StartBackgroundMusic()
		{
			try
			{
				string[] mp3Files = Directory.GetFiles(AppContext.BaseDirectory, "*.mp3");
				if (mp3Files.Length == 0) return;

				currentMp3Path = mp3Files[0];
				bgmPlayer.PlaybackFinished += async (s, e) =>
				{
					if (!isMuted && !isPaused) await bgmPlayer.Play(currentMp3Path);
				};
				await bgmPlayer.Play(currentMp3Path);
				isMusicPlaying = true;
			}
			catch { }
		}

		private async void ToggleMusicPause(bool pause)
		{
			if (!isMusicPlaying || isMuted) return;
			try
			{
				if (pause) await bgmPlayer.Pause();
				else await bgmPlayer.Resume();
			}
			catch { }
		}

		public async void StopMusicOnExit()
		{
			SaveHighScore();
			try { await bgmPlayer.Stop(); } catch { }
		}

		private void LoadHighScore()
		{
			try
			{
				if (File.Exists(saveFilePath))
					int.TryParse(File.ReadAllText(saveFilePath).Trim(), out highScore);
			}
			catch { }
		}

		private void SaveHighScore()
		{
			try
			{
				if (score > highScore) highScore = score;
				File.WriteAllText(saveFilePath, highScore.ToString());
			}
			catch { }
		}

		private void GameLoop(object sender, EventArgs e)
		{
			if (!isPaused && !isGameOver)
			{
				searchlightAngle += 0.03;

				for (int i = 0; i < rainDrops.Count; i++)
				{
					double rx = rainDrops[i].X - 2;
					double ry = rainDrops[i].Y + 9;
					if (ry > groundY) { ry = 0; rx = rand.Next(0, 820); }
					if (rx < 0) rx += 800;
					rainDrops[i] = new Point(rx, ry);
				}

				avSpawnTimer--;
				if (avSpawnTimer <= 0)
				{
					avTraffic.Add(new FlyingAV { X = 850, Y = rand.Next(35, 130), Speed = rand.Next(5, 10), LightColor = (rand.Next(0, 2) == 0) ? Colors.Cyan : Colors.Crimson });
					avSpawnTimer = rand.Next(60, 140);
				}
				for (int i = avTraffic.Count - 1; i >= 0; i--)
				{
					avTraffic[i].X -= avTraffic[i].Speed;
					if (avTraffic[i].X < -80) avTraffic.RemoveAt(i);
				}
			}

			if (isInMenu || isGameOver || isPaused)
			{
				InvalidateVisual();
				return;
			}

			animCounter++;
			if (animCounter >= 3) { runFrame = (runFrame == 0) ? 1 : 0; animCounter = 0; }

			playerY += verticalSpeed;
			if (playerY < groundY - playerHeight) verticalSpeed += gravity;
			else { playerY = groundY - playerHeight; verticalSpeed = 0; isJumping = false; }

			spawnTimer--;
			if (spawnTimer <= 0)
			{
				if (score >= 3200)
				{
					int roll = rand.Next(0, 100);
					if (roll < 12) { enemies.Add(new Enemy { Bounds = new Rect(820, groundY - 60, 44, 60), Type = 5 }); spawnTimer = rand.Next(50, 72); }
					else if (roll < 24) { enemies.Add(new Enemy { Bounds = new Rect(820, groundY - 36, 104, 36), Type = 4 }); spawnTimer = rand.Next(48, 70); }
					else if (roll < 36) { enemies.Add(new Enemy { Bounds = new Rect(820, groundY - 36, 96, 36), Type = 3 }); spawnTimer = rand.Next(48, 70); }
					else if (roll < 52) { enemies.Add(new Enemy { Bounds = new Rect(820, groundY - 76, 56, 76), Type = 2 }); spawnTimer = rand.Next(45, 68); }
					else
					{
						int count = rand.Next(2, 5);
						int enemyType = rand.Next(0, 2);
						for (int c = 0; c < count; c++)
							enemies.Add(new Enemy { Bounds = new Rect(800 + (c * 34), groundY - 56, 40, 56), Type = enemyType });
						spawnTimer = rand.Next(30, 55);
					}

					if (!altActive && rand.Next(0, 100) < 12) { altActive = true; altX = 820; }
				}
				else
				{
					if (spawnRogueNext) { enemies.Add(new Enemy { Bounds = new Rect(820, groundY - 60, 44, 60), Type = 5 }); spawnRogueNext = false; spawnTimer = rand.Next(50, 75); }
					else if (spawnVtechNext) { enemies.Add(new Enemy { Bounds = new Rect(820, groundY - 36, 104, 36), Type = 4 }); spawnVtechNext = false; spawnTimer = rand.Next(50, 75); }
					else if (spawnPorscheNext) { enemies.Add(new Enemy { Bounds = new Rect(820, groundY - 36, 96, 36), Type = 3 }); spawnPorscheNext = false; spawnTimer = rand.Next(50, 75); }
else if (spawnSmasherNext) { enemies.Add(new Enemy { Bounds = new Rect(820, groundY - 76, 56, 76), Type = 2 }); spawnSmasherNext = false; spawnTimer = rand.Next(48, 72); }
else
{
int count = rand.Next(1, (score >= 150) ? 5 : 4);
int enemyType = rand.Next(0, 2);
for (int c = 0; c < count; c++)
enemies.Add(new Enemy { Bounds = new Rect(800 + (c * 34), groundY - 56, 40, 56), Type = enemyType });
spawnTimer = rand.Next(32, 60);
}
}
}
Rect playerHitbox = new Rect(playerX + 8, playerY + 4, 28, playerHeight - 6);
for (int i = enemies.Count - 1; i >= 0; i--)
{
Enemy enemy = enemies[i];
Rect b = enemy.Bounds;
double moveX = (enemy.Type == 4) ? (gameSpeed + 7) : ((enemy.Type == 3) ? (gameSpeed + 5) : gameSpeed);
enemy.Bounds = new Rect(b.X - moveX, b.Y, b.Width, b.Height);
Rect enemyHitbox = enemy.Bounds;
if (enemy.Type == 2) enemyHitbox = new Rect(enemy.Bounds.X + 8, enemy.Bounds.Y + 4, enemy.Bounds.Width - 16, enemy.Bounds.Height - 4);
else if (enemy.Type == 3 || enemy.Type == 4) enemyHitbox = new Rect(enemy.Bounds.X + 6, enemy.Bounds.Y + 6, enemy.Bounds.Width - 12, enemy.Bounds.Height - 6);
if (playerHitbox.Intersects(enemyHitbox))
{
isGameOver = true;
gameOverCause = enemy.Type;
SaveHighScore();
}
if (enemy.Bounds.X + enemy.Bounds.Width < -160)
{
score += (enemy.Type == 5) ? 100 : (enemy.Type == 4) ? 70 : (enemy.Type == 3 || enemy.Type == 2) ? 50 : 10;
enemies.RemoveAt(i);
if (score > highScore)
{
highScore = score;
SaveHighScore();
}
if (score % 40 == 0 && gameSpeed < 34) gameSpeed++;
if (score >= 3000 && !davidSpawned)
{
davidSpawned = true;
davidTimer = 260;
}
if (score < 3200)
{
if (score >= nextAltMilestone) { nextAltMilestone += 1000; altActive = true; altX = 820; }
if (score >= 520 && !satSpawned) { satSpawned = true; satActive = true; satX = 760; satY = -20; satFlashAlpha = 95; johnnyCommentTimer = 140; }
if (score >= nextSmasherMilestone) { nextSmasherMilestone += 730; spawnSmasherNext = true; }
if (score >= 1000 && !porscheSpawned) { porscheSpawned = true; spawnPorscheNext = true; }
if (score >= 1800 && !vtechSpawned) { vtechSpawned = true; spawnVtechNext = true; }
if (score >= 2500 && !rogueSpawned) { rogueSpawned = true; spawnRogueNext = true; }
}
}
}
if (davidTimer > 0) davidTimer--;
if (satActive)
{
satX -= 3.5;
satY += 2.2;
if (satY > groundY + 20) satActive = false;
}
if (satFlashAlpha > 0) satFlashAlpha -= 5;
if (johnnyCommentTimer > 0) johnnyCommentTimer--;
if (altActive)
{
altX -= 6;
altY = 95 + Math.Sin(altX * 0.04) * 12;
if (altX < -320) altActive = false;
}
InvalidateVisual();
}
private void DrawText(DrawingContext ctx, string text, double size, IBrush brush, double x, double y)
{
var ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, monoFont, size, brush);
ctx.DrawText(ft, new Point(x, y));
}
public override void Render(DrawingContext ctx)
{
ctx.FillRectangle(Brushes.Black, new Rect(0, 0, 800, 400));
double beamTargetX = 220 + Math.Sin(searchlightAngle) * 180;
ctx.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(28, 0, 255, 255)), 16), new Point(87, 52), new Point(beamTargetX, 0));
var rainPen = new Pen(new SolidColorBrush(Color.FromArgb(45, 100, 180, 220)), 1);
foreach (Point p in rainDrops)
ctx.DrawLine(rainPen, p, new Point(p.X - 2, p.Y + 7));
if (satFlashAlpha > 0)
ctx.FillRectangle(new SolidColorBrush(Color.FromArgb((byte)satFlashAlpha, 255, 90, 20)), new Rect(0, 0, 800, groundY));
if (satActive)
{
ctx.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(180, 255, 69, 0)), 6), new Point(satX + 24, satY + 8), new Point(satX + 110, satY - 48));
ctx.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(220, 255, 215, 0)), 3), new Point(satX + 24, satY + 8), new Point(satX + 85, satY - 33));
DrawPixelArt(ctx, satSprite, satX, satY, 3);
DrawText(ctx, "СПУТНИК «ДЖОРДЖИАН»", 10, Brushes.OrangeRed, satX - 25, satY - 16);
}
DrawCorpTowers(ctx);
if (davidTimer > 0)
{
DrawPixelArt(ctx, davidSprite, 62, 40, 3);
DrawText(ctx, "ДЭВИД: Я НА ВЕРШИНЕ...", 10, Brushes.Yellow, 15, 24);
}
foreach (FlyingAV av in avTraffic)
{
ctx.FillRectangle(new SolidColorBrush(Color.FromRgb(45, 50, 65)), new Rect(av.X, av.Y, 46, 12));
ctx.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 35, 45)), new Rect(av.X + 8, av.Y - 5, 26, 5));
ctx.FillRectangle(new SolidColorBrush(av.LightColor), new Rect(av.X + 2, av.Y + 4, 6, 3));
ctx.FillRectangle(Brushes.OrangeRed, new Rect(av.X + 44, av.Y + 4, 4, 4));
}
var cyberYellow = new SolidColorBrush(Color.FromRgb(252, 238, 10));
ctx.DrawLine(new Pen(cyberYellow, 3), new Point(0, groundY), new Point(800, groundY));
if (isInMenu)
{
DrawPixelArt(ctx, johnnyRun1, playerX, playerY, 4);
ctx.DrawRectangle(new SolidColorBrush(Color.FromArgb(215, 8, 8, 14)), new Pen(cyberYellow, 2), new Rect(140, 45, 520, 255));
DrawText(ctx, "CYBERPUNK 2077", 14, Brushes.Crimson, 335, 60);
DrawText(ctx, "RUN JONNY SILVERHAND", 28, Brushes.Cyan, 215, 82);
DrawText(ctx, "ЛУЧШИЙ РЕКОРД: " + highScore, 16, cyberYellow, 305, 130);
string soundStatusMenu = isMusicPlaying ? (isMuted ? "[M] ЗВУК: ВЫКЛ" : "[M] OST: THE REBEL PATH") : "[M] НЕТ MP3";
DrawText(ctx, "Прыжок: ПРОБЕЛ   |   Пауза: ESC   |   " + soundStatusMenu, 11, Brushes.LightGray, 175, 168);
ctx.DrawRectangle(cyberYellow, new Pen(Brushes.Cyan, 2), playButtonRect);
DrawText(ctx, "PLAY", 24, Brushes.Black, 368, 220);
DrawText(ctx, "Нажмите PLAY или клавишу ENTER", 11, Brushes.Gray, 290, 272);
return;
}
if (altActive)
{
DrawPixelArt(ctx, altCyberSprite, altX, altY, 4);
DrawText(ctx, "АЛЬТ: Я БОЛЬШЕ НЕ АЛЬТ КАННИНГЕМ. ТЫ ЗНАЕШЬ ЭТО, ДЖОННИ.", 10, Brushes.DeepPink, altX - 110, altY - 18);
}
DrawPixelArt(ctx, isJumping ? johnnyJump : ((runFrame == 0) ? johnnyRun1 : johnnyRun2), playerX, playerY, 4);
if (johnnyCommentTimer > 0)
DrawText(ctx, "ДЖОННИ: НЕПЛОХОЙ ФЕЙЕРВЕРК, АЛЬТ.", 11, cyberYellow, playerX - 20, playerY - 22);
foreach (Enemy enemy in enemies)
{
if (enemy.Type == 0) DrawPixelArt(ctx, arasakaSprite, enemy.Bounds.X, enemy.Bounds.Y, 4);
else if (enemy.Type == 1) DrawPixelArt(ctx, ncpdSprite, enemy.Bounds.X, enemy.Bounds.Y, 4);
else if (enemy.Type == 2)
{
DrawPixelArt(ctx, smasherSprite, enemy.Bounds.X, enemy.Bounds.Y, 4);
DrawText(ctx, "АДАМ СМЭШЕР", 10, Brushes.OrangeRed, enemy.Bounds.X - 10, enemy.Bounds.Y - 15);
}
else if (enemy.Type == 3)
{
DrawPixelArt(ctx, porscheSprite, enemy.Bounds.X, enemy.Bounds.Y, 4);
DrawText(ctx, "PORSCHE TURBO", 10, Brushes.Silver, enemy.Bounds.X - 5, enemy.Bounds.Y - 15);
}
else if (enemy.Type == 4)
{
DrawPixelArt(ctx, vtechSprite, enemy.Bounds.X, enemy.Bounds.Y, 4);
DrawText(ctx, "QUADRA TURBO-R V-TECH", 10, Brushes.Gold, enemy.Bounds.X - 15, enemy.Bounds.Y - 15);
}
else if (enemy.Type == 5)
{
double barX = enemy.Bounds.X + 45, barY = groundY - 115;
ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(20, 22, 32)), new Pen(Brushes.Cyan, 2), new Rect(barX, barY, 135, 115));
ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(10, 15, 25)), new Pen(Brushes.DeepSkyBlue, 2), new Rect(barX + 10, barY + 10, 115, 28));
DrawText(ctx, "AFTERLIFE", 15, Brushes.Cyan, barX + 18, barY + 14);
ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(35, 15, 25)), new Pen(Brushes.Crimson, 1), new Rect(barX + 45, barY + 55, 45, 60));
DrawPixelArt(ctx, rogueSprite, enemy.Bounds.X, enemy.Bounds.Y, 4);
DrawText(ctx, "БЕСТИЯ", 10, Brushes.Cyan, enemy.Bounds.X + 2, enemy.Bounds.Y - 15);
}
}
DrawText(ctx, "RUN JONNY SILVERHAND", 13, Brushes.Cyan, 15, 15);
string soundStatus = isMusicPlaying ? (isMuted ? "[M] ЗВУК: ВЫКЛ" : "[M] OST: THE REBEL PATH") : "[M] НЕТ MP3";
DrawText(ctx, "[ESC] ПАУЗА  |  " + soundStatus, 11, Brushes.Gray, 15, 36);
DrawText(ctx, "РЕПУТАЦИЯ: " + score, 16, cyberYellow, 570, 15);
DrawText(ctx, "РЕКОРД:    " + highScore, 14, Brushes.White, 570, 38);
if (isPaused && !isGameOver)
{
ctx.DrawRectangle(new SolidColorBrush(Color.FromArgb(220, 8, 10, 16)), new Pen(Brushes.Cyan, 2), new Rect(210, 85, 380, 155));
DrawText(ctx, "ПАУЗА", 28, Brushes.Cyan, 355, 100);
DrawText(ctx, "Нажмите ESC, чтобы продолжить", 12, Brushes.White, 285, 142);
ctx.DrawRectangle(Brushes.Crimson, new Pen(Brushes.White, 1), menuButtonRect);
DrawText(ctx, "ВЫЙТИ В МЕНЮ [Q]", 12, Brushes.White, menuButtonRect.X + 42, menuButtonRect.Y + 11);
}
if (isGameOver)
{
if (gameOverCause == 3)
{
ctx.DrawRectangle(new SolidColorBrush(Color.FromArgb(230, 10, 10, 15)), new Pen(Brushes.Crimson, 3), new Rect(150, 35, 500, 265));
DrawPixelArt(ctx, samuraiLogoSprite, 360, 48, 5);
DrawText(ctx, "SAMURAI", 30, Brushes.Crimson, 330, 130);
DrawText(ctx, "Счет: " + score + "  |  Лучший рекорд: " + highScore, 14, cyberYellow, 245, 180);
DrawText(ctx, "Нажмите R, чтобы сжечь город заново", 13, Brushes.White, 245, 212);
exitButtonRect = new Rect(325, 248, 150, 34);
}
else if (gameOverCause == 5)
{
ctx.DrawRectangle(new SolidColorBrush(Color.FromArgb(235, 8, 14, 24)), new Pen(Brushes.Cyan, 3), new Rect(140, 50, 520, 230));
ctx.DrawRectangle(null, new Pen(Brushes.DeepSkyBlue, 1), new Rect(146, 56, 508, 218));
DrawText(ctx, "AFTERLIFE", 38, Brushes.Cyan, 285, 68);
DrawText(ctx, "ПОСМЕРТИЕ • НАЙТ-СИТИ", 12, Brushes.Crimson, 315, 122);
DrawText(ctx, "Счет: " + score + "  |  Лучший рекорд: " + highScore, 14, cyberYellow, 245, 155);
DrawText(ctx, "Нажмите R, чтобы выпить за легенду", 13, Brushes.White, 250, 188);
exitButtonRect = new Rect(325, 226, 150, 34);
}
else
{
string titleText = "ПОЙМАН КОРПОРАТАМИ";
IBrush titleBrush = Brushes.Crimson;
IBrush borderBrush = Brushes.Crimson;
double titleX = 225;
if (gameOverCause == 1) { titleText = "ПОЙМАН ПОЛИЦИЕЙ"; titleBrush = Brushes.DeepSkyBlue; borderBrush = Brushes.DeepSkyBlue; titleX = 245; }
else if (gameOverCause == 2) { titleText = "УБИТ СМЭШЕРОМ"; titleBrush = Brushes.OrangeRed; borderBrush = Brushes.OrangeRed; titleX = 275; }
else if (gameOverCause == 4) { titleText = "ЛЕГЕНДА НАЙТ-СИТИ"; titleBrush = Brushes.Gold; borderBrush = Brushes.Gold; titleX = 240; }
ctx.DrawRectangle(new SolidColorBrush(Color.FromArgb(225, 8, 8, 12)), new Pen(borderBrush, 2), new Rect(140, 75, 520, 190));
DrawText(ctx, titleText, 28, titleBrush, titleX, 95);
DrawText(ctx, "Счет: " + score + "  |  Лучший рекорд: " + highScore, 14, cyberYellow, 245, 142);
DrawText(ctx, "Нажмите R, чтобы сжечь город заново", 13, Brushes.White, 245, 174);
exitButtonRect = new Rect(325, 212, 150, 34);
}
ctx.DrawRectangle(Brushes.Crimson, new Pen(Brushes.White, 1), exitButtonRect);
DrawText(ctx, "ВЫХОД", 13, Brushes.White, exitButtonRect.X + 52, exitButtonRect.Y + 8);
}
}
private void DrawCorpTowers(DrawingContext ctx)
{
var winDim = new SolidColorBrush(Color.FromRgb(28, 34, 48));
var winLit = new SolidColorBrush(Color.FromRgb(65, 140, 170));
ctx.DrawLine(new Pen(Brushes.Crimson, 2), new Point(95, 25), new Point(95, 52));
ctx.FillRectangle(Brushes.Red, new Rect(93, 22, 5, 5));
ctx.FillRectangle(new SolidColorBrush(Color.FromRgb(24, 22, 34)), new Rect(78, 52, 36, 18));
ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(18, 18, 28)), new Pen(new SolidColorBrush(Color.FromRgb(65, 25, 35)), 1), new Rect(40, 70, 95, 260));
DrawText(ctx, "ARASAKA", 11, Brushes.Crimson, 58, 82);
ctx.FillRectangle(Brushes.Crimson, new Rect(83, 102, 8, 45));
for (int wy = 160; wy < 310; wy += 22)
{
ctx.FillRectangle((wy == 204) ? winLit : winDim, new Rect(50, wy, 14, 10));
ctx.FillRectangle(winDim, new Rect(110, wy, 14, 10));
}
ctx.DrawLine(new Pen(Brushes.MediumTurquoise, 2), new Point(272, 18), new Point(272, 42));
ctx.FillRectangle(new SolidColorBrush(Color.FromRgb(20, 26, 38)), new Rect(238, 42, 68, 13));
ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(15, 20, 30)), new Pen(new SolidColorBrush(Color.FromRgb(35, 75, 95)), 1), new Rect(220, 55, 105, 275));
ctx.DrawLine(new Pen(Brushes.MediumTurquoise, 2), new Point(220, 55), new Point(325, 55));
DrawText(ctx, "NIGHT CORP", 11, Brushes.MediumTurquoise, 234, 68);
for (int wy = 95; wy < 310; wy += 24)
{
for (int wx = 232; wx <= 300; wx += 22)
ctx.FillRectangle((wy == 143 && wx == 254) ? winLit : winDim, new Rect(wx, wy, 12, 12));
}
ctx.FillRectangle(new SolidColorBrush(Color.FromRgb(22, 28, 42)), new Rect(442, 78, 60, 17));
ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(16, 22, 34)), new Pen(new SolidColorBrush(Color.FromRgb(30, 55, 85)), 1), new Rect(420, 95, 105, 235));
ctx.DrawLine(new Pen(Brushes.DeepSkyBlue, 2), new Point(420, 95), new Point(525, 95));
DrawText(ctx, "MILITECH", 11, Brushes.DeepSkyBlue, 440, 108);
for (int wx = 435; wx <= 505; wx += 23)
ctx.DrawLine(new Pen(new SolidColorBrush(Color.FromRgb(28, 40, 60)), 2), new Point(wx, 132), new Point(wx, 325));
ctx.DrawLine(new Pen(Brushes.Gold, 1), new Point(665, 28), new Point(665, 50));
ctx.DrawLine(new Pen(Brushes.Gold, 1), new Point(705, 28), new Point(705, 50));
ctx.FillRectangle(new SolidColorBrush(Color.FromRgb(28, 26, 36)), new Rect(650, 50, 70, 15));
ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(20, 18, 26)), new Pen(new SolidColorBrush(Color.FromRgb(75, 65, 30)), 1), new Rect(630, 65, 110, 265));
DrawText(ctx, "KANG TAO", 11, Brushes.Gold, 652, 78);
ctx.FillRectangle(new SolidColorBrush(Color.FromArgb(180, 255, 215, 0)), new Rect(645, 98, 80, 3));
for (int wy = 115; wy < 310; wy += 24)
{
ctx.FillRectangle(winDim, new Rect(645, wy, 32, 10));
ctx.FillRectangle((wy == 187) ? winLit : winDim, new Rect(692, wy, 32, 10));
}
}
private void DrawPixelArt(DrawingContext ctx, string[] sprite, double startX, double startY, double pixelSize)
{
for (int row = 0; row < sprite.Length; row++)
{
for (int col = 0; col < sprite[row].Length; col++)
{
char pixel = sprite[row][col];
if (pixel == '.') continue;
IBrush brush = Brushes.Transparent;
switch (pixel)
{
case 'H': brush = new SolidColorBrush(Color.FromRgb(40, 35, 35)); break;
case 'S': brush = Brushes.PeachPuff; break;
case 'G': brush = Brushes.DarkRed; break;
case 'B': brush = new SolidColorBrush(Color.FromRgb(45, 50, 60)); break;
case 'A': brush = Brushes.Silver; break;
case 'P': brush = Brushes.DimGray; break;
case 'Y': brush = Brushes.Yellow; break;
case 'M': brush = Brushes.LightSteelBlue; break;
case 'K': brush = new SolidColorBrush(Color.FromRgb(20, 20, 25)); break;
case 'D': brush = new SolidColorBrush(Color.FromRgb(32, 34, 40)); break;
case 'R': brush = Brushes.Crimson; break;
case 'W': brush = Brushes.White; break;
case 'N': brush = new SolidColorBrush(Color.FromRgb(20, 55, 130)); break;
case 'C': brush = Brushes.Cyan; break;
case 'E': brush = new SolidColorBrush(Color.FromRgb(25, 65, 45)); break;
case 'Q': brush = new SolidColorBrush(Color.FromArgb(210, 255, 40, 120)); break;
case 'Z': brush = new SolidColorBrush(Color.FromArgb(160, 220, 20, 80)); break;
}
ctx.FillRectangle(brush, new Rect(startX + col * pixelSize, startY + row * pixelSize, pixelSize, pixelSize));
}
}
}
protected override void OnPointerPressed(PointerPressedEventArgs e)
{
base.OnPointerPressed(e);
var pos = e.GetPosition(this);
if (isInMenu && playButtonRect.Contains(pos))
{
isInMenu = false;
ResetGame();
}
else if (isPaused && !isGameOver && menuButtonRect.Contains(pos))
{
isPaused = false;
isInMenu = true;
ToggleMusicPause(false);
InvalidateVisual();
}
else if (isGameOver && exitButtonRect.Contains(pos))
{
StopMusicOnExit();
Environment.Exit(0);
}
}
protected override void OnKeyDown(KeyEventArgs e)
{
base.OnKeyDown(e);
if (isInMenu)
{
if (e.Key == Key.Enter || e.Key == Key.Space)
{
isInMenu = false;
ResetGame();
}
return;
}
if ((e.Key == Key.Escape || e.Key == Key.P) && !isGameOver)
{
isPaused = !isPaused;
ToggleMusicPause(isPaused);
InvalidateVisual();
}
if (isPaused && !isGameOver && e.Key == Key.Q)
{
isPaused = false;
isInMenu = true;
ToggleMusicPause(false);
InvalidateVisual();
}
if (e.Key == Key.M && isMusicPlaying)
{
isMuted = !isMuted;
ToggleMusicPause(isMuted);
InvalidateVisual();
}
if ((e.Key == Key.Space || e.Key == Key.Up || e.Key == Key.W) && !isJumping && !isGameOver && !isPaused)
{
verticalSpeed = jumpStrength;
isJumping = true;
}
if (e.Key == Key.R && isGameOver) ResetGame();
}
private void ResetGame()
{
playerY = groundY - playerHeight;
verticalSpeed = 0; isJumping = false;
enemies.Clear();
gameSpeed = 15; score = 0;
nextAltMilestone = 250; nextSmasherMilestone = 730;
spawnSmasherNext = false;
satSpawned = false; satActive = false;
satFlashAlpha = 0; johnnyCommentTimer = 0;
davidSpawned = false; davidTimer = 0;
porscheSpawned = false; spawnPorscheNext = false;
vtechSpawned = false; spawnVtechNext = false;
rogueSpawned = false; spawnRogueNext = false;
altActive = false; isGameOver = false; isPaused = false;
InvalidateVisual();
}
}
public class MainWindow : Window
{
private GameCanvas canvas;
public MainWindow()
{
Title = "Cyberpunk: Run Jonny Silverhand";
Width = 800;
Height = 400;
CanResize = false;
canvas = new GameCanvas();
Content = canvas;
Opened += (s, e) => canvas.Focus();
Closing += (s, e) => canvas.StopMusicOnExit();
}
}
}
