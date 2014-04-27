using System;
using Sce.PlayStation.HighLevel.Model;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Environment;

namespace EscapePenguins
{
	public class rotateBlock
	{
		private BasicModel Block;
		private float rotate;
		private float preRotate;
		private int rotateMoveFlag;
		private float rotateSpeed;
		
		public rotateBlock ()
		{
		}
		
		public void init(){
			Block = new BasicModel( "/Application/mdx/box.mdx", 0 );
			rotate = 0;
			preRotate = rotate;
			rotateMoveFlag = 0;
			rotateSpeed = 3.0f;	
		}
		
		public void frame(){
			if(rotateMoveFlag != 0){
				rotate += rotateMoveFlag*rotateSpeed;
				if(rotate >= preRotate+90){
					if(rotate >= 360) rotate -=360;
					preRotate = rotate;
					rotateMoveFlag = 0;
				}else if(rotate <= preRotate-90){
					if(rotate < 0) rotate +=360;
					preRotate = rotate;					
					rotateMoveFlag = 0;
				}
			}
		}
		
		public void render(GraphicsContext graphics, BasicProgram program, Matrix4 world){
			world *= Matrix4.RotationY( rotate/180*FMath.PI);
			if( Block.BoundingSphere.W != 0.0f )
			{
				float scale = ( 1.0f / Block.BoundingSphere.W );
				world *= Matrix4.Scale( scale, scale, scale );
				world *= Matrix4.Translation( -Block.BoundingSphere.Xyz );
			}
			//  draw 3D mdl_floor
			Block.SetWorldMatrix( ref world );
			Block.Update();
			Block.Draw( graphics, program );			
		}
		
		public int RotateDirection{
			set{rotateMoveFlag = value;}
			get{return rotateMoveFlag;}
		}
		
		public int Rotate{
			get{return (int)preRotate/90;}
		}
	}
}

