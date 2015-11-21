// Really Bad Chip-8 Emulator Graphics Core.
// Created by Kraust, 11-21-2015

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace Chip8
{
   public class Game1 : Game
   {
      GraphicsDeviceManager graphics;
      SpriteBatch spriteBatch;
      Chip8Core chip8;
      private byte[] gfx = new byte[64 * 32];
      Texture2D rect;
      int blockSize = 10;


      public Game1()
      {
         graphics = new GraphicsDeviceManager(this);
         Content.RootDirectory = "Content";
      }

      protected override void Initialize()
      {
         this.TargetElapsedTime = new TimeSpan(0,0,0,0,3);
         graphics.PreferredBackBufferHeight = 320;
         graphics.PreferredBackBufferWidth = 640;
         graphics.ApplyChanges();
         rect = new Texture2D(graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
         rect.SetData<Color>(new Color[] { Color.White });
         chip8 = new Chip8Core();
         chip8.Load();


         base.Initialize();
      }

      protected override void LoadContent()
      {
         spriteBatch = new SpriteBatch(GraphicsDevice);

      }

      protected override void UnloadContent()
      {

      }

      protected override void Update(GameTime gameTime)
      {
         if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

         HandleInput();
         chip8.Cycle();

         base.Update(gameTime);
      }

      protected override void Draw(GameTime gameTime)
      {
         GraphicsDevice.Clear(Color.CornflowerBlue);

         if (chip8.drawFlagSet())
         {
            gfx = chip8.getDisplay();
            chip8.drawFlagClear();
         }

         spriteBatch.Begin();
         for (int i = 0; i < 32; i++)
         {
            for (int j = 0; j < 64; j++)
            {
               if(gfx[(i*64) + j] != 0)
               {
                  spriteBatch.Draw(rect, new Rectangle(j * blockSize, i * blockSize, blockSize, blockSize), Color.White);
               }
               else
               {
                  spriteBatch.Draw(rect, new Rectangle(j * blockSize, i * blockSize, blockSize, blockSize), Color.Black);
               }
            }
         }

         spriteBatch.End();
         base.Draw(gameTime);
      }

      private Keys[] keys =
      {
         Keys.D1,
         Keys.D2,
         Keys.D3,
         Keys.D4,
         Keys.Q,
         Keys.W,
         Keys.E,
         Keys.R,
         Keys.A,
         Keys.S,
         Keys.D,
         Keys.F,
         Keys.Z,
         Keys.X,
         Keys.C,
         Keys.V,
      };

      private byte[] mapping =
      {
         0x00,
         0x01,
         0x02,
         0x03,
         0x04,
         0x05,
         0x06,
         0x07,
         0x08,
         0x09,
         0x0A,
         0x0B,
         0x0C,
         0x0D,
         0x0E,
         0x0F
      };

      void HandleInput()
      {
         for(int i = 0; i < keys.Length; i++)
         {
            if (Keyboard.GetState().IsKeyDown(keys[i]))
            {
               chip8.HandleKey(mapping[i], true);
            }
            else
            {
               chip8.HandleKey(mapping[i], false);
            }

         }
      }
   }
}
