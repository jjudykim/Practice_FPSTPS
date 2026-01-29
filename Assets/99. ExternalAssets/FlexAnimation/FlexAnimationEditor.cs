#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

public class FlexBaseEditor : Editor
{
    protected SerializedProperty modulesProp;
    
    // Editor State
    protected static bool isAdvancedMode = false;
    protected static string copiedModuleJson;
    protected static Type copiedModuleType;

    // Styles
    private GUIStyle headerStyle;
    private GUIStyle boxStyle;

    protected virtual void OnEnable()
    {
        modulesProp = serializedObject.FindProperty("modules");
    }

    protected void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.alignment = TextAnchor.MiddleLeft;
            headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : Color.black;
        }

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(EditorStyles.helpBox);
            boxStyle.padding = new RectOffset(10, 10, 10, 10);
            boxStyle.margin = new RectOffset(0, 0, 5, 5);
        }
    }

    public override void OnInspectorGUI()
    {
        InitStyles();
        serializedObject.Update();

        DrawToolbar();
        EditorGUILayout.Space(5);

        DrawContent();

        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void DrawContent()
    {
        DrawModules();
        DrawAddModuleButton();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        string[] modes = { "Basic Mode", "Expert Mode" };
        int modeIndex = isAdvancedMode ? 1 : 0;
        int newIndex = GUILayout.Toolbar(modeIndex, modes, GUILayout.Height(24), GUILayout.Width(200));
        
        if (newIndex != modeIndex)
        {
            isAdvancedMode = (newIndex == 1);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        if (isAdvancedMode)
        {
            EditorGUILayout.HelpBox("Expert Mode: 모든 세부 설정(Ease Override, Loop, Link Type 등)에 접근할 수 있습니다.", MessageType.None);
        }
    }

    protected void DrawModules()
    {
        if (modulesProp == null) return;

        for (int i = 0; i < modulesProp.arraySize; i++)
        {
            DrawModule(i);
        }
    }

    protected void DrawModule(int index)
    {
        var moduleProp = modulesProp.GetArrayElementAtIndex(index);
        var moduleType = GetManagedReferenceType(moduleProp);
        if (moduleType == null) return;

        var enabledProp = moduleProp.FindPropertyRelative("enabled");
        var linkTypeProp = moduleProp.FindPropertyRelative("linkType");
        
        string title = ObjectNames.NicifyVariableName(moduleType.Name).Replace("Module", "");
        GUIContent icon = GetModuleIcon(moduleType.Name);

        // --- Container Box ---
        EditorGUILayout.BeginVertical(boxStyle);

        // --- Header Row ---
        Rect headerRect = EditorGUILayout.GetControlRect(false, 24f);
        
        // 1. Checkbox
        Rect toggleRect = new Rect(headerRect.x, headerRect.y + 4, 16, 16);
        enabledProp.boolValue = EditorGUI.Toggle(toggleRect, enabledProp.boolValue);

        // 2. Icon & Title (Clickable foldout)
        Rect titleRect = new Rect(headerRect.x + 24, headerRect.y, headerRect.width - 100, 24);
        
        // Background for header interaction
        if (Event.current.type == EventType.MouseDown && titleRect.Contains(Event.current.mousePosition))
        {
            moduleProp.isExpanded = !moduleProp.isExpanded;
            Event.current.Use();
        }

        GUIContent labelContent = new GUIContent($" {title}", icon.image);
        EditorGUI.LabelField(titleRect, labelContent, headerStyle);

        // 3. Link Type Badge (Right side)
        Rect badgeRect = new Rect(headerRect.xMax - 80, headerRect.y + 2, 55, 20);
        if (isAdvancedMode)
        {
             EditorGUI.PropertyField(badgeRect, linkTypeProp, GUIContent.none);
        }
        else
        {
            // Simple label for link type in basic mode
            int linkVal = linkTypeProp.enumValueIndex;
            string linkText = linkVal == 0 ? "SEQ" : (linkVal == 1 ? "JOIN" : "INS");
            Color badgeColor = linkVal == 0 ? new Color(0.4f, 0.4f, 0.4f) : (linkVal == 1 ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.8f, 0.6f, 0.1f));
            
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = badgeColor;
            if (GUI.Button(badgeRect, linkText, EditorStyles.miniButton))
            {
                // Toggle between Append/Join on click in basic mode
                linkTypeProp.enumValueIndex = (linkVal == 0) ? 1 : 0;
            }
            GUI.backgroundColor = oldColor;
        }

        // 4. Context Menu (Gear)
        Rect gearRect = new Rect(headerRect.xMax - 20, headerRect.y + 2, 20, 20);
        if (GUI.Button(gearRect, EditorGUIUtility.IconContent("_Popup"), EditorStyles.iconButton))
        {
            ShowModuleContextMenu(index);
        }

        // --- Content Body ---
        if (moduleProp.isExpanded)
        {
            using (new EditorGUI.DisabledScope(!enabledProp.boolValue))
            {
                EditorGUILayout.Space(5);
                DrawModuleContent(moduleProp, isAdvancedMode);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawModuleContent(SerializedProperty moduleProp, bool advanced)
    {
        var iterator = moduleProp.Copy();
        var end = iterator.GetEndProperty();
        iterator.NextVisible(true); // Skip generic root

        while (iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, end))
        {
            string name = iterator.name;

            // Skip hidden fields
            if (name == "enabled") continue;
            
            // Basic Mode filtering
            if (!advanced)
            {
                if (name == "linkType" || name == "ease" || name == "loop" || name == "loopCount" || name == "relative") 
                    continue;
            }

            // Custom drawing for specific fields
            if (name == "duration")
            {
                EditorGUILayout.BeginHorizontal();
                float originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.PropertyField(iterator, new GUIContent("Time"));
                
                // Draw Delay next to Duration
                var delayProp = moduleProp.FindPropertyRelative("delay");
                if (delayProp != null)
                {
                    EditorGUILayout.Space(10);
                    EditorGUIUtility.labelWidth = 40;
                    EditorGUILayout.PropertyField(delayProp, new GUIContent("Delay"));
                }
                EditorGUIUtility.labelWidth = originalLabelWidth;
                EditorGUILayout.EndHorizontal();
            }
            else if (name == "delay")
            {
                // Handled with duration
                continue;
            }
            else
            {
                EditorGUILayout.PropertyField(iterator, true);
            }
        }
    }

    protected GUIContent GetModuleIcon(string typeName)
    {
        string iconName = "cs Script Icon"; // Default
        if (typeName.Contains("Move")) iconName = "MoveTool";
        else if (typeName.Contains("Rotate")) iconName = "RotateTool";
        else if (typeName.Contains("Scale")) iconName = "ScaleTool";
        else if (typeName.Contains("Fade") || typeName.Contains("Color")) iconName = "PreMatCube";
        else if (typeName.Contains("UI")) iconName = "RectTransform Icon";
        else if (typeName.Contains("Punch") || typeName.Contains("Shake")) iconName = "Animation.EventMarker";

        return EditorGUIUtility.IconContent(iconName);
    }

    protected void ShowModuleContextMenu(int index)
    {
        GenericMenu menu = new GenericMenu();
        var moduleProp = modulesProp.GetArrayElementAtIndex(index);
        var currentType = GetManagedReferenceType(moduleProp);

        menu.AddItem(new GUIContent("Copy Module"), false, () =>
        {
            object module = moduleProp.managedReferenceValue;
            copiedModuleJson = JsonUtility.ToJson(module);
            copiedModuleType = currentType;
        });

        if (!string.IsNullOrEmpty(copiedModuleJson) && copiedModuleType == currentType)
        {
            menu.AddItem(new GUIContent("Paste Module Value"), false, () =>
            {
                object newModule = Activator.CreateInstance(currentType);
                JsonUtility.FromJsonOverwrite(copiedModuleJson, newModule);
                moduleProp.managedReferenceValue = newModule;
                serializedObject.ApplyModifiedProperties();
            });
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Paste Module Value"));
        }

        menu.AddSeparator("");
        if (index > 0) menu.AddItem(new GUIContent("Move Up"), false, () => MoveModule(index, index - 1));
        else menu.AddDisabledItem(new GUIContent("Move Up"));

        if (index < modulesProp.arraySize - 1) menu.AddItem(new GUIContent("Move Down"), false, () => MoveModule(index, index + 1));
        else menu.AddDisabledItem(new GUIContent("Move Down"));

        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Remove"), false, () =>
        {
            modulesProp.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        });

        menu.ShowAsContext();
    }

    protected void MoveModule(int from, int to)
    {
        modulesProp.MoveArrayElement(from, to);
        serializedObject.ApplyModifiedProperties();
    }

    protected void DrawAddModuleButton()
    {
        EditorGUILayout.Space(10);
        Rect rect = EditorGUILayout.GetControlRect(false, 30);
        if (GUI.Button(rect, "Add Animation Module"))
        {
            ShowAddModuleMenu();
        }
    }

    protected void ShowAddModuleMenu()
    {
        GenericMenu menu = new GenericMenu();
        var moduleTypes = Assembly.GetAssembly(typeof(AnimationModule)).GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AnimationModule)));

        foreach (var type in moduleTypes)
        {
            string niceName = ObjectNames.NicifyVariableName(type.Name).Replace("Module", "");
            menu.AddItem(new GUIContent(niceName), false, () => AddModule(type));
        }
        menu.ShowAsContext();
    }

    protected void AddModule(Type type)
    {
        int index = modulesProp.arraySize;
        modulesProp.InsertArrayElementAtIndex(index);
        modulesProp.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type);
        serializedObject.ApplyModifiedProperties();
    }

    protected Type GetManagedReferenceType(SerializedProperty property)
    {
        if (property == null || string.IsNullOrEmpty(property.managedReferenceFullTypename)) return null;
        var parts = property.managedReferenceFullTypename.Split(' ');
        return Type.GetType($"{parts[1]}, {parts[0]}");
    }
}

[CustomEditor(typeof(FlexAnimation))]
public class FlexAnimationEditor : FlexBaseEditor
{
    private SerializedProperty presetProp;
    private SerializedProperty playOnEnableProp;
    private SerializedProperty timeScaleProp;
    private SerializedProperty ignoreTimeScaleProp;
    
    // Foldouts
    private bool showConfig = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        presetProp = serializedObject.FindProperty("preset");
        playOnEnableProp = serializedObject.FindProperty("playOnEnable");
        timeScaleProp = serializedObject.FindProperty("timeScale");
        ignoreTimeScaleProp = serializedObject.FindProperty("ignoreTimeScale");
    }

    protected override void DrawContent()
    {
        DrawPlayerControls();
        EditorGUILayout.Space(10);

        // --- Configuration Section ---
        if (isAdvancedMode)
        {
            showConfig = EditorGUILayout.Foldout(showConfig, "General Settings", true);
            if (showConfig)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(playOnEnableProp);
                    EditorGUILayout.PropertyField(presetProp);
                    if (presetProp.objectReferenceValue != null)
                        EditorGUILayout.HelpBox("Using Preset Data. Local modules are ignored.", MessageType.Info);
                    
                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(timeScaleProp);
                    EditorGUILayout.PropertyField(ignoreTimeScaleProp);
                }
            }
        }
        else
        {
            // Simple Config
            EditorGUILayout.PropertyField(playOnEnableProp);
            EditorGUILayout.PropertyField(presetProp);
            if (presetProp.objectReferenceValue != null)
                EditorGUILayout.HelpBox("Using Preset Data.", MessageType.Info);
        }

        EditorGUILayout.Space(10);

        // --- Modules Section ---
        if (presetProp.objectReferenceValue == null)
        {
            EditorGUILayout.LabelField("Animation Timeline", EditorStyles.boldLabel);
            DrawModules();
            DrawAddModuleButton();
        }

        // --- Events Section ---
        if (isAdvancedMode)
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPlay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnComplete"));
        }
    }

    private void DrawPlayerControls()
    {
        var anim = (FlexAnimation)target;
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(EditorGUIUtility.IconContent("PlayButton"), GUILayout.Height(30)))
        {
            anim.PlayAll();
        }
        if (GUILayout.Button(EditorGUIUtility.IconContent("PauseButton"), GUILayout.Height(30))) // Stop icon
        {
            anim.StopAll();
        }
        EditorGUILayout.EndHorizontal();
    }
}

[CustomEditor(typeof(FlexAnimationPreset))]
public class FlexAnimationPresetEditor : FlexBaseEditor
{
    protected override void DrawContent()
    {
        EditorGUILayout.LabelField("Preset Timeline", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        DrawModules();
        DrawAddModuleButton();
    }
}
#endif