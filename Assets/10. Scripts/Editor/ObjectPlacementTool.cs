
using UnityEngine;
using UnityEditor;

public class ObjectPlacementTool : EditorWindow
{
    // Move Object fields
    private Vector3 targetPosition;

    // Arrange Objects fields
    private Vector3 arrangementCenter;
    private enum ArrangementAxis { X, Y, Z }
    private ArrangementAxis axis = ArrangementAxis.X;
    private enum ArrangementMode { FixedInterval, ObjectSize }
    private ArrangementMode mode = ArrangementMode.FixedInterval;
    private float fixedInterval = 1.0f;

    private Vector2 scrollPosition;

    [MenuItem("Tools/Object Placement Tool")]
    public static void ShowWindow()
    {
        GetWindow<ObjectPlacementTool>("Object Placement");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("오브젝트 배치 툴", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // === Move Selected Object Section ===
        EditorGUILayout.LabelField("선택 오브젝트 이동", EditorStyles.boldLabel);
        targetPosition = EditorGUILayout.Vector3Field("목표 위치", targetPosition);

        if (GUILayout.Button("선택한 오브젝트를 목표 위치로 이동"))
        {
            MoveSelectedObject();
        }

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(20);

        // === Arrange Selected Objects Section ===
        EditorGUILayout.LabelField("선택 오브젝트 나열", EditorStyles.boldLabel);
        arrangementCenter = EditorGUILayout.Vector3Field("중심 위치", arrangementCenter);
        axis = (ArrangementAxis)EditorGUILayout.EnumPopup("정렬 축", axis);
        mode = (ArrangementMode)EditorGUILayout.EnumPopup("정렬 기준", mode);

        if (mode == ArrangementMode.FixedInterval)
        {
            fixedInterval = EditorGUILayout.FloatField("고정 간격", fixedInterval);
        }

        if (GUILayout.Button("선택한 오브젝트 나열"))
        {
            ArrangeSelectedObjects();
        }

        EditorGUILayout.EndScrollView();
    }

    private void MoveSelectedObject()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("오류", "이동할 오브젝트를 하이어라키에서 하나 선택해주세요.", "확인");
            return;
        }

        Undo.RecordObject(selectedObject.transform, "Move Object");
        selectedObject.transform.position = targetPosition;
        Debug.Log($"'{selectedObject.name}' 오브젝트를 {targetPosition} 위치로 이동했습니다.");
    }

    private void ArrangeSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length < 2)
        {
            EditorUtility.DisplayDialog("오류", "나열할 오브젝트를 하이어라키에서 두 개 이상 선택해주세요.", "확인");
            return;
        }
        
        // 정렬을 위해 이름순으로 정렬 (선택 순서가 보장되지 않으므로)
        System.Array.Sort(selectedObjects, (a, b) => a.name.CompareTo(b.name));

        Undo.RecordObjects(System.Array.ConvertAll(selectedObjects, item => (Object)item.transform), "Arrange Objects");

        float currentOffset = 0;
        int objectCount = selectedObjects.Length;
        
        // 전체 나열 길이를 계산하여 중앙 정렬
        float totalSize = 0;
        if (mode == ArrangementMode.FixedInterval)
        {
            totalSize = fixedInterval * (objectCount - 1);
        }
        else // ObjectSize
        {
            for (int i = 0; i < objectCount; i++)
            {
                totalSize += GetObjectSize(selectedObjects[i], axis);
                if(i < objectCount-1) totalSize += GetObjectSize(selectedObjects[i+1], axis) / 2;
                if(i > 0) totalSize += GetObjectSize(selectedObjects[i-1], axis) / 2;
            }
        }
        
        currentOffset = -totalSize / 2f;


        for (int i = 0; i < objectCount; i++)
        {
            GameObject obj = selectedObjects[i];
            float objectSize = GetObjectSize(obj, axis);

            Vector3 newPosition = arrangementCenter;
            float positionOffset = 0;

            if (mode == ArrangementMode.FixedInterval)
            {
                positionOffset = currentOffset;
                currentOffset += fixedInterval;
            }
            else // ObjectSize
            {
                positionOffset = currentOffset + objectSize / 2;
                currentOffset += objectSize;
            }

            switch (axis)
            {
                case ArrangementAxis.X:
                    newPosition.x += positionOffset;
                    break;
                case ArrangementAxis.Y:
                    newPosition.y += positionOffset;
                    break;
                case ArrangementAxis.Z:
                    newPosition.z += positionOffset;
                    break;
            }

            obj.transform.position = newPosition;
        }
        
        Debug.Log($"{objectCount}개의 오브젝트를 {arrangementCenter} 중심으로 나열했습니다.");
    }

    private float GetObjectSize(GameObject obj, ArrangementAxis arrangementAxis)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return 0;

        switch (arrangementAxis)
        {
            case ArrangementAxis.X:
                return renderer.bounds.size.x;
            case ArrangementAxis.Y:
                return renderer.bounds.size.y;
            case ArrangementAxis.Z:
                return renderer.bounds.size.z;
        }
        return 0;
    }
}
