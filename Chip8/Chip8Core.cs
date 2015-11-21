// Really Bad Chip-8 Emulator Processor Core.
// Created by Kraust, 11-21-2015


using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chip8
{
   class Chip8Core
   {
      private ushort opcode;
      private byte[] memory = new byte[4096];
      private byte[] V = new byte[16];
      private ushort I;
      private ushort pc;
      public byte[] gfx = new byte[64 * 32];
      private byte delay_timer;
      private byte sound_timer;
      private ushort[] stack = new ushort[16];
      private ushort sp;
      private byte[] key = new byte[16];
      private bool drawFlag;

      public bool drawFlagSet()
      {
         return drawFlag;
      }

      public void drawFlagClear()
      {
         drawFlag = false;
      }

      public byte[] getDisplay()
      {
         return gfx;
      }

      ushort vreg;
      ushort vreg2;
      byte vval;

      private byte[] chip8_fontset =
      {
           0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
           0x20, 0x60, 0x20, 0x20, 0x70, // 1
           0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
           0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
           0x90, 0x90, 0xF0, 0x10, 0x10, // 4
           0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
           0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
           0xF0, 0x10, 0x20, 0x40, 0x40, // 7
           0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
           0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
           0xF0, 0x90, 0xF0, 0x90, 0x90, // A
           0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
           0xF0, 0x80, 0x80, 0x80, 0xF0, // C
           0xE0, 0x90, 0x90, 0x90, 0xE0, // D
           0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
           0xF0, 0x80, 0xF0, 0x80, 0x80  // F
      };

      public Chip8Core()
      {
         pc = 0x200;
         opcode = 0;
         I = 0;
         sp = 0;

         for (int i = 0; i < memory.Length; i++)
         {
            memory[i] = 0;
         }

         for (int i = 0; i < V.Length; i++)
         {
            V[i] = 0;
         }

         for (int i = 0; i < stack.Length; i++)
         {
            stack[i] = 0;
         }

         for (int i = 0; i < gfx.Length; i++)
         {
            gfx[i] = 0;
         }

         for(int i = 0; i < 80; i++)
         {
            memory[i] = chip8_fontset[i];
         }

         for(int i = 0; i < key.Length; i++)
         {
            key[i] = 0xFF;
         }

         delay_timer = 0;
         sound_timer = 0;
      }

      //string inputFilename = @"F:\myChip8-bin-src\tetris.c8";
      public void Load()
      {
         OpenFileDialog open = new OpenFileDialog();
         open.ShowDialog();

         if(open.FileName != "")
         {
            try
            {
               byte[] buffer = File.ReadAllBytes(open.FileName);
               for (int i = 0; i < buffer.Length; i++)
               {
                  memory[i + 512] = buffer[i];
               }
            }
            catch
            {
               Environment.Exit(0);
            }
         }
         else
         {
            Environment.Exit(0);
         }

      }

      public void Cycle()
      {
         opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);
         Debug.WriteLine("0x{0:X}", opcode);

         switch(opcode & 0xF000)
         {
            case 0x0000:
               switch(opcode & 0x00FF)
               {
                  case 0xE0:
                     for (int i = 0; i < gfx.Length; i++)
                     {
                        gfx[i] = 0;
                     }
                     pc += 2;
                     break;
                  case 0xEE:
                     --sp;
                     pc = (ushort)(stack[sp] + 2);
                     break;
                  default:
                     Debug.WriteLine("Invalid Opcode 0x{0:X}", opcode);
                     break;
               }
               break;
            case 0x1000:
               pc = (ushort)(opcode & 0xFFF);
               break;
            case 0x2000:
               stack[sp] = pc;
               ++sp;
               pc = (ushort)(opcode & 0xFFF);
               break;
            case 0x3000:
               vreg = (ushort)((opcode >> 8) & 0xF);
               vval = (byte)(opcode & 0xFF);
               if(V[vreg] == vval)
               {
                  pc += 4;
               }
               else
               {
                  pc += 2;
               }
               break;
            case 0x4000:
               vreg = (ushort)((opcode >> 8) & 0xF);
               vval = (byte)(opcode & 0xFF);
               if (V[vreg] != vval)
               {
                  pc += 4;
               }
               else
               {
                  pc += 2;
               }
               break;
            case 0x6000:
               vreg = (ushort)((opcode >> 8) & 0xF);
               vval = (byte)(opcode & 0xFF);
               V[vreg] = vval;
               pc += 2;
               break;
            case 0x7000:
               vreg = (ushort)((opcode >> 8) & 0xF);
               vval = (byte)(opcode & 0xFF);
               V[vreg] += vval;
               pc += 2;
               break;
            case 0x8000:
               switch(opcode & 0xF)
               {
                  case 0:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     vreg2 = (ushort)((opcode >> 4) & 0xF);
                     V[vreg] = V[vreg2];
                     pc += 2;
                     break;
                  case 2:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     vreg2 = (ushort)((opcode >> 4) & 0xF);
                     V[vreg] = (byte)(V[vreg] & V[vreg2]);
                     pc += 2;
                     break;
                  case 3:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     vreg2 = (ushort)((opcode >> 4) & 0xF);
                     V[vreg] = (byte)(V[vreg] ^ V[vreg2]);
                     pc += 2;
                     break;
                  case 4:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     vreg2 = (ushort)((opcode >> 4) & 0xF);
                     if((ushort)(V[vreg] + V[vreg2]) > byte.MaxValue)
                     {
                        V[0xF] = 1;
                     }
                     V[vreg] = (byte)(V[vreg] + V[vreg2]);
                     pc += 2;
                     break;
                  case 5:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     vreg2 = (ushort)((opcode >> 4) & 0xF);
                     if ((short)(V[vreg] + V[vreg2]) < 0)
                     {
                        V[0xF] = 0;
                     }
                     else
                     {
                        V[0xF] = 1;
                     }
                     V[vreg] = (byte)(V[vreg] - V[vreg2]);
                     pc += 2;
                     break;
                  case 6:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     vreg2 = (ushort)((opcode >> 4) & 0xF);
                     V[0xF] = (byte)(V[vreg] >> 7);
                     V[vreg] = (byte)(V[vreg] >> 1);
                     pc += 2;
                     break;
                  default:
                     Debug.WriteLine("Invalid Opcode 0x{0:X}", opcode);
                     break;
               }
               break;
            case 0x9000:
               vreg = (ushort)((opcode >> 8) & 0xF);
               vreg2 = (ushort)((opcode >> 4) & 0xF);
               if (V[vreg] != V[vreg2])
               {
                  pc += 4;
               }
               else
               {
                  pc += 2;
               }
               break;
            case 0xA000:
               I = (ushort)(opcode & 0xFFF);
               pc += 2;
               break;
            case 0xC000:
               Random rnd = new Random();
               vreg = (ushort)((opcode >> 8) & 0xF);
               vval = (byte)(opcode & 0xFF);
               V[vreg] = (byte)(vval & rnd.Next(byte.MinValue, byte.MaxValue));
               pc += 2;
               break;
            case 0xD000:
               ushort x = V[(opcode & 0x0F00) >> 8];
               ushort y = V[(opcode & 0x00F0) >> 4];
               ushort height = (ushort)(opcode & 0x000F);
               ushort pixel;

               V[0xF] = 0;
               for (int yline = 0; yline < height; yline++)
               {
                  pixel = memory[I + yline];
                  for (int xline = 0; xline < 8; xline++)
                  {
                     if ((pixel & (0x80 >> xline)) != 0)
                     {
                        try
                        {
                           if (gfx[(x + xline + ((y + yline) * 64))] == 1)
                           {
                              V[0xF] = 1;
                           }
                           gfx[x + xline + ((y + yline) * 64)] ^= 1;
                        }
                        catch
                        {
                           // nothing
                        }
                     }
                  }
               }

               drawFlag = true;
               pc += 2;

               break;
            case 0xE000:
               switch (opcode & 0xFF)
               {
                  case 0x9E:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     if (key[V[vreg]] == 0)
                     {
                        pc += 4;
                     }
                     else
                     {
                        pc += 2;
                     }
                     break;
                  case 0xA1:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     if( key[V[vreg]] != 0)
                     {
                        pc += 4;
                     }
                     else
                     {
                        pc += 2;
                     }
                     break;
                  default:
                     Debug.WriteLine("Invalid Opcode 0x{0:X}", opcode);
                     break;
               }
               break;
            case 0xF000:
               switch(opcode & 0xFF)
               {
                  case 0x07:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     V[vreg] = delay_timer;
                     pc += 2;
                     break;
                  case 0x15:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     delay_timer = V[vreg];
                     pc += 2;
                     break;
                  case 0x18:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     sound_timer = V[vreg];
                     pc += 2;
                     break;
                  case 0x1E:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     I += V[vreg];
                     pc += 2;
                     break;
                  case 0x29:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     I = V[vreg];
                     pc += 2;
                     break;
                  case 0x33:
                     memory[I] = (byte)(V[(opcode & 0x0F00) >> 8] / 100);
                     memory[I + 1] = (byte)((V[(opcode & 0x0F00) >> 8] / 10) % 10);
                     memory[I + 2] = (byte)((V[(opcode & 0x0F00) >> 8] % 100) % 10);
                     pc += 2;
                     break;
                  case 0x55:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     for (int i = 0; i < vreg; i++)
                     {
                        memory[I + i] = V[i];
                     }
                     pc += 2;
                     break;
                  case 0x65:
                     vreg = (ushort)((opcode >> 8) & 0xF);
                     for (int i = 0; i < vreg; i++)
                     {
                        V[i] = memory[I + i];
                     }
                     pc += 2;
                     break;
                  default:
                     Debug.WriteLine("Invalid Opcode 0x{0:X}", opcode);
                     break;
               }
               break;
            default:
               Debug.WriteLine("Invalid Opcode 0x{0:X}", opcode);
               break;
         }

         if(delay_timer > 0)
         {
            --delay_timer;
         }

         if(sound_timer > 0)
         {
            if (sound_timer == 1)
            {
               Debug.WriteLine("BEEP");
            }
            --sound_timer;
         }
      }

      public void HandleKey(byte val, bool k)
      {
         if(k)
         {
            key[val] = 0x00;
         }
         else
         {
            key[val] = 0xFF;
         }
      }
   }
}
