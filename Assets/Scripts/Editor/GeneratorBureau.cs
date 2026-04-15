using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Codice.Client.Common.GameUI;

namespace Netologia.Quest.Editor
{
    public class GeneratorBureau
    {

        private const string c_path = "Scripts/Generated/InformationBureauGenerate.cs";
        private const string c_messageHeader = "<b>[Bureau]</b>: <i>{0}";


        [MenuItem("Netologia/Quest/Generate Bureau", priority = 1)]
        private static void Generate()
        {
            var mainBuilder = new StringBuilder();
            var argBuilder = new StringBuilder();
            var callBuilder = new StringBuilder();
            var messageBuilder = new StringBuilder();
            var bureauType = typeof(InformationBureau);

            mainBuilder.AppendLine("using UnityEngine;\t");
            mainBuilder.AppendLine($"namespace {bureauType.Namespace}\n{{");
            mainBuilder.AppendLine("///___________________________");
            mainBuilder.AppendLine("///---- AUTOGENERETED FILE----");
            mainBuilder.AppendLine("///___________________________");
            mainBuilder.AppendLine("\tpublic static partial class InformationBureau\n\t{");

            var events = bureauType.GetEvents(BindingFlags.Static | BindingFlags.Public);

            foreach (var info in events)
            {
                var name = info.Name;
                FindAllArgs(info, argBuilder, callBuilder, messageBuilder);
                mainBuilder.AppendLine($"\t\tpublic static void Call{name}({argBuilder})");
                mainBuilder.AppendLine($"\t\t{{");
                mainBuilder.AppendLine($"\t\t\tDebug.Log($\"{messageBuilder}\");");
                mainBuilder.AppendLine($"\t\t\tif({name} == null)\n\t\t\t{{");
                mainBuilder.AppendLine($"\t\t\t\tDebug.Log9\"{string.Format(c_messageHeader, $"{name}</i> is <b>NULL</b>")}\");");
                mainBuilder.AppendLine($"\t\t\t\treturn;\n\t\t\t}}");
                mainBuilder.AppendLine($"\t\t\t{name}.Invoke({callBuilder});");
                mainBuilder.AppendLine($"\t\t}}\n");
            }

            mainBuilder.AppendLine("\t}\n}");

            var path = Path.Combine(Application.dataPath, c_path);
            if (!File.Exists(path)) File.Create(path).Close();
            File.WriteAllText(path, mainBuilder.ToString());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void FindAllArgs(EventInfo info, StringBuilder args, StringBuilder calls, StringBuilder mes)
        {
            const string spliter = ", ";

            var type = info.EventHandlerType;
            args.Clear(); calls.Clear(); mes.Clear();
            if (!type.IsGenericType)
            {
                mes.Append($"{string.Format(c_messageHeader, info.Name)}()</i>");
                return;
            }

            mes.Append($"{string.Format(c_messageHeader, info.Name)}(");
            var array = type.GenericTypeArguments;
            for (int i = 0, iMax = array.Length; i < iMax; i++)
            {
                args.Append($"{array[i].FullName} arg{i}");
                calls.Append($"arg{i}");
                mes.Append($"{{arg{i}.ToString()}}");

                if (i + 1 < iMax)
                {
                    args.Append(spliter);
                    calls.Append(spliter);
                    mes.Append(spliter);
                }
            }
            mes.Append(")</i>");
        }
    }
}