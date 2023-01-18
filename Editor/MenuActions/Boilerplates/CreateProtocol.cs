using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephVault.Unity.MenuActions.Utils;
using UnityEditor;
using UnityEngine;

namespace AlephVault.Unity.Meetgard
{
    namespace MenuActions
    {
        namespace Boilerplates
        {
            public static class CreateProtocol
            {
                /// <summary>
                ///   Utility window used to create protocol files. It takes
                ///   a name only, and the three files (definition, server and
                ///   client sides) will be generated out of it.
                /// </summary>
                public class CreateProtocolWindow : EditorWindow
                {
                    // The base name to use.
                    private Regex baseNameCriterion = new Regex("^[A-Z][A-Za-z0-9_]*$");
                    private string baseName = "MyCustom";

                    private void OnGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();
                        GUIStyle captionLabelStyle = MenuActionUtils.GetCaptionLabelStyle();
                        GUIStyle indentedStyle = MenuActionUtils.GetIndentedStyle();

                        EditorGUILayout.BeginVertical();
                        baseName = EditorGUILayout.TextField("Base name", baseName).Trim();
                        bool validBaseName = baseNameCriterion.IsMatch(baseName);
                        if (!validBaseName)
                        {
                            EditorGUILayout.LabelField("The base name is invalid!", indentedStyle);
                        }
                        
                        bool execute = validBaseName && GUILayout.Button("Generate");
                        EditorGUILayout.EndVertical();
                        
                        if (execute) Execute();
                    }

                    private void Execute()
                    {
                        DumpProtocolTemplates(baseName);
                        Close();
                    }
                }

                // Performs the full dump of the code.
                private static void DumpProtocolTemplates(string basename)
                {
                    
                }
            }
        }
    }
}