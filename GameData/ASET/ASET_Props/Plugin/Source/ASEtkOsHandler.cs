using System;
using UnityEngine;
using KSP;
using kOS;
using kOS.Safe.Screen;
using kOS.Module;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using kOS.Safe.UserIO;

namespace ASETkOsHandler
{

	public class ASETkOsHandler : InternalModule
	{
		private List<string> kOsBuffer = new List<string>();
		private bool curBlinkTime = true;
		private bool caps = false;
		private int consoleWidth;
		private int consoleHeight;

		private bool processorInstalled = false;
		private int processorId = 0;
		private List<kOSProcessor> processors;
		private List<SharedObjects> procShares;
		private bool initialized = false;
		private bool isPowered = false;

		private int cycle = 0;

		private char[] keymap = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '_', 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', '+', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', '[', ']', 'z', 'x', 'c', 'v', 'b', 'n', 'm', ',', '.', ';', '`', '\\', ' ', '/', '\'' };
		private char[] uKeymap = { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', '=', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', '{', '}', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', '<', '>', ':', '~', '|', ' ', '?', '"' };

		public string RenderkOs(int screenWidth, int screenHeight)
		{
			consoleWidth = screenWidth;
			consoleHeight = screenHeight;

			if (!initialized)
			{
				Initialize();
			}

			if (!processorInstalled) return "No kOS Processor Installed.";

            Debug.Log("TEST1");
			updateVars();
            updatekOsBuffer();


			string outputBuffer = "";
			int bufferRows = System.Math.Min(kOsBuffer.Count, consoleHeight);

			for (int i = 1; i <= bufferRows; i++)
			{
				outputBuffer = outputBuffer.Insert(0, kOsBuffer[kOsBuffer.Count - i]);
			}

			outputBuffer = outputBuffer.Replace("[", "[[");


            cycle++;
			if (cycle >= 5)
			{
				curBlinkTime = !curBlinkTime;
				cycle = 0;
			}

			return outputBuffer;
		}

		public void processInput(char inputChar)
		{
			procShares[processorId].Window.ProcessOneInputChar(inputChar, null);
			curBlinkTime = true;
			cycle = 0;
        }

		public void ButtonProcessor(int buttonID)
		{
			switch (buttonID)
			{
				case 0:
					processInput((char)UnicodeCommand.UPCURSORONE);
					break;
				case 1:
					processInput((char)UnicodeCommand.DOWNCURSORONE);
					break;
                case 5:
					processInput((char)UnicodeCommand.RIGHTCURSORONE);
					break;
				case 6:
					processInput((char)UnicodeCommand.LEFTCURSORONE);
                    break;
				case 21:
					caps = !caps;
					break;
				case 22:
					processInput((char)UnicodeCommand.DELETELEFT);
					break;
				case 23:
					processInput((char)UnicodeCommand.DELETERIGHT);
					break;
				case 24:
					processInput((char)UnicodeCommand.BREAK);
					break;
				case 25:
					processInput((char)UnicodeCommand.HOMECURSOR);
					break;
				case 26:
					processInput((char)UnicodeCommand.ENDCURSOR);
					break;
				case 27:
					processInput((char)UnicodeCommand.STARTNEXTLINE);
					break;
                case var expression when (buttonID >= 28 && buttonID <= 76):
					int i = buttonID - 28;
					if (caps) processInput(uKeymap[i]);
					else processInput(keymap[i]);

					break;
			}
		}

		public void Initialize()
		{
			processors = GetProcessorList();

			procShares = new List<SharedObjects>();
			foreach (kOSProcessor processor in processors)
			{
				processorInstalled = true;

				procShares.Add(GetProcessorShare(processor));
			}

			initialized = true;

		}

		public void updatekOsBuffer()
		{

			if(!processorInstalled)
			{
				return;
			}


			if (procShares[processorId] != null)
			{
				bool showCur = curBlinkTime && isPowered;

				string curString = " ";
				if (showCur) curString = "_";

				if (procShares[processorId].Screen.ColumnCount != consoleWidth || procShares[processorId].Screen.RowCount != consoleHeight)
				{
					procShares[processorId].Screen.SetSize(consoleHeight, consoleWidth);
				}

				Debug.Log("TEST2");

                Debug.Log("curRow: " + procShares[processorId].Screen.CursorRowShow.ToString() + ", curCol: " + procShares[processorId].Screen.CursorColumnShow.ToString());

                List <IScreenBufferLine> buffer = new ScreenSnapShot(procShares[processorId].Screen).Buffer;
				for(int i = 0; i < buffer.Count; i++)
				{
					string row = buffer[i].ToString();
					Debug.Log("ROW: " + i.ToString());
					if (i == procShares[processorId].Screen.CursorRowShow)
					{
                        Debug.Log("OCR");
                        int column = procShares[processorId].Screen.CursorColumnShow;
						row = row.Insert(column, curString);
					}
					kOsBuffer.Add(row + "\n");
				}
			}

		}

		public void updateVars()
		{
			isPowered = procShares[processorId].Window.IsPowered;
		}

		List<kOSProcessor> GetProcessorList()
		{
			return this.vessel.FindPartModulesImplementing<kOSProcessor>();
		}

		SharedObjects GetProcessorShare(kOSProcessor processor)
		{
			FieldInfo sharedField = typeof(kOSProcessor).GetField("shared", BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic);
			var proc_shared = sharedField.GetValue(processor);
			return (SharedObjects)proc_shared;
		}

	}
}
