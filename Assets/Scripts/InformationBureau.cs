using System;
using System.Reflection;

namespace Netologia.Quest
{
	/// <summary>
	/// Кастомная реализация паттерна Broadcasters с простой кодогенерацией
	/// </summary>
	public static partial class InformationBureau
	{
		public static event Action OnDialogClose;
		public static event Action<QuestInfo> OnQuestChanged;
		public static event Action<bool, string> OnCharacterFocused;
		public static event Action<string, bool> OnShowDialog;

		static InformationBureau()
		{
			var error = false;
			var mainType = typeof(Delegate);
			var type = typeof(InformationBureau);
			var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (var field in fields)
			{
				type = field.FieldType;
				if (!mainType.IsAssignableFrom(type))
				{
					var info = $"<b>Field:</b> {field.Name} | <b>Type:</b> {type.Name}";
					LogUtility.Error(typeof(InformationBureau), "Constructor",
						$"It can't contain fields with types other than Action and Action<>\n{info}");
					error = true;
				}

				//Пример сборки кастомных методов времени выполнения с высокой производительностью 
				//const string log = "<b>[Bureau]</b>: call <i>{0}</i>";
				//var arg = Expression.Constant(string.Format(log, field.Name), typeof(string));
				//var call = Expression.Call(null, checker, arg);
				//var lambda = Expression.Lambda(call);
				//field.SetValue(typeof(InformationBureau), lambda.Compile());
			}
			
			if (error)
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Time.timeScale = 0f;
#endif
		}

		//Вызываемый через экспрессию метод
		//private static void Check(string value)
		//	=> Debug.Log(value);
	}
}