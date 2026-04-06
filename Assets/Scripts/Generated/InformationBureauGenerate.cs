using UnityEngine;	
namespace Netologia.Quest
{
	public static partial class InformationBureau
	{
		public static void CallOnDialogClose()
		{
			Debug.Log($"<b>[Bureau]</b>: <i>OnDialogClose()</i>");
			if(OnDialogClose == null)
			{
				Debug.Log("<b>[Bureau]</b>: <i>OnDialogClose</i> is <b>NULL</b>");
				return;
			}
			OnDialogClose.Invoke();
		}

		public static void CallOnQuestChanged(Netologia.Quest.QuestInfo arg0)
		{
			Debug.Log($"<b>[Bureau]</b>: <i>OnQuestChanged({arg0.ToString()})</i>");
			if(OnQuestChanged == null)
			{
				Debug.Log("<b>[Bureau]</b>: <i>OnQuestChanged</i> is <b>NULL</b>");
				return;
			}
			OnQuestChanged.Invoke(arg0);
		}

		public static void CallOnCharacterFocused(System.Boolean arg0, System.String arg1)
		{
			Debug.Log($"<b>[Bureau]</b>: <i>OnCharacterFocused({arg0.ToString()}, {arg1.ToString()})</i>");
			if(OnCharacterFocused == null)
			{
				Debug.Log("<b>[Bureau]</b>: <i>OnCharacterFocused</i> is <b>NULL</b>");
				return;
			}
			OnCharacterFocused.Invoke(arg0, arg1);
		}

		public static void CallOnShowDialog(System.String arg0, System.Boolean arg1)
		{
			Debug.Log($"<b>[Bureau]</b>: <i>OnShowDialog({arg0.ToString()}, {arg1.ToString()})</i>");
			if(OnShowDialog == null)
			{
				Debug.Log("<b>[Bureau]</b>: <i>OnShowDialog</i> is <b>NULL</b>");
				return;
			}
			OnShowDialog.Invoke(arg0, arg1);
		}

	}
}
