/* PlayStation(R)Mobile SDK 1.11.01
 * Copyright (C) 2013 Sony Computer Entertainment Inc.
 * All Rights Reserved.
 */

using System;
using System.Threading;
using System.Diagnostics;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.HighLevel.Model;

using System.Collections.Generic;

using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Input;
using Sample;
using Sce.PlayStation.Core.Audio;
using EscapePenguins;


namespace Sample
{
	public class EscapePenguins
	{
		protected static GraphicsContext graphics;
		
	    public  enum StateId{
	        MovingUp = 0,
	        MovingLeft,
	        MovingDown,
			MovingRight,
	        lookingUp,
	        lookingLeft,
	        lookingDown,
			lookingRight
	    }
		
		struct CharData{
			//モデルデータ
			public BasicModel idle;	
			public BasicModel move;		
			public BasicModel clear;
			//モデルの位置。角度
			// ３～４人協力プレイになることを見越して、プレイヤーデータは配列で持つようにしたいね yoshi
			public Vector2 position;
			public Vector2 nextPosition;
			public bool warped;
			//動作に関しての情報
			public StateId mode; 
			
		};
		
		// Models
		static CharData[] player = new CharData[2];
		
		static BasicModel mdl_wall;
		static BasicModel mdl_floor;
		static BasicModel mdl_switch;

		// 環境情報
		static BasicProgram program;

		// 時間
		static Stopwatch stopwatch;
		static int frameCount;
		static int prevTicks;
		static Vector3 cameraPostion;
		//static float turn;
		static bool bLoop = true;

		static GamePadData gamePadData;
		
		static Vector2[,] warpPos = new Vector2[2,4];	//ワープ　最大４つまで設置可能
		static int warpNum;							//ステージ上にあるワープの数
		
		static rotateBlock rBlock = new rotateBlock();

		static Vector2[] goalPos = new Vector2[2];	// ゴールの位置
		
		static ObjPlaceData Place = new ObjPlaceData();	 // ステージの情報

		public static int nGameStatID = 1;	// ゲームの状態ID
		
		public static int nStageNum = 0;	// 現在のステージ番号
		
		static int Countdown = 60;			// ステージ開始ズーム用変数

		static bool bCleared = false;		// 「ステージをクリアした」フラグ

		static short[,] stage;				// ステージ情報のテンポラリ
		
		private static MenuCtrl menuCtrl = null;
		

		/*
		static Sound soundClear;
		static public SoundPlayer soundPlayerClear;

		static Sound soundhitWall;
		static public SoundPlayer soundPlayerhitWall;
		
		static Bgm bgm;
		static public BgmPlayer bgmPlayer;
		*/

		//===========================================================================
		// メイン
		//===========================================================================
		public static void Main( string[] args )
		{
			// 初期化
			AppInit();
			
			// メインループ
			while( bLoop )
			{
				SystemEvents.CheckEvents();
				
				switch( nGameStatID )
				{
					// Puzzle
					case 0:
					{
						Update();
						Draw();
						Render();
						break;
					}
					// Menu
					case 1:
					{
						menuCtrl.Update();
						menuCtrl.Render();
						break;
					}
				}
			}

			// 終了処理
			AppTerm();
		}
		
		//===========================================================================
		// アプリ初期化
		//===========================================================================
		static void AppInit()
		{
			graphics = new GraphicsContext();

			stopwatch = new Stopwatch();
			stopwatch.Start();

			stage = (short[,])Place.PlaceData0.Clone ();

			//  load models&初期化
			player[0].idle  = new BasicModel( "/Application/mdx/player_idle.mdx", 0 );
			player[0].move  = new BasicModel( "/Application/mdx/player_move.mdx", 0 );
			player[0].clear = new BasicModel( "/Application/mdx/player_happy.mdx", 0 );
			player[0].mode = StateId.lookingDown;

			player[1].idle  = new BasicModel( "/Application/mdx/player2.mdx", 0 );
			player[1].move  = new BasicModel( "/Application/mdx/player2_move.mdx", 0 );
			player[1].clear = new BasicModel( "/Application/mdx/player2_happy.mdx", 0 );			
			player[1].mode = StateId.lookingDown;
			
			mdl_wall		= new BasicModel( "/Application/mdx/box.mdx", 0 );
			mdl_floor		= new BasicModel( "/Application/mdx/floor.mdx", 0 );
			mdl_switch		= new BasicModel( "/Application/mdx/switch.mdx", 0 );

			SampleDraw.Init(graphics);
			/*
			soundClear = new Sound( "/Application/sound/clear.wav" );
			soundPlayerClear = soundClear.CreatePlayer();

			soundhitWall = new Sound( "/Application/sound/hitwall.wav" );
			soundPlayerhitWall =  soundhitWall.CreatePlayer();
			
			//@e Initialization of BGM
			//@j BGMの初期化
			bgm = new Bgm( "/Application/sound/GameBgm.wav" );
			bgmPlayer = bgm.CreatePlayer();
			bgmPlayer.Loop = true;
			bgmPlayer.Play();
			*/
			program = new BasicProgram();
			
			menuCtrl = new MenuCtrl( graphics );
			
			rBlock.init();
		}

		//===========================================================================
		// Update
		//===========================================================================
		static void Update()
		{
			//
			gamePadData = GamePad.GetData( 0 );
			if( ( ( goalPos[0] == player[0].position && goalPos[1] == player[1].position ) ||	//goal: to next stage
			   ( goalPos[1] == player[0].position && goalPos[0] == player[1].position ) ) &&
			   player[0].mode >= StateId.lookingUp && player[1].mode >= StateId.lookingUp )
			{
				bCleared = true;
//				soundPlayerClear.Play();
			}
			
			if( bCleared )
			{
		        foreach( var touchData in Touch.GetData( 0 ) )
				{
		            if( touchData.Status == TouchStatus.Down )
					{
//						soundPlayerClear.Stop();
						
						// RESET TEST
						NewStage();
						player[0].mode = StateId.lookingDown;
						player[1].mode = StateId.lookingDown;

						if( nStageNum < 22 ) // 数値固定よくないね yoshi
						{
							// NEXT STAGE
							nStageNum++;					
							NewStage();
						}
						else
						{
							// ALL STAGE CLEARED
							menuCtrl.MoveSq( MenuCtrl.SqGameClear );
							nGameStatID = 1;
							nStageNum = 0;
						}
				   }
				}
			}

			// スタートボタン処理
			if( ( gamePadData.Buttons & GamePadButtons.Start ) != 0 )
			{
				// RESET
				NewStage();
				player[0].mode = StateId.lookingDown;
				player[1].mode = StateId.lookingDown;
				Countdown = 60;
			}

			// セレクトボタン処理
			if( ( gamePadData.Buttons & GamePadButtons.Select ) != 0 )
			{
				// to STAGE SELECT
				player[0].mode = StateId.lookingDown;
				player[1].mode = StateId.lookingDown;				
				menuCtrl.MoveSq( MenuCtrl.SqStageSelect );
				nGameStatID = 1;
			}
			
			// ゲームパッド処理
			if( bCleared == false )
			{
				// for player 1
				// for player 2
				changeMode();
			}
			
			// to move
			
			rBlock.frame();
			float fMoveSpeed = 0.25f; // スピード調整したいんだけどなあ.. yoshi
			for(int i=0; i<2; i++){
				MovePlayer(i,fMoveSpeed);	//移動
				if(0 < player[i].position.X && player[i].position.X < stage.GetLength(1)-1 &&
				   0 < player[i].position.Y && player[i].position.Y < stage.GetLength(0)-1)
				{
					WarpCol(i);					//ワープ判定
					WallCol(i);					//壁とのあたり判定
					RBlock(i);
//					RBlockCol(i);				//回転ブロック（実装中）
				}
//					if(0 < player[i].position.X && player[i].position.X < stage.GetLength(1) &&
//					   0 < player[i].position.Y && player[i].position.Y < stage.GetLength(0))
				WarpNoWall(i);
			}
			PlayerAndPlayerCol(0,1);	//プレイヤー同士のあたり判定
			
#if ( false )
			// to see stagedata
			for( int i=0; i<stage.GetLength(1); i++ )
			{
				for( int j=0; j<stage.GetLength(0); j++ )
				{
					Console.Write( stage[ j, i ] );
					Console.Write( "," );
				}
				Console.WriteLine();
			}
#endif
		}
		
		//===========================================================================
		// アプリ終了処理
		//===========================================================================
		static void AppTerm()
		{
			for(int i=0; i<2; i++){
				player[i].idle.Dispose();
				player[i].move.Dispose();
				player[i].clear.Dispose();
			}
			
			mdl_wall.Dispose();
			mdl_floor.Dispose();
			mdl_switch.Dispose();

			program.Dispose();
			graphics.Dispose();
		}


		//===========================================================================
		// Draw
		//===========================================================================
		static void Draw()
		{
			//時間管理
			int currTicks = (int)stopwatch.ElapsedTicks;
			if( frameCount ++ == 0 ) prevTicks = currTicks;
			float stepTime = ( ( currTicks - prevTicks ) / (float)Stopwatch.Frequency );
			prevTicks = currTicks;
			
			//カメラ位置
			if((float)graphics.Screen.Width/stage.GetLength(1) < (float)graphics.Screen.Height/stage.GetLength(0)){
				cameraPostion = new Vector3( (stage.GetLength(1)-1)/2.0f, 0.9f*stage.GetLength(1)+10.0f*Countdown/60, stage.GetLength(1)*0.6f+40.0f*Countdown/60 );							
			}else{
				cameraPostion = new Vector3( (stage.GetLength(1)-1)/2.0f, 1.4f*stage.GetLength(0)+10.0f*Countdown/60, stage.GetLength(0)*1.0f+40.0f*Countdown/60 );							
			}
			
						
			//カメラ情報
			Matrix4 proj = Matrix4.Perspective( FMath.Radians( 45.0f ), graphics.Screen.AspectRatio, 1.0f, 1000000.0f );
			Matrix4 view = Matrix4.LookAt( cameraPostion,
										new Vector3( (stage.GetLength(1)-1)/2.0f, 0.0f, 4.0f ),
										new Vector3( 0.0f, 1.0f, 0.0f ) );

			//環境情報
			EnvironmentData(proj,view);
			
			for(int i=0; i<2; i++) RenderPlayer(i, stepTime);	//draw Player
			DrawFloor();	// draw floor & walls
			
			// draw 2D
			graphics.Disable( EnableMode.CullFace );
			graphics.Disable( EnableMode.DepthTest );
			SampleDraw.DrawText( "SELECT button:MENU   START button:RESTART", 0xffffffff, 0, 0 );
			
			
			if( Countdown > 0 ){
				Countdown--;
				int fontWidth = SampleDraw.CurrentFont.GetTextWidth("STAGE 0");
				int fontHeight = SampleDraw.CurrentFont.Metrics.Height;
				SampleDraw.FillRect(0x7f000000, 0, SampleDraw.Height/2-fontHeight/2, SampleDraw.Width, 2*fontHeight);
				SampleDraw.DrawText( "STAGE "+ (nStageNum+1).ToString(), 0xffffffff, SampleDraw.Width/2-fontWidth/2, SampleDraw.Height/2 );	
			}
			
			if(bCleared == true){
				int fontWidthClear = SampleDraw.CurrentFont.GetTextWidth("STAGE CLEAR");
				int fontWidthNextStage = SampleDraw.CurrentFont.GetTextWidth("TAP SCREEN TO NEXT STAGE");
				int fontHeight = SampleDraw.CurrentFont.Metrics.Height;
				SampleDraw.FillRect(0x7f000000, 0, SampleDraw.Height/2-fontHeight, SampleDraw.Width, (int)(3.5f*fontHeight));
				SampleDraw.DrawText( "STAGE CLEAR!!", 0xffffffff, SampleDraw.Width/2-fontWidthClear/2, SampleDraw.Height/2-fontHeight/2 );	
				SampleDraw.DrawText( "TAP SCREEN TO NEXT STAGE", 0xffffffff, SampleDraw.Width/2-fontWidthNextStage/2, SampleDraw.Height/2+fontHeight );	
			}

			//graphics.SwapBuffers();
		}
		
		//===========================================================================
		// Render
		//===========================================================================
		public static void Render()
		{
			//graphics.Clear();
			//graphics.SetClearColor( 255,255,255, 255 );

			if( Countdown > 0 )
			{
				// Show Telop (StageNum)
			}
			
			if( bCleared )
			{
				// Show Telop (SOLVED)
			}

			graphics.SwapBuffers();	
		}
		//===========================================================================
		// moveObjX
		//===========================================================================		
		private static void moveObj( int playerNumber, Vector2 vector)
		{
			if( !(stage[ (int)(player[playerNumber].position.Y + vector.Y), (int)( player[playerNumber].position.X + vector.X ) ] == 0 ||
				stage[ (int)(player[playerNumber].position.Y + vector.Y), (int)( player[playerNumber].position.X + vector.X ) ] == 4 ||
				stage[ (int)(player[playerNumber].position.Y + vector.Y), (int)( player[playerNumber].position.X + vector.X ) ] == 5 ||
				stage[ (int)(player[playerNumber].position.Y + vector.Y), (int)( player[playerNumber].position.X + vector.X ) ] == 6 ||
				stage[ (int)(player[playerNumber].position.Y + vector.Y), (int)( player[playerNumber].position.X + vector.X ) ] == 7 ||
				stage[ (int)(player[playerNumber].position.Y + vector.Y), (int)( player[playerNumber].position.X + vector.X ) ] == 8 ||
				stage[ (int)(player[playerNumber].position.Y + vector.Y), (int)( player[playerNumber].position.X + vector.X ) ] == 9 ))
			{
				// 次の移動先がないとき
				player[playerNumber].mode += 4;
				player[playerNumber].nextPosition = player[playerNumber].position;
			}
		}
		
		//===========================================================================
		// movePlayer
		//===========================================================================
		
		private static void WallCol(int playerNumber){
			if( player[playerNumber].mode == StateId.MovingLeft)
			{
				moveObj( playerNumber, new Vector2(-1.0f,0.0f));
			}
			else if( player[playerNumber].mode == StateId.MovingRight){
				moveObj( playerNumber, new Vector2(1.0f,0.0f));
			}
			else if( player[playerNumber].mode == StateId.MovingUp){
				moveObj( playerNumber, new Vector2(0.0f,-1.0f));
			}
			else if( player[playerNumber].mode == StateId.MovingDown){
				moveObj( playerNumber, new Vector2(0.0f,1.0f));
			}
		}
		
		private static void MovePlayer(int playerNumber, float speed){
			if( player[playerNumber].mode == StateId.MovingLeft)
			{
				//move char1
				player[playerNumber].nextPosition.X -= speed;
				if((int)player[playerNumber].position.X-1 > (int)player[playerNumber].nextPosition.X) player[playerNumber].position.X--;
			}
			else if( player[playerNumber].mode == StateId.MovingRight){
				//move char1
				player[playerNumber].nextPosition.X += speed;
				if((int)player[playerNumber].position.X+1 <= (int)player[playerNumber].nextPosition.X) player[playerNumber].position.X++;
			}
			else if( player[playerNumber].mode == StateId.MovingUp){
				//move char1
				player[playerNumber].nextPosition.Y -= speed;
				if((int)player[playerNumber].position.Y-1 > (int)player[playerNumber].nextPosition.Y) player[playerNumber].position.Y--;
			}
			else if( player[playerNumber].mode == StateId.MovingDown){
				//move char1
				player[playerNumber].nextPosition.Y += speed;
				if((int)player[playerNumber].position.Y+1 <= (int)player[playerNumber].nextPosition.Y) player[playerNumber].position.Y++;
			}
		}
		
		//===========================================================================
		// changeMode
		//===========================================================================
		private static void changeMode(){
			if( (int)player[0].mode >= (int)StateId.lookingUp )
			{
				//get player1 Input while not char is not move
				if( ( gamePadData.Buttons & GamePadButtons.Left ) != 0 )
				{
					player[0].mode = StateId.MovingLeft;
				}
				else if( ( gamePadData.Buttons & GamePadButtons.Right ) != 0 ){
					player[0].mode = StateId.MovingRight;
				}
				else if( ( gamePadData.Buttons & GamePadButtons.Up ) != 0 ){
					player[0].mode = StateId.MovingUp;
				}
				else if( ( gamePadData.Buttons & GamePadButtons.Down ) != 0 ){
					player[0].mode = StateId.MovingDown;
				}
			}
			
			if( (int)player[1].mode >= (int)StateId.lookingUp )
			{
				//get player2 Input while not char is not move
				if( ( gamePadData.Buttons & GamePadButtons.Square ) != 0 )
				{
					player[1].mode = StateId.MovingLeft;
				}
				else if( ( gamePadData.Buttons & GamePadButtons.Circle ) != 0 )
				{
					player[1].mode = StateId.MovingRight;
				}
				else if( ( gamePadData.Buttons & GamePadButtons.Triangle ) != 0 )
				{
					player[1].mode = StateId.MovingUp;
				}
				else if( ( gamePadData.Buttons & GamePadButtons.Cross ) != 0 )
				{
					player[1].mode = StateId.MovingDown;
				}
			}
		}

		//===========================================================================
		// callStageInit
		//===========================================================================
		private static void callStageInit()
		{
			int goal_num = 0;
			int[] warp_num = new int[4];
			for( int i=0; i<stage.GetLength( 0 ); i++ )
			{
				for( int j=0; j<stage.GetLength( 1 ); j++ )
				{
//					player[0].position.X = ( j * 60 );
//					player[0].position.Y = ( i * 60 + 10 );

					if( stage[ i, j ] == 2 )
					{
						// player1
						player[0].position.X = j;
						player[0].position.Y = i;
						stage[ i, j ] = 0;
					}
					else if( stage[ i, j ] == 3 )
					{
						// player2
						player[1].position.X = j;
						player[1].position.Y = i;
						stage[ i, j ] = 0;
					}
					else if( stage[ i, j ] == 4 )
					{
						// goal
						goalPos[goal_num].X = j; 
						goalPos[goal_num].Y = i; 
						goal_num++;
					}else if( 5 <= stage[ i, j ] && stage[ i, j ] <= 8){
						//warp
						warpPos[warp_num[stage[ i, j ]-5],stage[ i, j ]-5].X = j;
						warpPos[warp_num[stage[ i, j ]-5],stage[ i, j ]-5].Y = i;
						warp_num[stage[ i, j ]-5]++;
						warpNum++;
					}else if( stage[ i, j ] == 9){
						
					}
				}
			}
			
			warpNum /= 2;
			player[0].nextPosition = player[0].position;
			player[1].nextPosition = player[1].position;
		}
		
		//===========================================================================
		// NewStage
		//===========================================================================
		public static void NewStage()
		{
			switch( nStageNum )
			{
				case 0: stage = (short[,])Place.PlaceData0.Clone(); break;
				case 1: stage = (short[,])Place.PlaceData1.Clone(); break;
				case 2: stage = (short[,])Place.PlaceData2.Clone(); break;
				case 3: stage = (short[,])Place.PlaceData3.Clone(); break;
				case 4: stage = (short[,])Place.PlaceData4.Clone(); break;
				case 5: stage = (short[,])Place.PlaceData5.Clone(); break;
				case 6: stage = (short[,])Place.PlaceData6.Clone(); break;
				case 7: stage = (short[,])Place.PlaceData7.Clone(); break;
				case 8: stage = (short[,])Place.PlaceData8.Clone(); break;
				case 9: stage = (short[,])Place.PlaceData9.Clone(); break;
				case 10: stage = (short[,])Place.PlaceData10.Clone(); break;
				case 11: stage = (short[,])Place.PlaceData11.Clone(); break;
				case 12: stage = (short[,])Place.PlaceData12.Clone(); break;
				case 13: stage = (short[,])Place.PlaceData13.Clone(); break;
				case 14: stage = (short[,])Place.PlaceData14.Clone(); break;
				case 15: stage = (short[,])Place.PlaceData15.Clone(); break;
				case 16: stage = (short[,])Place.PlaceData16.Clone(); break;
				case 17: stage = (short[,])Place.PlaceData17.Clone(); break;
				case 18: stage = (short[,])Place.PlaceData18.Clone(); break;
				case 19: stage = (short[,])Place.PlaceData19.Clone(); break;
				case 20: stage = (short[,])Place.PlaceData20.Clone(); break;
				case 21: stage = (short[,])Place.PlaceData21.Clone(); break;
			}

			callStageInit();

			Countdown = 60;
			bCleared = false;
		}
		
		//===========================================================================
		// DrawFloor
		//===========================================================================
		private static void DrawFloor(){
			for( int x = 0 ; x < stage.GetLength( 1 ); x++ )
			{
				for( int y = 0 ; y < stage.GetLength( 0 ); y++ )
				{
					float fAdj = 1.0f;
					Matrix4 world = Matrix4.Translation( ( x * fAdj ), 0, ( y * fAdj ) );

					int nID = stage[ y, x ];
					if( ( nID == 0 ) || ( nID == 2 ) || ( nID == 3 ) )
					{
						// floor
						// adjust position
						if( mdl_floor.BoundingSphere.W != 0.0f )
						{
							float scale = ( 1.0f / mdl_floor.BoundingSphere.W );
							world *= Matrix4.Scale( scale, scale, scale );
							world *= Matrix4.Translation( -mdl_floor.BoundingSphere.Xyz );
						}
						//  draw 3D mdl_floor
						mdl_floor.SetWorldMatrix( ref world );
						mdl_floor.Update();
						mdl_floor.Draw( graphics, program );
					}
					else if( nID == 1 )
					{
						// wall
						// adjust position
						if( mdl_wall.BoundingSphere.W != 0.0f )
						{
							float scale = ( 1.0f / mdl_wall.BoundingSphere.W );
							world *= Matrix4.Scale( scale, scale, scale ) ;
							//world *= Matrix4.Translation( -mdl_wall.BoundingSphere.Xyz );
						}
						// draw 3D mdl_wall
						mdl_wall.SetWorldMatrix( ref world );
						mdl_wall.Update();
						mdl_wall.Draw( graphics, program );
					}
					else if( nID == 4 )
					{
						// mdl_switch
						// adjust position
						if( mdl_switch.BoundingSphere.W != 0.0f )
						{
							float scale = ( 0.5f / mdl_switch.BoundingSphere.W );
							world *= Matrix4.Scale( scale, scale, scale );
							//world *= Matrix4.Translation( -mdl_wall.BoundingSphere.Xyz );
						}
						// draw 3D mdl_switch
						mdl_switch.SetWorldMatrix( ref world );
						mdl_switch.Update();
						mdl_switch.Draw( graphics, program );
					}
					else if( 5 <= nID && nID <= 8)
					{
						//warpDraw
					}
					else if( nID == 9 )
					{
						rBlock.render(graphics, program, world);
					}				
				}
			}
		}
		
		//===========================================================================
		// EnvironmentData
		//===========================================================================
		private static void EnvironmentData(Matrix4 proj, Matrix4 view){
			Vector3 litDirection1 = new Vector3( +1.0f, -1.0f, +1.0f ).Normalize();
			Vector3 litColor = new Vector3( 0.7f, 0.7f, 0.9f );
			Vector3 litAmbient = new Vector3( 0.6f, 0.6f, 0.7f );
			Vector3 fogColor = new Vector3( 0.0f, 0.5f, 0.8f );

			BasicParameters parameters = program.Parameters;
			parameters.Enable( BasicEnableMode.Lighting, true );
			parameters.Enable( BasicEnableMode.Fog, true );

			parameters.SetProjectionMatrix( ref proj );
			parameters.SetViewMatrix( ref view );
			parameters.SetLightCount( 1 );
			parameters.SetLightDirection( 0, ref litDirection1 );
			parameters.SetLightDiffuse( 0, ref litColor );
			parameters.SetLightSpecular( 0, ref litColor );
			parameters.SetLightAmbient( ref litAmbient );
			parameters.SetFogRange( 10.0f, 100.0f );
			parameters.SetFogColor( ref fogColor );

			graphics.SetViewport( 0, 0, graphics.Screen.Width, graphics.Screen.Height );
			graphics.SetClearColor( 0.0f, 0.5f, 1.0f, 0.0f );
			graphics.Clear();

			graphics.Enable( EnableMode.Blend );
			graphics.SetBlendFunc( BlendFuncMode.Add, BlendFuncFactor.SrcAlpha, BlendFuncFactor.OneMinusSrcAlpha );
			graphics.Enable( EnableMode.CullFace );
			graphics.SetCullFace( CullFaceMode.Back, CullFaceDirection.Ccw );
			graphics.Enable( EnableMode.DepthTest );
			graphics.SetDepthFunc( DepthFuncMode.LEqual, true );
		}
		
		//===========================================================================
		// RenderPlayer
		//===========================================================================
		private static void RenderPlayer(int playerNumber,float stepTime){
			Matrix4 world = Matrix4.Translation( player[playerNumber].nextPosition.X, 0, player[playerNumber].nextPosition.Y );
			switch( (int)player[playerNumber].mode )
			{
				case 0: world *= Matrix4.RotationY( -FMath.PI); 	 break;	//right
				case 1: world *= Matrix4.RotationY( -FMath.PI/2 ); 	 break; //down
				case 2:	world *= Matrix4.RotationY( 0.0f ); 		 break;	//left
				case 3: world *= Matrix4.RotationY( +FMath.PI/2 );	 break;	//up
				case 4: world *= Matrix4.RotationY( -FMath.PI); 	 break;	//right
				case 5: world *= Matrix4.RotationY( -FMath.PI/2 ); 	 break; //down	
				case 6:	world *= Matrix4.RotationY( 0.0f ); 		 break;	//left
				case 7: world *= Matrix4.RotationY( +FMath.PI/2 );	 break;	//up
			}


			if( player[playerNumber].mode >= StateId.lookingUp && ( bCleared == false ) )
			{
				// adjust model size
				if( player[playerNumber].idle.BoundingSphere.W != 0.0f ){
					float scale = ( 1.0f / player[playerNumber].idle.BoundingSphere.W );
					world *= Matrix4.Scale( scale, scale, scale );
				}

				// select motion
				if( player[playerNumber].idle.Motions.Length > 1 )
				{
					if( frameCount % 120 == 0 )
					{
						int next = ( player[playerNumber].idle.CurrentMotion + 1 ) % player[playerNumber].idle.Motions.Length;
						player[playerNumber].idle.SetCurrentMotion( next, 0.1f );
					}
				}
				
				//  draw 3D p1_mdl
				player[playerNumber].idle.SetWorldMatrix( ref world );
				player[playerNumber].idle.Animate( stepTime );
				player[playerNumber].idle.Update();
				player[playerNumber].idle.Draw( graphics, program );
			}
			else if( bCleared == false )
			{
				// adjust model size
				if( player[playerNumber].move.BoundingSphere.W != 0.0f ){	
					float scale = ( 1.0f / player[playerNumber].move.BoundingSphere.W );
					world *= Matrix4.Scale( scale, scale, scale );
				}

				// select motion
				if( player[playerNumber].move.Motions.Length > 1 )
				{
					if( frameCount % 120 == 0 )
					{
						int next = ( ( player[playerNumber].move.CurrentMotion + 1 ) % player[playerNumber].move.Motions.Length );
						player[playerNumber].move.SetCurrentMotion( next, 0.1f );
					}
				}
				
				player[playerNumber].move.SetWorldMatrix( ref world );
				player[playerNumber].move.Animate( stepTime );
				player[playerNumber].move.Update();
				player[playerNumber].move.Draw( graphics, program );
			}
			else if( bCleared == true){
				// adjust model size	
				if( player[playerNumber].clear.BoundingSphere.W != 0.0f ){	
					float scale = ( 1.0f / player[playerNumber].clear.BoundingSphere.W );
					world *= Matrix4.Scale( scale, scale, scale );
				}
				
				// select motion
				if( player[playerNumber].clear.Motions.Length > 1 )
				{
					if( frameCount % 120 == 0 )
					{
						int next = ( ( player[playerNumber].clear.CurrentMotion + 1 ) % player[playerNumber].clear.Motions.Length );
						player[playerNumber].clear.SetCurrentMotion( next, 0.1f );
					}
				}

				player[playerNumber].clear.SetWorldMatrix( ref world );
				player[playerNumber].clear.Animate( stepTime );
				player[playerNumber].clear.Update();
				player[playerNumber].clear.Draw( graphics, program );
			}
		}
		
		private static float distance(Vector2 pos1, Vector2 pos2){
			return FMath.Sqrt(FMath.Pow(pos1.X-pos2.X,2)+FMath.Pow(pos1.Y-pos2.Y,2));
		}
		
		private static void PlayerAndPlayerCol(int playerNumber1,int playerNumber2){
			if(distance(player[playerNumber1].nextPosition,player[playerNumber2].nextPosition)< 1.0){
				if(distance(player[playerNumber1].position,player[playerNumber2].position) <= 1.0){
					if(player[playerNumber1].mode < StateId.lookingUp){
						player[playerNumber1].nextPosition = player[playerNumber1].position;
						player[playerNumber1].mode += 4;
					}
					if(player[playerNumber2].mode < StateId.lookingUp){
						player[playerNumber2].nextPosition = player[playerNumber2].position;
						player[playerNumber2].mode += 4;				
					}
				}else{
					if(player[playerNumber1].mode < StateId.lookingUp){
						if(player[playerNumber1].nextPosition.X%1.0f > 0.5f){
							player[playerNumber1].nextPosition.X = FMath.Floor(player[playerNumber1].nextPosition.X);
						}else if(player[playerNumber1].nextPosition.X%1.0f <= 0.5f){
							player[playerNumber1].nextPosition.X = FMath.Truncate(player[playerNumber1].nextPosition.X);
						}
						if(player[playerNumber1].nextPosition.Y%1.0f > 0.5f){
							player[playerNumber1].nextPosition.Y = FMath.Floor(player[playerNumber1].nextPosition.Y);
						}else if(player[playerNumber1].nextPosition.X%1.0f <= 0.5f){
							player[playerNumber1].nextPosition.Y = FMath.Truncate(player[playerNumber1].nextPosition.Y);
						}
						player[playerNumber1].position = player[playerNumber1].nextPosition;
					}

					if(player[playerNumber2].mode < StateId.lookingUp){
						if(player[playerNumber2].nextPosition.X%1.0f > 0.5f){
							player[playerNumber2].nextPosition.X = FMath.Floor(player[playerNumber2].nextPosition.X);
						}else if(player[playerNumber2].nextPosition.X%1.0f <= 0.5f){
							player[playerNumber2].nextPosition.X = FMath.Truncate(player[playerNumber2].nextPosition.X);
						}
						if(player[playerNumber2].nextPosition.Y%1.0f > 0.5f){
							player[playerNumber2].nextPosition.Y = FMath.Floor(player[playerNumber2].nextPosition.Y);
						}else if(player[playerNumber2].nextPosition.X%1.0f <= 0.5f){
							player[playerNumber2].nextPosition.Y = FMath.Truncate(player[playerNumber2].nextPosition.Y);
						}
						player[playerNumber2].position = player[playerNumber2].nextPosition;
					}
				}
			}
		}
		
		private static void WarpCol(int playerNumber){
			int anotherPlayerNum = 0;
			if(playerNumber == 0){
				anotherPlayerNum = 1;
			}else if(playerNumber == 1){
				anotherPlayerNum = 0;
			}
			
			for(int i=0; i<warpNum; i++){
				if( warpPos[0,i] == player[playerNumber].position && player[playerNumber].warped == false){
					if(player[anotherPlayerNum].position != warpPos[1,i]){
						player[playerNumber].position = warpPos[1,i];
						player[playerNumber].nextPosition = warpPos[1,i];
					}
					player[playerNumber].warped = true;
				}else if( warpPos[1,i] == player[playerNumber].position  && player[playerNumber].warped == false){
					if(player[anotherPlayerNum].position != warpPos[0,i]){
						player[playerNumber].position = warpPos[0,i];
						player[playerNumber].nextPosition = warpPos[0,i];				
					}
					player[playerNumber].warped = true;
				}
			}
			
			int warpPosInitFlag = 0;
			for(int i=0; i<warpNum; i++){
				if(warpPos[0,i] == player[playerNumber].position || warpPos[1,i] == player[playerNumber].position){
					warpPosInitFlag++;
				}
			}
			if(warpPosInitFlag == 0)
			{
				player[playerNumber].warped = false;
			}
		}
		
		private static void RblockMol( int playerNumber, Vector2 vector)
		{
			switch(stage[ (int)(player[playerNumber].position.Y + vector.Y), (int)( player[playerNumber].position.X + vector.X ) ]){
			case 9:
				if(player[playerNumber].mode == StateId.MovingLeft || 
				   player[playerNumber].mode == StateId.MovingUp){
					player[playerNumber].mode += 4;
					player[playerNumber].nextPosition = player[playerNumber].position;
				}
				break;
			case 10:
				if(player[playerNumber].mode == StateId.MovingRight || 
				   player[playerNumber].mode == StateId.MovingUp){
					player[playerNumber].mode += 4;
					player[playerNumber].nextPosition = player[playerNumber].position;
				}
				break;
			case 11:
				if(player[playerNumber].mode == StateId.MovingRight || 
				   player[playerNumber].mode == StateId.MovingDown){
					player[playerNumber].mode += 4;
					player[playerNumber].nextPosition = player[playerNumber].position;
				}
				break;
			case 12:
				if(player[playerNumber].mode == StateId.MovingLeft || 
				   player[playerNumber].mode == StateId.MovingDown){
					player[playerNumber].mode += 4;
					player[playerNumber].nextPosition = player[playerNumber].position;
				}
				break;
			}
		}
		
		private static void RBlock(int playerNumber)
		{
			if( player[playerNumber].mode == StateId.MovingLeft)
			{
				RblockMol( playerNumber, new Vector2(-1.0f,0.0f));
			}
			else if( player[playerNumber].mode == StateId.MovingRight){
				RblockMol( playerNumber, new Vector2(1.0f,0.0f));
			}
			else if( player[playerNumber].mode == StateId.MovingUp){
				RblockMol( playerNumber, new Vector2(0.0f,-1.0f));
			}
			else if( player[playerNumber].mode == StateId.MovingDown){
				RblockMol( playerNumber, new Vector2(0.0f,1.0f));
			}
			
			
			switch(stage[(int)player[playerNumber].position.Y,(int)player[playerNumber].position.X]){
			case 9:
				if(player[playerNumber].mode == StateId.MovingDown){
					player[playerNumber].mode = StateId.MovingLeft;
				}else if(player[playerNumber].mode == StateId.MovingRight){
					player[playerNumber].mode = StateId.MovingUp;					
				}
				break;
			case 10:
				if(player[playerNumber].mode == StateId.MovingDown){
					player[playerNumber].mode = StateId.MovingRight;
				}else if(player[playerNumber].mode == StateId.MovingLeft){
					player[playerNumber].mode = StateId.MovingUp;					
				}
				break;
			case 11:
				if(player[playerNumber].mode == StateId.MovingLeft){
					player[playerNumber].mode = StateId.MovingDown;
				}else if(player[playerNumber].mode == StateId.MovingUp){
					player[playerNumber].mode = StateId.MovingRight;					
				}
				break;
			case 12:
				if(player[playerNumber].mode == StateId.MovingRight){
					player[playerNumber].mode = StateId.MovingDown;
				}else if(player[playerNumber].mode == StateId.MovingUp) {
					player[playerNumber].mode = StateId.MovingLeft;					
				}
				break;
			}
		}
		
		
		private static void RBlockCol(int playerNumber)
		{
			if(stage[(int)player[playerNumber].position.Y,(int)player[playerNumber].position.X] == 9){
				switch(rBlock.Rotate){
				case 0:
					if(player[playerNumber].mode == StateId.MovingDown){
						player[playerNumber].mode = StateId.MovingLeft;
						rBlock.RotateDirection = +1;
					}else if(player[playerNumber].mode == StateId.MovingRight){
						player[playerNumber].mode = StateId.MovingUp;					
						rBlock.RotateDirection = -1;
					}
					break;
				case 1:
					if(player[playerNumber].mode == StateId.MovingDown){
						player[playerNumber].mode = StateId.MovingRight;
						rBlock.RotateDirection = -1;
					}else if(player[playerNumber].mode == StateId.MovingLeft){
						player[playerNumber].mode = StateId.MovingUp;					
						rBlock.RotateDirection = +1;
					}
					break;
				case 2:
					if(player[playerNumber].mode == StateId.MovingLeft){
						player[playerNumber].mode = StateId.MovingDown;
						rBlock.RotateDirection = -1;
					}else if(player[playerNumber].mode == StateId.MovingUp){
						player[playerNumber].mode = StateId.MovingRight;					
						rBlock.RotateDirection = +1;
					}
					break;
				case 3:
					if(player[playerNumber].mode == StateId.MovingRight){
						player[playerNumber].mode = StateId.MovingDown;
						rBlock.RotateDirection = -1;
					}else if(player[playerNumber].mode == StateId.MovingUp) {
						player[playerNumber].mode = StateId.MovingLeft;					
						rBlock.RotateDirection = +1;
					}
					break;
				}
				
			}
		}
		
		private static void WarpNoWall(int playerNumber){
			if(player[playerNumber].position.X == 0 && player[playerNumber].mode == StateId.MovingLeft){
				player[playerNumber].position.X = stage.GetLength(1);
				player[playerNumber].nextPosition.X = player[playerNumber].position.X;
			}
			if(player[playerNumber].position.X == stage.GetLength(1)-1 && player[playerNumber].mode == StateId.MovingRight){
				player[playerNumber].position.X = 0;
				player[playerNumber].nextPosition.X = player[playerNumber].position.X;
			}
					
			if(player[playerNumber].position.Y == 0 && player[playerNumber].mode == StateId.MovingUp){
				player[playerNumber].position.Y = stage.GetLength(0);
				player[playerNumber].nextPosition.Y = player[playerNumber].position.Y;
			}
			if(player[playerNumber].position.Y == stage.GetLength(0)-1 && player[playerNumber].mode == StateId.MovingDown){
				player[playerNumber].position.Y = 0;
				player[playerNumber].nextPosition.Y = player[playerNumber].position.Y;
			}
		}
		
		
	}
} // namespace
