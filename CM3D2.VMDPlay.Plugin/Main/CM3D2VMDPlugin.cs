using BepInEx;
using CM3D2.VMDPlay.Plugin.Utill;
using COM3D2.Lilly.Plugin;
using HarmonyLib;
using UnityEngine;
//using UnityInjector.Attributes;

namespace CM3D2.VMDPlay.Plugin
{
	//[PluginFilter("COM3D2x64")]
	//[PluginName("COM3D2.VMDPlay.Plugin")]
	//[PluginVersion("0.3.11.0")]

	[BepInPlugin("COM3D2.VMDPlay.Plugin", "COM3D2.VMDPlay.Plugin", "0.3.11.0")]// ���� ��Ģ ����. �ݵ�� 2~4���� ���ڱ������� �ؾ���. ���ؼ��� ���о����
	[BepInProcess("COM3D2x64.exe")]
	public class CM3D2VMDPlugin : BaseUnityPlugin//UnityInjector.PluginBase
	{
		public static CM3D2VMDPlugin Instance;

		public const string NAME = "COM3D2.VMDPlay.Plugin";

		public const string VERSION = "0.3.11.0";

		public CM3D2VMDPlugin()
        {
			Instance = this;
			SongMotionDic.path= CM3D2VMDPlugin.Instance.Config.ConfigFilePath + @"\CM3D2.VMDPlay.Plugin.json";
			Settings.config = this.Config;
			Debug.Log("https://github.com/customordermaid3d2/CM3D2.VMDPlay.Plugin");
		}

		private void Awake()
		{
			SongMotionDic.Deserialize();
			Harmony.CreateAndPatchAll(typeof(CharacterMgrPatch));
		}

		private void Start()
		{
			GameObject val = new GameObject("COM3D2VMDPlayPlugin");
			Object.DontDestroyOnLoad(val);
			VMDAnimationMgr.Install(val);
			DebugHelper.Install(val);
		}

		public void togGUI()
		{
			VMDAnimationMgr.Instance.ToggleGUI();
		}

		

		private void OnLevelWasLoaded(int level)
		{
		}
		
	}
}
