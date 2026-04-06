using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Netologia.Quest.Editor
{
	public class LayerAndTagGenerator : EditorWindow
	{
#region Constants

        private static StringBuilder _code;

        private const string c_LayerSpawnPoint = "%LAYERS_COUNT_SPAWN%";
        private const string c_TagSpawnPoint = "%TAGS_COUNT_SPAWN%";

        private const string c_NamespacePoint = "%NAMESPACE_POINT%";
        private const string c_LayerEnumPoint = "%LAYER_NAME_POINT%";
        private const string c_LayerNameValuePoint = "%LAYER_PROPERTY_VALUE_POINT%";
        private const string c_TagEnumPoint = "%TAG_NAME_POINT%";
        private const string c_TagEnumValuePoint = "%TAG_ENUM_VALUE_POINT%";
        
#endregion
        
        private const string _baseNamespace = "Netologia.Quest";

        private (int, string)[] _layers;

        [SerializeField]
        private TextAsset _template;
        [SerializeField]
        private TextAsset _target;
        
        private FileInfo GetFullFilePath(Object target)
            => new (Path.Combine(Application.dataPath.Replace("Assets", ""), AssetDatabase.GetAssetPath(target)));

        [MenuItem("Netologia/Quest/Generate Tags and Layers constants", priority = 2)]
        private static void Call()
        {
            var window = GetWindow<LayerAndTagGenerator>();
            window.InternalGenerate();
            window.Close();
        }
        
        private void InternalGenerate()
        {
            //---Configuration---
            var layers = UnityEditorInternal.InternalEditorUtility.layers;
            var count = layers.Length;

            //---Configuration---
            _layers = new (int, string)[count];
            for (int i = 0; i < count; i++)
            {
                _layers[i] = (LayerMask.NameToLayer(layers[i]), layers[i].Replace(" ", ""));
            }
            
            _code = new StringBuilder();
            var pathInfo = GetFullFilePath(_template);
            using (var stream = File.OpenRead(pathInfo.FullName))
                using(var reader = new StreamReader(stream))
                    while(!reader.EndOfStream)
                    {
                        LineParse(reader.ReadLine());
                    }

            //Create File
            pathInfo = GetFullFilePath(_target);
            var path = pathInfo.FullName;
            if (!pathInfo.Exists) Directory.CreateDirectory(path);

            var result = _code.ToString();
            File.WriteAllText(path, result);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void LineParse(string line)
        {
            //Create Layers
            if(line.Contains(c_LayerSpawnPoint))
            {
                Layer(line.Replace(c_LayerSpawnPoint, ""));
                return;
            }
            //Create Tags
            else if (line.Contains(c_TagSpawnPoint))
            {
                Tags(line.Replace(c_TagSpawnPoint, ""));
                return;
            }

            //Replace namespace
            _code.AppendLine(line.Replace(c_NamespacePoint, _baseNamespace));
        }

        private void Layer(string line)
        {
            for(int i = 0; i < _layers.Length; i++)
            {
                var copy = line.Substring(0);
                _code.AppendLine(copy.Replace(c_LayerEnumPoint, _layers[i].Item2).Replace(c_LayerNameValuePoint, _layers[i].Item1.ToString()));
			}
		}

        private void Tags(string line)
        {
            var tags = UnityEditorInternal.InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                var copy = line.Clone() as string;
                _code.AppendLine(copy.Replace(c_TagEnumPoint, tags[i]).Replace(c_TagEnumValuePoint, i.ToString()));
            }
        }
    }
}