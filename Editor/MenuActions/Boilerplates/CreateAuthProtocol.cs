using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephVault.Unity.Boilerplates.Utils;
using AlephVault.Unity.MenuActions.Types;
using AlephVault.Unity.MenuActions.Utils;
using UnityEditor;
using UnityEngine;

namespace AlephVault.Unity.Meetgard
{
    namespace MenuActions
    {
        namespace Boilerplates
        {
            public static class CreateAuthProtocol
            {
                /// <summary>
                ///   Utility window used to create protocol files. It takes
                ///   a name only, and the three files (definition, server and
                ///   client sides) will be generated out of it.
                /// </summary>
                public class CreateAuthProtocolWindow : SmartEditorWindow
                {
                    private Regex nameCriterion = new Regex("^[A-Z][A-Za-z0-9_]*$");
                    private Regex existingNameCriterion = new Regex("^([A-Za-z][A-Za-z0-9_]*\\.)*[A-Za-z][A-Za-z0-9_]*$");

                    // The base name to use.
                    private string baseName = "MySimpleAuth";
                    
                    // The name of the Login message type.
                    private string loginType = "Login";
                    
                    // The name of the LoginFailed message type.
                    private string loginFailedType = "LoginFailed";
                    
                    // The name of the Kicked message type.
                    private string kickedType = "Kicked";
                    
                    // The name of the account id type. It should exist
                    // in AlephVault.Unity.Binary, System, a primitive
                    // type, or be fully qualified.
                    private string accountIdType = "string";
                    
                    // The name of the AccountData type.
                    private string accountDataType = "AccountData";
                    
                    // The name of the AccountPreviewData type (might become
                    // a message, and thus, will be an ISerializable type
                    // as the Login, LoginFailed and Kicked types).
                    private string accountPreviewDataType = "AccountPreviewData";

                    // If true, this component will treat the current game
                    // object as the "offlineUI".
                    private bool useSelfOfflineUI;

                    // If true, tells to generate the UI code using TMPro.
                    private bool useTMProInUI;
                    
                    protected override float GetSmartWidth()
                    {
                        return 730;
                    }
                    
                    protected override void OnAdjustedGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();
                        GUIStyle captionLabelStyle = MenuActionUtils.GetCaptionLabelStyle();
                        GUIStyle indentedStyle = MenuActionUtils.GetIndentedStyle();

                        EditorGUILayout.LabelField(@"
This utility generates the three protocol files, with boilerplate code and instructions on how to understand that code.

The base name has to be chosen (carefully and according to the game design):
- It must start with an uppercase letter.
- It must continue with letters, numbers, and/or underscores.

The three files will be generated:
- {base name}ProtocolDefinition to define the messages and their data-types.
- {base name}ProtocolServerSide to define the handling of client messages, and sending server messages.
- {base name}ProtocolClientSide to define the handling of server messages, and sending client messages.

It also generates code for the following types: Login, LoginFailed, Kicked, AccountPreviewData and AccountData.
When choosing an Account ID type, ensure the type is valid (you might need to manually fix the code otherwise).

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

                        // The Login message type
                        EditorGUILayout.BeginHorizontal();
                        loginType = EditorGUILayout.TextField("Login message", loginType).Trim();
                        bool validLoginType = nameCriterion.IsMatch(loginType);
                        if (!validLoginType)
                        {
                            EditorGUILayout.LabelField("The Login type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // The LoginFailed message type
                        EditorGUILayout.BeginHorizontal();
                        loginFailedType = EditorGUILayout.TextField("LoginFailed message", loginFailedType).Trim();
                        bool validLoginFailedType = nameCriterion.IsMatch(loginFailedType);
                        if (!validLoginFailedType)
                        {
                            EditorGUILayout.LabelField("The LoginFailed type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // The Kicked message type
                        EditorGUILayout.BeginHorizontal();
                        kickedType = EditorGUILayout.TextField("Kicked message", kickedType).Trim();
                        bool validKickedType = nameCriterion.IsMatch(kickedType);
                        if (!validKickedType)
                        {
                            EditorGUILayout.LabelField("The Kicked type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // The Account ID message type
                        EditorGUILayout.BeginHorizontal();
                        accountIdType = EditorGUILayout.TextField("Account ID type", accountIdType).Trim();
                        bool validAccountIdType = existingNameCriterion.IsMatch(accountIdType);
                        if (!validAccountIdType)
                        {
                            EditorGUILayout.LabelField("The Account ID type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // The AccountPreviewData message type
                        EditorGUILayout.BeginHorizontal();
                        accountPreviewDataType = EditorGUILayout.TextField("AccountPreviewData type", accountPreviewDataType).Trim();
                        bool validAccountPreviewDataType = nameCriterion.IsMatch(accountPreviewDataType);
                        if (!validAccountPreviewDataType)
                        {
                            EditorGUILayout.LabelField("The AccountPreviewData type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // The AccountData type
                        EditorGUILayout.BeginHorizontal();
                        accountDataType = EditorGUILayout.TextField("AccountData type", accountDataType).Trim();
                        bool validAccountDataType = nameCriterion.IsMatch(accountDataType);
                        if (!validAccountDataType)
                        {
                            EditorGUILayout.LabelField("The AccountData type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // A flag telling to use the same component as the "offlineUI" interface.
                        EditorGUILayout.BeginHorizontal();
                        useSelfOfflineUI = EditorGUILayout.ToggleLeft(
                            "Mode: Component's game object will serve as offline UI",
                            useSelfOfflineUI
                        );
                        EditorGUILayout.EndHorizontal();
                        
                        // A flag telling to use, in the UI, the TMPro package
                        EditorGUILayout.BeginHorizontal();
                        useTMProInUI = EditorGUILayout.ToggleLeft(
                            "Use TMPro classes in the generated UI component",
                            useTMProInUI
                        );
                        EditorGUILayout.EndHorizontal();
                        
                        if (validBaseName && validLoginType && validLoginFailedType &&
                            validKickedType && validAccountIdType && validAccountPreviewDataType &&
                            validAccountDataType) SmartButton("Generate", Execute);
                    }

                    private void Execute()
                    {
                        DumpProtocolTemplates(
                            baseName, loginType, loginFailedType, kickedType, accountIdType,
                            accountPreviewDataType, accountDataType, useTMProInUI, useSelfOfflineUI
                        );
                    }
                }

                // Performs the full dump of the code.
                private static void DumpProtocolTemplates(
                    string basename, string loginType, string loginFailedType, string kickedType,
                    string accountIdType, string accountPreviewDataType, string accountDataType,
                    bool usingTMPro, bool useSelfOfflineUI
                ) {
                    string directory = "Packages/com.alephvault.unity.meetgard.auth/" +
                                       "Editor/MenuActions/Boilerplates/LoginTemplates";

                    // The protocol templates.
                    TextAsset pcsText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/SimpleAuthProtocolClientSide.cs.txt"
                    );
                    TextAsset pssText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/SimpleAuthProtocolServerSide.cs.txt"
                    );
                    TextAsset defText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/SimpleAuthProtocolDefinition.cs.txt"
                    );
                    TextAsset uiText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + (useSelfOfflineUI ? "/SimpleAuthUI-self.cs.txt" : "/SimpleAuthUI.cs.txt")
                    );

                    // The datatype templates.
                    TextAsset loginTypeText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/Login.cs.txt"
                    );
                    TextAsset loginFailedTypeText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/LoginFailed.cs.txt"
                    );
                    TextAsset kickedTypeText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/Kicked.cs.txt"
                    );
                    TextAsset accountPreviewDataTypeText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/AccountPreviewData.cs.txt"
                    );
                    TextAsset accountDataTypeText = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/AccountData.cs.txt"
                    );
                    
                    Dictionary<string, string> replacements = new Dictionary<string, string>
                    {
                        {"PROTOCOLCLIENTSIDE", basename + "ProtocolClientSide"},
                        {"PROTOCOLDEFINITION", basename + "ProtocolDefinition"},
                        {"LOGIN_TYPE", loginType},
                        {"LOGINFAILED_TYPE", loginFailedType},
                        {"KICKED_TYPE", kickedType},
                        {"ACCOUNTID_TYPE", accountIdType},
                        {"ACCOUNTPREVIEWDATA_TYPE", accountPreviewDataType},
                        {"ACCOUNTDATA_TYPE", accountDataType},
                        {"USING_TMPRO_COMMENT", usingTMPro ? "" : "// "},
                        {"TMPRO_PREFIX", usingTMPro ? "TMP_" : ""}
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
                                    .IntoDirectory("Types", false)
                                        .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                            accountDataTypeText, accountDataType, replacements
                                        ))
                                    .End()
                                .End()
                            .End()
                            .IntoDirectory("Protocols", false)
                                .IntoDirectory("Messages", false)
                                    .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                        accountPreviewDataTypeText, accountPreviewDataType, replacements
                                    ))
                                    .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                        loginTypeText, loginType, replacements
                                    ))
                                    .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                        loginFailedTypeText, loginFailedType, replacements
                                    ))
                                    .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                        kickedTypeText, kickedType, replacements
                                    ))
                                .End()
                                .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                    defText, basename + "ProtocolDefinition", replacements
                                ))
                            .End()
                        .End();
                }
                
                /// <summary>
                ///   Opens a dialog to execute the protocol creation boilerplate.
                /// </summary>
                [MenuItem("Assets/Create/Aleph Vault/Meetgard.Auth/Boilerplates/Create Simple Auth Protocol", false, 201)]
                public static void ExecuteBoilerplate()
                {
                    CreateAuthProtocolWindow window = ScriptableObject.CreateInstance<CreateAuthProtocolWindow>();
                    Vector2 size = new Vector2(750, 394);
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