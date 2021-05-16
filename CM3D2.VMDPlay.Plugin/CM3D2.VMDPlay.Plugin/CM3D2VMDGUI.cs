using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CM3D2.VMDPlay.Plugin
{
	public class CM3D2VMDGUI : UnityEngine.MonoBehaviour
	{
		public Maid focusChara;

		private CameraCtrlOff cameraCtrl;

		private int windowID = 8723;

		private int dialogWindowID = 8724;

		private Rect windowRect = new Rect(0f, 300f, 630f, 510f);

		private string windowTitle = "COM3D2 VMDPlay Plugin";

		private Texture2D windowBG = new Texture2D(1, 1, (TextureFormat)5, false);

		public bool visibleGUI = true;

		private bool isVR;

		//private int currentTab;

		private float sliderLabelWidth = 100f;

		private float sliderWidth = 240f;

		private float valueLabelWidth = 70f;

		public bool pinObject;

		public Dictionary<string, Action> AdditionalMenus = new Dictionary<string, Action>();

		private static MethodInfo m_Apply = typeof(GUISkin).GetMethod("Apply", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);

		private Dictionary<string, GUIStyle> styleBackup = new Dictionary<string, GUIStyle>();

		private VMDAnimationController lastController;

		private string lastFilename;

		protected FileBrowser m_fileBrowser;

		protected Texture2D m_directoryImage;

		protected Texture2D m_fileImage;

		//private Vector2 vmdAreaScrollPos;

		private Dictionary<string, float> sliderMax = new Dictionary<string, float>();

		private void Start()
		{
			if (GameMain.Instance.VRMode)
			{
				Console.WriteLine("VMDPlayPlugin:VR Mode detect");
				isVR = true;
			}
			else
			{
				Console.WriteLine("VMDPlayPlugin:non VR Mode detect");
			}
			windowBG.SetPixel(0, 0, Color.black);
			windowBG.Apply();
			if (cameraCtrl == null)
			{
				cameraCtrl = this.gameObject.AddComponent<CameraCtrlOff>();
				cameraCtrl.ikInfoGui = this;
				cameraCtrl.enabled = true;
			}
		}

		private void OnEnable()
		{
			if (cameraCtrl != null)
			{
				cameraCtrl.enabled = true;
			}
		}

		private void OnDisable()
		{
			if (cameraCtrl != null)
			{
				cameraCtrl.enabled = false;
			}
		}

		private void Update()
		{
		}

		public void Clear()
		{
			focusChara = null;
			lastController = null;
			lastFilename = null;
		}

		private unsafe void OnGUI()
		{
			if (visibleGUI)
			{
				try
				{
					GUIStyle val = new GUIStyle(GUI.skin.window);
					if (GUI.skin.GetStyle("List Item") != null)
					{
						GUIStyle[] array = (GUIStyle[])new GUIStyle[GUI.skin.customStyles.Length + 1];
						for (int i = 0; i < GUI.skin.customStyles.Length; i++)
						{
							array[i] = GUI.skin.customStyles[i];
						}
						GUIStyle val2 = new GUIStyle(GUI.skin.button);
						val2.name = "List Item";
						array[GUI.skin.customStyles.Length] = val2;
						GUI.skin.customStyles = array;
						m_Apply.Invoke((object)GUI.skin, new object[0]);
					}
					if (isVR)
					{
						val.onNormal.background = windowBG;
						val.normal.background = windowBG;
						val.hover.background = windowBG;
						val.focused.background = windowBG;
						val.active.background = windowBG;
						val.hover.textColor = Color.white;
						val.onHover.textColor = Color.white;
					}
					if (m_fileBrowser != null)
					{
						m_fileBrowser.OnGUIAsWindow(dialogWindowID);
					}
					else
					{
						windowRect = GUI.Window(windowID, windowRect, FuncWindowGUI, windowTitle, val);
					}
				}
				catch (Exception value)
				{
					Console.WriteLine(value);
				}
			}
		}

		private float CalcAdjustedSliderMax(float value)
		{
			if (value <= 1f)
			{
				return 1f;
			}
			if (value <= 10f)
			{
				return 10f;
			}
			return 100f;
		}

		private unsafe void FuncWindowGUI(int winID)
		{
			styleBackup = new Dictionary<string, GUIStyle>();
			BackupGUIStyle("Button");
			BackupGUIStyle("Label");
			BackupGUIStyle("Toggle");
			try
			{
				if (GUIUtility.hotControl == 0)
				{
					cameraCtrl.enabled = false;
				}
				if ((int)Event.current.type == 0)
				{
					GUI.FocusControl("");
					GUI.FocusWindow(winID);
					cameraCtrl.enabled = true;
					cameraCtrl.cameraCtrlOff = true;
				}
				GUI.enabled = true;
				GUIStyle style = GUI.skin.GetStyle("Button");
				style.normal.textColor = Color.white;
				style.alignment = (TextAnchor)4;
				GUIStyle style2 = GUI.skin.GetStyle("Label");
				style2.normal.textColor = Color.white;
				style2.alignment = (TextAnchor)3;
				style2.wordWrap = false;
				GUIStyle style3 = GUI.skin.GetStyle("Toggle");
				style3.normal.textColor = Color.white;
				style3.onNormal.textColor = Color.white;
				GUILayout.BeginVertical((GUILayoutOption[])new GUILayoutOption[0]);
				if (focusChara != null && (focusChara.body0 == null || focusChara.body0.m_Bones == null))
				{
					focusChara = null;
				}
				if (focusChara == null)
				{
					focusChara = FindFirstMaid();
				}
				DrawVMDAnimationArea();
				GUILayout.EndVertical();
				GUI.DragWindow();
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
			finally
			{
				RestoreGUIStyle("Button");
				RestoreGUIStyle("Label");
				RestoreGUIStyle("Toggle");
			}
		}

		private Maid FindFirstMaid()
		{
			CharacterMgr val = GameMain.Instance.CharacterMgr;
			for (int i = 0; i < val.GetMaidCount(); i++)
			{
				Maid val2 = val.GetMaid(i);
				if (val2 != null && val2.body0.isLoadedBody)
				{
					return val2;
				}
			}
			return null;
		}

		private Maid FindPrevNextMaid(bool next)
		{
			List<Maid> list = new List<Maid>();
			CharacterMgr val = GameMain.Instance.CharacterMgr;
			for (int i = 0; i < val.GetMaidCount(); i++)
			{
				Maid val2 = val.GetMaid(i);
				if (val2 != null && val2.body0.isLoadedBody)
				{
					list.Add(val2);
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			if (focusChara != null)
			{
				int num = list.IndexOf(focusChara);
				if (num >= 0)
				{
					num += (next ? 1 : (-1));
					num = (num + list.Count) % list.Count;
					return list[num];
				}
			}
			return list[0];
		}

		private void BackupGUIStyle(string name)
		{
			GUIStyle value = new GUIStyle(GUI.skin.GetStyle(name));
			styleBackup.Add(name, value);
		}

		private void RestoreGUIStyle(string name)
		{
			if (styleBackup.ContainsKey(name))
			{
				GUIStyle val = styleBackup[name];
				GUIStyle style = GUI.skin.GetStyle(name);
				style.normal.textColor = val.normal.textColor;
				style.alignment = val.alignment;
				style.wordWrap = val.wordWrap;
			}
		}

		private void DrawVMDAnimationArea()
		{
			EnsureResourceLoaded();
			GUI.skin.GetStyle("Button");
			if (focusChara == null)
			{
				GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
				GUILayout.Label("Character not selected.", (GUILayoutOption[])new GUILayoutOption[1]
				{
					GUILayout.Width(300f)
				});
				GUILayout.Space(250f);
				if (GUILayout.Button("Close", (GUILayoutOption[])new GUILayoutOption[2]
				{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
				}))
				{
					this.gameObject.SetActive(false);
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				VMDAnimationController vMDAnimationController = VMDAnimationController.Install(focusChara);
				if (!(vMDAnimationController == null) && focusChara != null)
				{
					GUILayout.BeginVertical((GUILayoutOption[])new GUILayoutOption[0]);
					if (vMDAnimationController != lastController)
					{
						lastFilename = vMDAnimationController.lastLoadedVMD;
						lastController = vMDAnimationController;
					}
					if (lastFilename == null)
					{
						lastFilename = "";
					}
					GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
					if (GUILayout.Button("<", (GUILayoutOption[])new GUILayoutOption[2]
					{
						GUILayout.Width(20f),
						GUILayout.Height(25f)
					}))
					{
						focusChara = FindPrevNextMaid(false);
					}
					if (GUILayout.Button(">", (GUILayoutOption[])new GUILayoutOption[2]
					{
						GUILayout.Width(20f),
						GUILayout.Height(25f)
					}))
					{
						focusChara = FindPrevNextMaid(true);
					}
					GUILayout.Label((focusChara.status.lastName + focusChara.status.firstName) , (GUILayoutOption[])new GUILayoutOption[1]
					{
						GUILayout.Width(150f)
					});
					if (GUILayout.Button(vMDAnimationController.VMDAnimEnabled ? "On" : "Off", (GUILayoutOption[])new GUILayoutOption[2]
					{
						GUILayout.Width(50f),
						GUILayout.Height(25f)
					}))
					{
						vMDAnimationController.VMDAnimEnabled = !vMDAnimationController.VMDAnimEnabled;
					}
					if (vMDAnimationController.VMDAnimEnabled)
					{
						GUILayout.Space(30f);
						if (vMDAnimationController.lastLoadedVMD != null && File.Exists(vMDAnimationController.lastLoadedVMD))
						{
							GUILayout.Label(Path.GetFileNameWithoutExtension(vMDAnimationController.lastLoadedVMD), (GUILayoutOption[])new GUILayoutOption[0]);
						}
					}
					if (GUILayout.Button("Close", (GUILayoutOption[])new GUILayoutOption[2]
					{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
					}))
					{
						this.gameObject.SetActive(false);
					}
					GUILayout.EndHorizontal();
					if (vMDAnimationController.VMDAnimEnabled)
					{
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						if (GUILayout.Button("Load", (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
						}))
						{
							vMDAnimationController.LoadVMDAnimation(lastFilename);
						}
						if (GUILayout.Button("Reload", (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
						}))
						{
							vMDAnimationController.ReloadVMDAnimation();
							lastFilename = vMDAnimationController.lastLoadedVMD;
						}
						lastFilename = GUILayout.TextField(lastFilename, (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(350f),
							GUILayout.Height(25f)
						});
						if (GUILayout.Button("...", (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(30f),
							GUILayout.Height(25f)
						}))
						{
							m_fileBrowser = new FileBrowser(new Rect((float)(Screen.width / 2 - 300), 200f, 600f, 500f), "Choose .vmd File", FileSelectedCallback);
							m_fileBrowser.SelectionPattern = "*.vmd";
							m_fileBrowser.DirectoryImage = m_directoryImage;
							m_fileBrowser.FileImage = m_fileImage;
							if (File.Exists(lastFilename))
							{
								m_fileBrowser.CurrentDirectory = Path.GetDirectoryName(lastFilename);
							}
							else if (!string.IsNullOrEmpty(vMDAnimationController.lastLoadedVMD))
							{
								m_fileBrowser.CurrentDirectory = Path.GetDirectoryName(vMDAnimationController.lastLoadedVMD);
							}
							else
							{
								string stringValue = Settings.Instance.GetStringValue("DefaultDir", "", true);
								if (stringValue != null && stringValue != "")
								{
									m_fileBrowser.CurrentDirectory = stringValue;
								}
							}
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						GUILayout.Label("(Player)", (GUILayoutOption[])new GUILayoutOption[1]
						{
							GUILayout.Width(50f)
						});
						if (GUILayout.Button("Play", (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
						}))
						{

							vMDAnimationController.Play();
						}
						if (GUILayout.Button("Pause", (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
						}))
						{
							vMDAnimationController.Pause();
						}
						if (GUILayout.Button("Stop", (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
						}))
						{
							vMDAnimationController.Stop();
						}
						GUILayout.Space(30f);
						GUILayout.Label("(All)", (GUILayoutOption[])new GUILayoutOption[1]
						{
							GUILayout.Width(50f)
						});
						if (GUILayout.Button("Play", (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
						}))
						{
							VMDAnimationMgr.Instance.PlayAll();
						}
						if (GUILayout.Button("Pause", (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
						}))
						{
							VMDAnimationMgr.Instance.PauseAll();
						}
						if (GUILayout.Button("Stop", (GUILayoutOption[])new GUILayoutOption[2]
						{
							GUILayout.Width(50f),
							GUILayout.Height(25f)
						}))
						{
							VMDAnimationMgr.Instance.StopAll();
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						vMDAnimationController.speed = AddSliderWithText("vmdAnimSpeed", "Speed", vMDAnimationController.speed, 5f);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						GUILayout.Label("Loop", (GUILayoutOption[])new GUILayoutOption[1]
						{
							GUILayout.Width(40f)
						});
						if (GUILayout.Button(vMDAnimationController.Loop ? "On" : "Off", (GUILayoutOption[])new GUILayoutOption[1]
						{
							GUILayout.Width(40f)
						}))
						{
							vMDAnimationController.Loop = !vMDAnimationController.Loop;
						}
						GUILayout.Space(20f);
						GUILayout.Label("Face", (GUILayoutOption[])new GUILayoutOption[1]
						{
							GUILayout.Width(40f)
						});
						if (GUILayout.Button(vMDAnimationController.faceAnimeEnabled ? "On" : "Off", (GUILayoutOption[])new GUILayoutOption[1]
						{
							GUILayout.Width(40f)
						}))
						{
							vMDAnimationController.faceAnimeEnabled = !vMDAnimationController.faceAnimeEnabled;
						}
						GUILayout.Space(10f);
						GUILayout.Label("IK (foot)", (GUILayoutOption[])new GUILayoutOption[1]
						{
							GUILayout.Width(80f)
						});
						if (GUILayout.Button(vMDAnimationController.enableIK ? "On" : "Off", (GUILayoutOption[])new GUILayoutOption[1]
						{
							GUILayout.Width(40f)
						}))
						{
							vMDAnimationController.enableIK = !vMDAnimationController.enableIK;
						}
						GUILayout.Space(10f);
						if (vMDAnimationController.enableIK)
						{
							GUILayout.Label("IK (toe)", (GUILayoutOption[])new GUILayoutOption[1]
							{
								GUILayout.Width(60f)
							});
							if (GUILayout.Button(vMDAnimationController.IKWeight.disableToeIK ? "Off" : "On", (GUILayoutOption[])new GUILayoutOption[1]
							{
								GUILayout.Width(40f)
							}))
							{
								vMDAnimationController.IKWeight.disableToeIK = !vMDAnimationController.IKWeight.disableToeIK;
							}
						}
						/*GUILayout.Space(10f);
						GUILayout.Label("IK(Head)", (GUILayoutOption[])new GUILayoutOption[1]
						{
								GUILayout.Width(60f)
						});
						if (GUILayout.Button(vMDAnimationController.enableHeadRotate ? "On" : "Off", (GUILayoutOption[])new GUILayoutOption[1]
						{
								GUILayout.Width(40f)
						}))
						{
							vMDAnimationController.enableHeadRotate = !vMDAnimationController.enableHeadRotate;
						}*/
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						GUILayout.Label("Sync Anm to BGM", (GUILayoutOption[])new GUILayoutOption[1]
						{
								GUILayout.Width(120f)
						});
						if (GUILayout.Button(vMDAnimationController.SyncToBGM ? "On" : "Off", (GUILayoutOption[])new GUILayoutOption[1]
						{
								GUILayout.Width(40f)
						}))
						{
							vMDAnimationController.SyncToBGM = !vMDAnimationController.SyncToBGM;
						}
						/*GUILayout.Space(30f);
						GUILayout.Label("Sync BGM to Anm", (GUILayoutOption[])new GUILayoutOption[1]
						{
								GUILayout.Width(120f)
						});
						if (GUILayout.Button(vMDAnimationController.SyncToAnim ? "On" : "Off", (GUILayoutOption[])new GUILayoutOption[1]
						{
								GUILayout.Width(40f)
						}))
						{
							vMDAnimationController.SyncToAnim = !vMDAnimationController.SyncToAnim;
						}*/
						GUILayout.EndHorizontal();
						if (vMDAnimationController.enableIK)
						{
							GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
							vMDAnimationController.IKWeight.footIKPosWeight = AddSliderWithText("vmdIKFootPosWeight", "IK Weight(pos)", vMDAnimationController.IKWeight.footIKPosWeight, 1f);
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
							vMDAnimationController.IKWeight.footIKRotWeight = AddSliderWithText("vmdIKFootRotWeight", "IK Weight(rot)", vMDAnimationController.IKWeight.footIKRotWeight, 1f);
							GUILayout.EndHorizontal();
						}
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						GUILayout.Label("Config: (needs Reload): ", (GUILayoutOption[])new GUILayoutOption[1]
						{
							GUILayout.Width(150f)
						});
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						float num = AddSliderWithText("vmdCenterYPos", "(PMD)Center pos(y)", vMDAnimationController.centerBasePos.y, 15f);
						if (num != vMDAnimationController.centerBasePos.y)
						{
							vMDAnimationController.centerBasePos = new Vector3(0f, num, 0f);
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						float num2 = AddSliderWithTextFixedScale("(PMD)Hip pos(y)", vMDAnimationController.hipPositionAdjust.y, 1f, 6f);
						if (num2 != vMDAnimationController.hipPositionAdjust.y)
						{
							vMDAnimationController.hipPositionAdjust = new Vector3(0f, num2, 0f);
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						float num3 = AddSliderWithText("vmdModelScale", "Model Scale", vMDAnimationController.quickAdjust.ScaleModel, 2f);
						if (num3 != vMDAnimationController.quickAdjust.ScaleModel)
						{
							vMDAnimationController.quickAdjust.ScaleModel = num3;
						}
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						vMDAnimationController.quickAdjust.Shoulder = AddSliderWithTextFixedScale("Shoulder Tilt", vMDAnimationController.quickAdjust.Shoulder, -10f, 40f);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						vMDAnimationController.quickAdjust.ArmUp = AddSliderWithTextFixedScale("Upper Arm Tilt", vMDAnimationController.quickAdjust.ArmUp, -10f, 40f);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						vMDAnimationController.quickAdjust.ArmLow = AddSliderWithTextFixedScale("Lower Arm Tilt", vMDAnimationController.quickAdjust.ArmLow, -10f, 40f);
						GUILayout.EndHorizontal();
						//GUILayout.Label("Other Config", (GUILayoutOption[])new GUILayoutOption[1]
						//{
						//	GUILayout.Width(150f)
						//});
						//GUILayout.BeginHorizontal((GUILayoutOption[])new GUILayoutOption[0]);
						//vMDAnimationController.BgmVolume = AddSliderWithTextFixedScale("BGM volume", vMDAnimationController.BgmVolume, 0f, 1f);
						//GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
			}
		}

		private void EnsureResourceLoaded()
		{
			if (m_fileImage == null)
			{
				m_fileImage = new Texture2D(32, 32, (TextureFormat)5, false);
				//m_fileImage.LoadImage(VMDResources.file_icon);
				m_fileImage.Apply();
				m_directoryImage = new Texture2D(32, 32, (TextureFormat)5, false);
				//m_directoryImage.LoadImage(VMDResources.folder_icon);
				m_directoryImage.Apply();
			}
		}

		protected void FileSelectedCallback(string path)
		{
			m_fileBrowser = null;
			lastFilename = path;
		}

		public float AddSliderGeneral(string prop, string label, float value, float defaultMin, float defaultMax, bool fixedScale, bool useText)
		{
			GUILayout.Label(label, (GUILayoutOption[])new GUILayoutOption[1]
			{
				GUILayout.Width(sliderLabelWidth)
			});
			GUILayout.Space(5f);
			float num;
			float num2;
			if (fixedScale)
			{
				num = defaultMin;
				num2 = defaultMax;
			}
			else
			{
				num = defaultMin;
				num2 = GetSliderMax(prop, defaultMax);
			}
			float result = GUILayout.HorizontalSlider(value, num, num2, (GUILayoutOption[])new GUILayoutOption[1]
			{
				GUILayout.Width(sliderWidth)
			});
			GUILayout.Space(5f);
			if (useText)
			{
				string text = value.ToString("F4");
				string text2 = GUILayout.TextField(text, (GUILayoutOption[])new GUILayoutOption[1]
				{
					GUILayout.Width(valueLabelWidth)
				});
				if (text2 != text)
				{
					try
					{
						result = float.Parse(text2);
					}
					catch (Exception)
					{
					}
				}
			}
			else
			{
				GUILayout.Label(value.ToString("F4"), (GUILayoutOption[])new GUILayoutOption[1]
				{
					GUILayout.Width(valueLabelWidth)
				});
			}
			GUILayout.Space(5f);
			if (!fixedScale)
			{
				if (GUILayout.Button("0-1", (GUILayoutOption[])new GUILayoutOption[2]
				{
					GUILayout.Width(50f),
					GUILayout.Height(25f)
				}))
				{
					SetSliderMax(prop, 1f);
				}
				if (GUILayout.Button("0-10", (GUILayoutOption[])new GUILayoutOption[2]
				{
					GUILayout.Width(50f),
					GUILayout.Height(25f)
				}))
				{
					SetSliderMax(prop, 10f);
				}
				if (GUILayout.Button("x2", (GUILayoutOption[])new GUILayoutOption[2]
				{
					GUILayout.Width(50f),
					GUILayout.Height(25f)
				}))
				{
					SetSliderMax(prop, GetSliderMax(prop, 1f) * 2f);
				}
			}
			return result;
		}

		public float AddSliderWithLabel(string prop, string label, float value, float defaultMax)
		{
			return AddSliderGeneral(prop, label, value, 0f, defaultMax, false, false);
		}

		public float AddSliderWithText(string prop, string label, float value, float defaultMax)
		{
			return AddSliderGeneral(prop, label, value, 0f, defaultMax, false, true);
		}

		public float AddSliderWithLabelFixedScale(string label, float value, float min, float max)
		{
			return AddSliderGeneral("", label, value, min, max, true, false);
		}

		public float AddSliderWithTextFixedScale(string label, float value, float min, float max)
		{
			return AddSliderGeneral("", label, value, min, max, true, true);
		}

		public float GetSliderMax(string key, float defaultMax)
		{
			if (sliderMax.ContainsKey(key))
			{
				return sliderMax[key];
			}
			return defaultMax;
		}

		public void SetSliderMax(string key, float value)
		{
			sliderMax[key] = value;
		}
		

	}
}
