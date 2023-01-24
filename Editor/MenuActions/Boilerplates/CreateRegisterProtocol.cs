using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephVault.Unity.Boilerplates.Utils;
using AlephVault.Unity.MenuActions.Utils;
using UnityEditor;
using UnityEngine;

namespace AlephVault.Unity.Meetgard
{
    namespace MenuActions
    {
        namespace Boilerplates
        {
            public static class CreateRegisterProtocol
            {
                /// <summary>
                ///   Utility window used to create protocol files. It takes
                ///   a name only, and the three files (definition, server and
                ///   client sides) will be generated out of it.
                /// </summary>
                public class CreateRegisterProtocolWindow : EditorWindow
                {
                    private Regex nameCriterion = new Regex("^[A-Z][A-Za-z0-9_]*$");

                    // The base name to use.
                    private string baseName = "MySimpleRegister";
                    
                    // The name of the Register message type.
                    private string registerType = "Register";
                    
                    // The name of the RegisterFailed message type.
                    private string registerFailedType = "RegisterFailed";
                    
                    private void OnGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();

                        EditorGUILayout.BeginVertical();
                        
                        EditorGUILayout.LabelField(@"
This utility generates the three protocol files, with boilerplate code and instructions on how to understand that code.

The base name has to be chosen (carefully and according to the game design):
- It must start with an uppercase letter.
- It must continue with letters, numbers, and/or underscores.

The three files will be generated:
- {base name}ProtocolDefinition to define the messages and their data-types.
- {base name}ProtocolServerSide to define the handling of client messages, and sending server messages.
- {base name}ProtocolClientSide to define the handling of server messages, and sending client messages.

It also generates code for the following types: Register, RegisterFailed.

WARNING: THIS MIGHT OVERRIDE EXISTING CODE. Always use proper source code management & versioning.
".Trim(), longLabelStyle);

                        // The base name
                        EditorGUILayout.BeginHorizontal();
                        baseName = EditorGUILayout.TextField("Base name", baseName).Trim();
                        bool validBaseName = nameCriterion.IsMatch(baseName);
                        if (!validBaseName)
                        {
                            EditorGUILayout.LabelField("The base name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();

                        // The Register message type
                        EditorGUILayout.BeginHorizontal();
                        registerType = EditorGUILayout.TextField("Register message", registerType).Trim();
                        bool validRegisterType = nameCriterion.IsMatch(registerType);
                        if (!validRegisterType)
                        {
                            EditorGUILayout.LabelField("The Register type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // The RegisterFailed message type
                        EditorGUILayout.BeginHorizontal();
                        registerFailedType = EditorGUILayout.TextField("RegisterFailed message", registerFailedType).Trim();
                        bool validRegisterFailedType = nameCriterion.IsMatch(registerFailedType);
                        if (!validRegisterFailedType)
                        {
                            EditorGUILayout.LabelField("The RegisterFailed type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        bool execute = validBaseName && validRegisterType && validRegisterFailedType &&
                                       GUILayout.Button("Generate");
                        EditorGUILayout.EndVertical();
                        
                        if (execute) Execute();
                    }

                    private void Execute()
                    {
                        DumpProtocolTemplates(
                            baseName, registerType, registerFailedType
                        );
                        Close();
                    }
                }

                // Performs the full dump of the code.
                private static void DumpProtocolTemplates(
                    string basename, string registerType, string registerFailedType
                ) {
                    string directory = "Packages/com.alephvault.unity.meetgard.auth/" +
                                       "Editor/MenuActions/Boilerplates/RegisterTemplates";

                    // The protocol templates.
                    TextAsset pcsText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/SimpleRegisterProtocolClientSide.cs.txt"
                    );
                    TextAsset pssText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/SimpleRegisterProtocolServerSide.cs.txt"
                    );
                    TextAsset defText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/SimpleRegisterProtocolDefinition.cs.txt"
                    );
                    TextAsset uiText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/SimpleRegisterUI.cs.txt"
                    );

                    // The datatype templates.
                    TextAsset registerTypeText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/Register.cs.txt"
                    );
                    TextAsset registerFailedTypeText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/RegisterFailed.cs.txt"
                    );
                    
                    Dictionary<string, string> replacements = new Dictionary<string, string>
                    {
                        {"PROTOCOLCLIENTSIDE", basename + "ProtocolClientSide"},
                        {"PROTOCOLDEFINITION", basename + "ProtocolDefinition"},
                        {"REGISTER_TYPE", registerType},
                        {"REGISTERFAILED_TYPE", registerFailedType},
                    };

                    new Boilerplate()
                        .IntoDirectory("Scripts", false)
                            .IntoDirectory("Client", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("Protocols", false)
                                            .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                                pcsText, basename + "ProtocolClientSide", replacements
                                            ))
                                        .End()
                                        .IntoDirectory("UI", false)
                                            .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                                uiText, basename + "UI", replacements
                                            ))
                                        .End()
                                    .End()
                                .End()
                            .End()
                            .IntoDirectory("Server", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("Protocols", false)
                                            .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                                pssText, basename + "ProtocolServerSide", replacements
                                            ))
                                        .End()
                                    .End()
                                .End()
                            .End()
                            .IntoDirectory("Protocols", false)
                                .IntoDirectory("Messages", false)
                                    .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                        registerTypeText, registerType, replacements
                                    ))
                                    .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                        registerFailedTypeText, registerFailedType, replacements
                                    ))
                                .End()
                                .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                    defText, basename + "ProtocolDefinition", replacements
                                ))
                            .End()
                        .End();
                }
                
                /// <summary>
                ///   Opens a dialog to execute the strategy creation boilerplate.
                /// </summary>
                [MenuItem("Assets/Create/Meetgard.Auth/Boilerplates/Create Simple Register Protocol", false, 201)]
                public static void ExecuteBoilerplate()
                {
                    CreateRegisterProtocolWindow window = ScriptableObject.CreateInstance<CreateRegisterProtocolWindow>();
                    Vector2 size = new Vector2(750, 300);
                    window.position = new Rect(new Vector2(110, 250), size);
                    window.minSize = size;
                    window.maxSize = size;
                    window.titleContent = new GUIContent("Meetgard Authentication Protocol generation");
                    window.ShowUtility();
                }
            }
        }
    }
}