using System;
using System.Collections.Generic;

using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Input;
using Sce.PlayStation.HighLevel.GameEngine2D;
using Sce.PlayStation.HighLevel.GameEngine2D.Base;
using Sce.PlayStation.Core.Imaging;


namespace Sample
{
	/// <summary>
	/// メニュー操作クラス
	/// </summary>
	public class MenuCtrl
	{
		/// <summary>
		/// シーケンス：何もしない
		/// </summary>
		public const int SqNone = 0;
		/// <summary>
		/// シーケンス：タイトル開始
		/// </summary>
		public const int SqTitle = 1;
		/// <summary>
		/// シーケンス：タイトルユーザ入力
		/// </summary>
		public const int SqTitleUserInput = 2;
		/// <summary>
		/// シーケンス：クレジット開始
		/// </summary>
		public const int SqCredit = 3;
		/// <summary>
		/// シーケンス：クレジットユーザ入力
		/// </summary>
		public const int SqCreditUserInput = 4;
		/// <summary>
		/// シーケンス：ストーリー開始
		/// </summary>
		public const int SqStory = 5;
		/// <summary>
		/// シーケンス：ストーリーユーザ入力
		/// </summary>
		public const int SqStoryUserInput = 6;
		/// <summary>
		/// シーケンス：ステージ選択開始
		/// </summary>
		public const int SqStageSelect = 7;
		/// <summary>
		/// シーケンス：ステージ選択ユーザ入力
		/// </summary>
		public const int SqStageSelectUserInput = 8;
		/// <summary>
		/// シーケンス：ステージクリアストーリー開始
		/// </summary>
		public const int SqClearStroy = 9;
		/// <summary>
		/// シーケンス：ステージクリアストーリーユーザ入力
		/// </summary>
		public const int SqClearStroyUserInput = 10;
		/// <summary>
		/// シーケンス：ゲームクリア開始
		/// </summary>
		public const int SqGameClear = 11;
		/// <summary>
		/// シーケンス：ゲームクリアユーザ入力
		/// </summary>
		public const int SqGameClearUserInput = 12;
		
		/// <summary>
		/// ファイル名
		/// </summary>
		private static readonly string FileNameTitleBack = "/Application/resources/title_back.png";
		private static readonly string FileNameTitleStart = "/Application/resources/title_start.png";
		private static readonly string FileNameTitleCredit = "/Application/resources/title_credit.png";
		private static readonly string FileNameCreditBack = "/Application/resources/credit_back.png";
		private static readonly string FileNameStoryBack = "/Application/resources/story_back.png";
		private static readonly string FileNameStageSelectBack = "/Application/resources/stage_select_back.png";
		private static readonly string FileNameStageSelectReturn = "/Application/resources/stage_select_return.png";
		private static readonly string FileNameStageSelectButton = "/Application/resources/stage_select_button{0}.png";
		private static readonly string FileNameClearStroyBack = "/Application/resources/clear_story_back.png";
		private static readonly string FileNameGameClearBack = "/Application/resources/game_clear_back.png";
		
		/// <summary>
		/// スクリーンの横幅
		/// </summary>
		private static readonly int ScreenWidth = 960;
		/// <summary>
		/// スクリーンの縦幅
		/// </summary>
		private static readonly int ScreenHeight = 544;
		/// <summary>
		/// タイトルSTARTのポジション
		/// </summary>
		private static readonly Vector2 TitleStartPos = new Vector2(450f, 170);
		/// <summary>
		/// タイトルCREDITのポジション
		/// </summary>
		private static readonly Vector2 TitleCreditPos = new Vector2(450f, 70f);
		/// <summary>
		/// ステージ選択ボタンの開始ポジション
		/// </summary>
		private static readonly Vector2 StageSelectButtonStartPos = new Vector2(260f, 425f);
		/// <summary>
		/// ステージ選択ボタンの配置時の移動ポジション
		/// </summary>
		private static readonly Vector2 StageSelectButtonMoveSize = new Vector2(200f, 170f);
		/// <summary>
		/// ステージ選択の戻るボタンのポジション
		/// </summary>
		private static readonly Vector2 StageSelectReturnPos = new Vector2(75f, 519f);
		
		/// <summary>
		/// シーン
		/// </summary>
		private Scene scene = null;
		/// <summary>
		/// キー入力値
		/// </summary>
		private GamePadData key;
		/// <summary>
		/// タッチ入力値
		/// </summary>
		private List<TouchData> touches = null;
		/// <summary>
		/// シーケンス
		/// </summary>
		private int sq = SqNone;
		/// <summary>
		/// 選択ステージ番号
		/// </summary>
		private int selectStageNo = 0;
		/// <summary>
		/// タイトル背景スプライト
		/// </summary>
		private SpriteUV titleBackSprite = null;
		/// <summary>
		/// タイトルSTARTスプライト
		/// </summary>
		private SpriteUV titleStartSprite = null;
		/// <summary>
		/// タイトルCREDITスプライト
		/// </summary>
		private SpriteUV titleCreditSprite = null;
		/// <summary>
		/// CREDIT背景スプライト
		/// </summary>
		private SpriteUV creditBackSprite = null;
		/// <summary>
		/// ストーリー背景スプライト
		/// </summary>
		private SpriteUV storyBackSprite = null;
		/// <summary>
		/// ステージ選択背景スプライト
		/// </summary>
		private SpriteUV stageSelectBackSprite = null;
		/// <summary>
		/// ステージ選択の戻るボタン
		/// </summary>
		private SpriteUV stageSelectReturnSprite = null;
		/// <summary>
		/// ステージ選択ボタンスプライトリスト
		/// </summary>
		private SpriteUV[] stageSelectButtonSprites = new SpriteUV[12];
		/// <summary>
		/// ステージクリアストーリー背景スプライト
		/// </summary>
		private SpriteUV clearStroyBackSprite = null;
		/// <summary>
		/// ゲームクリア背景スプライト
		/// </summary>
		private SpriteUV gameClearBackSprite = null;
		
		private float stageSelectHeight = 0;
		private float vectorHeight = 0; 
		private float touchPreHeight = 0; 
		private	int PlusAplha = 0;
		private	int buttonHeight = 0;
		private bool scroleFlag = false;
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name='context'>グラフィックオブジェクト</param>
		public MenuCtrl( GraphicsContext context )
		{
			Director.Initialize( 500, 400, context );
			
			scene = new Scene();
			scene.Camera.SetViewFromViewport();
			
			Director.Instance.GL.Context.SetClearColor( Colors.Grey20 );
			Director.Instance.RunWithScene( scene, true );
			
			MoveSq( SqTitle );
		}
		
		/// <summary>
		/// 選択ステージ番号
		/// </summary>
		public int SelectStageNo
		{
			get { return selectStageNo; }
		}
		
		/// <summary>
		/// シーン更新処理
		/// </summary>
		public void Update()
		{
			key = GamePad.GetData( 0 );
			touches = Touch.GetData( 0 );
			
			switch( sq )
			{
				case SqTitle:
					DoSqTitle();
					break;
				case SqTitleUserInput:
					DoSqTitleUserInput();
					break;
				case SqCredit:
					DoSqCredit();
					break;
				case SqCreditUserInput:
					DoSqCreditUserInput();
					break;
				case SqStory:
					DoSqStory();
					break;
				case SqStoryUserInput:
					DoSqStoryUserInput();
					break;
				case SqStageSelect:
					DoSqStageSelect();
					break;
				case SqStageSelectUserInput:
					DoSqStageSelectUserInput();
					break;
				case SqClearStroy:
					DoSqClearStroy();
					break;
				case SqClearStroyUserInput:
					DoSqClearStroyUserInput();
					break;
				case SqGameClear:
					DoSqGameClear();
					break;
				case SqGameClearUserInput:
					DoSqGameClearUserInput();
					break;
			}
		}
		
		/// <summary>
		/// シーン描画処理
		/// </summary>
		public void Render ()
		{
			Director.Instance.Update();
			Director.Instance.Render();
			Director.Instance.GL.Context.SwapBuffers();
			Director.Instance.PostSwap();
		}
		
		/// <summary>
		/// シーケンス：タイトル開始
		/// </summary>
		public void DoSqTitle()
		{
			// ユーザ入力へ
			titleBackSprite = CreateSprite(FileNameTitleBack);
			titleStartSprite = CreateSprite(FileNameTitleStart, TitleStartPos);
			titleCreditSprite = CreateSprite(FileNameTitleCredit, TitleCreditPos);
			
			MoveSq(SqTitleUserInput);
		}
		
		/// <summary>
		/// シーケンス：タイトルユーザ入力
		/// </summary>
		public void DoSqTitleUserInput()
		{
			foreach( TouchData touch in touches )
			{
				if( touch.Status == TouchStatus.Down )
				{
					Vector2 pos = GetTouchPos( touch );
					bool isCollideStart = ( IsCollide( touch, titleStartSprite ) );
					bool isCollideCredit = ( IsCollide( touch, titleCreditSprite ) );
					if( isCollideStart || isCollideCredit )
					{
						// ストーリー or クレジットへ
						RemoveNode( titleBackSprite );
						RemoveNode( titleStartSprite );
						RemoveNode( titleCreditSprite );
						MoveSq( isCollideStart? SqStory: SqCredit );
						break;
					}
				}
			}
		}
		
		/// <summary>
		/// シーケンス：クレジット開始
		/// </summary>
		public void DoSqCredit()
		{
			// ユーザ入力へ
			creditBackSprite = CreateSprite( FileNameCreditBack );
			MoveSq( SqCreditUserInput );
		}
		
		/// <summary>
		/// シーケンス：クレジットユーザ入力
		/// </summary>
		public void DoSqCreditUserInput()
		{
			foreach( TouchData touch in touches )
			{
				if( touch.Status == TouchStatus.Down )
				{
					// タイトルへ
					RemoveNode( creditBackSprite );
					MoveSq( SqTitle );
					break;
				}
			}
		}
		
		/// <summary>
		/// シーケンス：ストーリー開始
		/// </summary>
		public void DoSqStory()
		{
			// ユーザ入力へ
			storyBackSprite = CreateSprite( FileNameStoryBack );
			MoveSq( SqStoryUserInput );
		}
		
		/// <summary>
		/// シーケンス：ストーリーユーザ入力
		/// </summary>
		public void DoSqStoryUserInput()
		{
			foreach( TouchData touch in touches )
			{
				if( touch.Status == TouchStatus.Down )
				{
					// ステージ選択へ
					RemoveNode( storyBackSprite );
					MoveSq( SqStageSelect );
				}
			}
		}
		
		/// <summary>
		/// シーケンス：ステージ選択開始
		/// </summary>
		public void DoSqStageSelect()
		{
			// ユーザ入力へ
			stageSelectBackSprite = CreateSprite( FileNameStageSelectBack );
			stageSelectReturnSprite = CreateSprite( FileNameStageSelectReturn, StageSelectReturnPos );
			for( int i = 0; i < stageSelectButtonSprites.Length; i++ )
			{
				int x = ( i % 3 );
				int y = ( i / 3 );
				Vector2 pos = new Vector2(
					StageSelectButtonStartPos.X + x * StageSelectButtonMoveSize.X,
					StageSelectButtonStartPos.Y - y * StageSelectButtonMoveSize.Y - stageSelectHeight );
				stageSelectButtonSprites[i] = CreateSprite( string.Format( FileNameStageSelectButton, i ), pos );
			}
			
			MoveSq( SqStageSelectUserInput );
		}
		
		/// <summary>
		/// シーケンス：ステージ選択ユーザ入力
		/// </summary>
		public void DoSqStageSelectUserInput()
		{
			GamePadData gamePadData;
			gamePadData = GamePad.GetData( 0 );
			if((gamePadData.Buttons & GamePadButtons.Up) != 0){
				stageSelectHeight++;
			}
			if((gamePadData.Buttons & GamePadButtons.Down) != 0){
				stageSelectHeight--;
			}
//			Console.WriteLine(stageSelectHeight);
			
			/*
			foreach( SpriteUV sprite in stageSelectButtonSprites )
			{
				RemoveNode( sprite );
			}
			for( int i = 0; i < stageSelectButtonSprites.Length; i++ )
			{
				int x = ( i % 3 );
				int y = ( i / 3 );
				Vector2 pos = new Vector2(
					StageSelectButtonStartPos.X + x * StageSelectButtonMoveSize.X,
					StageSelectButtonStartPos.Y - y * StageSelectButtonMoveSize.Y - buttonHeight );
				stageSelectButtonSprites[i] = CreateSprite( string.Format( FileNameStageSelectButton, i+PlusAplha ), pos );
			}
			
			if(touches.Count == 0){
				if(-10 > vectorHeight || vectorHeight > 10){
					if(vectorHeight > 0){
						vectorHeight -= 1.5f;
					}
					if(vectorHeight < 0){
						vectorHeight += 1.5f;
					}
					stageSelectHeight += vectorHeight;
				}
			}
			*/
			
			foreach( TouchData touch in touches )
			{
				if( touch.Status == TouchStatus.Down )
				{
					scroleFlag = true;
					Vector2 pos = GetTouchPos( touch );
					
					touchPreHeight = pos.Y;

					if( IsCollide( touch, stageSelectReturnSprite ) )
					{
						RemoveNode( stageSelectBackSprite );
						RemoveNode( stageSelectReturnSprite );
						foreach( SpriteUV sprite in stageSelectButtonSprites )
						{
							RemoveNode( sprite );
						}
						MoveSq( SqTitle );
					}
					else
					{
						for( int i = 0; i < stageSelectButtonSprites.Length; i++ )
						{
							if( IsCollide( touch, stageSelectButtonSprites[i] ) )
							{
								// ゲーム画面へ
								RemoveNode( stageSelectBackSprite );
								RemoveNode( stageSelectReturnSprite );
								foreach( SpriteUV sprite in stageSelectButtonSprites )
								{
									RemoveNode( sprite );
								}
								MoveSq( SqNone );
								MoveStage( i);
//								MoveStage( i+PlusAplha );
								break;
							}
						}
					}
				/*
				}else if( touch.Status == TouchStatus.Move){
					Vector2 pos = GetTouchPos( touch );
					stageSelectHeight += touchPreHeight - pos.Y;
					touchPreHeight = pos.Y;
				}else if( touch.Status == TouchStatus.Up){
					if(scroleFlag == true){
						Vector2 pos = GetTouchPos( touch );
						vectorHeight = (touchPreHeight - pos.Y); 
					}*/
				}
			}
//			MoveIcon();
//			Console.WriteLine(stageSelectHeight);
		}
		
		private void MoveIcon(){
			if(0 <= stageSelectHeight){
				buttonHeight = 0;
				stageSelectHeight = 0;
			}else if(-170 <= stageSelectHeight && stageSelectHeight < 0){
				PlusAplha = 0;		
				buttonHeight = (int)stageSelectHeight;
			}else if(-340 <= stageSelectHeight && stageSelectHeight < -170){
				PlusAplha = 3;							
				buttonHeight = (int)stageSelectHeight + 170;
			}else if(-510 <= stageSelectHeight && stageSelectHeight < -340){
				PlusAplha = 6;							
				buttonHeight = (int)stageSelectHeight + 340;
			}else if(stageSelectHeight < -510){
				PlusAplha = 6;							
				stageSelectHeight = -510;
				buttonHeight = (int)stageSelectHeight + 340;
			}			
		}
			
		
		/// <summary>
		/// シーケンス：ステージクリアストーリー開始
		/// </summary>
		public void DoSqClearStroy()
		{
			// ユーザ入力へ
			clearStroyBackSprite = CreateSprite( FileNameClearStroyBack );
			MoveSq( SqClearStroyUserInput );
		}
		
		/// <summary>
		/// シーケンス：ステージクリアストーリーユーザ入力
		/// </summary>
		public void DoSqClearStroyUserInput()
		{
			foreach( TouchData touch in touches )
			{
				if( touch.Status == TouchStatus.Down )
				{
					// ステージ選択に戻る
					RemoveNode( clearStroyBackSprite );
					MoveSq( SqNone );
					MoveStage( selectStageNo );
				}
			}
		}
		
		/// <summary>
		/// シーケンス：ゲームクリア開始
		/// </summary>
		public void DoSqGameClear()
		{
			// ユーザ入力へ
			gameClearBackSprite = CreateSprite( FileNameGameClearBack );
			MoveSq( SqGameClearUserInput );
		}
		
		/// <summary>
		/// シーケンス：ゲームクリアユーザ入力
		/// </summary>
		public void DoSqGameClearUserInput()
		{
			foreach( TouchData touch in touches )
			{
				if( touch.Status == TouchStatus.Down )
				{
					// CREDITSへ
					RemoveNode( gameClearBackSprite );
					MoveSq( SqCredit );
				}
			}
		}
		
		/// <summary>
		/// シーケンス移動
		/// </summary>
		public void MoveSq( int sq )
		{
			this.sq = sq;
		}
		
		/// <summary>
		/// ステージ移動
		/// </summary>
		/// <param name='stageNo'>ステージナンバー</param>
		public void MoveStage( int stageNo )
		{
			selectStageNo = stageNo;

			EscapePenguins.nGameStatID = 0;
			EscapePenguins.nStageNum = stageNo;
			EscapePenguins.NewStage();

			Console.WriteLine( "stageNo = " + stageNo );
		}
		
		/// <summary>
		/// スプライト作成
		/// </summary>
		/// <param name='fileName'>ファイル名</param>
		/// <returns>スプライト</returns>
		public SpriteUV CreateSprite(string fileName)
		{
			return CreateSprite(fileName, scene.Camera.CalcBounds().Center);
		}
		
		/// <summary>
		/// スプライト作成
		/// </summary>
		/// <param name='fileName'>ファイル名</param>
		/// <param name='pos'>ポジション</param>
		/// <returns>スプライト</returns>
		public SpriteUV CreateSprite(string fileName, Vector2 pos)
		{
			SpriteUV sprite = new SpriteUV();
			sprite.TextureInfo = new TextureInfo(new Texture2D(fileName, false));
			sprite.Quad.S = sprite.TextureInfo.TextureSizef; 
			sprite.CenterSprite();
			sprite.Position = pos;
			scene.AddChild(sprite);
			return sprite;
		}
		
		/// <summary>
		/// ノード削除
		/// </summary>
		/// <param name='node'>ノード</param>
		public void RemoveNode(Node node)
		{
			if(node is SpriteUV)
			{
				((SpriteUV)node).TextureInfo.Texture.Dispose();
			}
			scene.RemoveChild(node, true);
		}
		
		/// <summary>
		/// タッチのポジション取得
		/// </summary>
		/// <returns>タッチのポジション</returns>
		/// <param name='touch'>タッチデータ</param>
		public Vector2 GetTouchPos(TouchData touch)
		{
			return new Vector2(
				(touch.X + 0.5f) * ScreenWidth,
				(-touch.Y + 0.5f) * ScreenHeight); // 座標系が左下なのでマイナス
		}
		
		/// <summary>
		/// 当たり判定を取得
		/// </summary>
		/// <param name='touch'>タッチデータ</param>
		/// <param name='sprite'>スプライト</param>
		/// <returns>当たっていた場合 true</returns>
		public bool IsCollide(TouchData touch, SpriteUV sprite)
		{
			Vector2 touchPos = GetTouchPos(touch);
			Vector2 spritePos = sprite.Position;
			float width = sprite.TextureInfo.Texture.Width;
			float height = sprite.TextureInfo.Texture.Height;
			return (
				touchPos.X > spritePos.X - width / 2 && 
				touchPos.X < spritePos.X + width / 2 && 
				touchPos.Y > spritePos.Y - height / 2 && 
				touchPos.Y < spritePos.Y + height / 2);
		}
	}
}

