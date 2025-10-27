using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    [Header("Mesh Combining Settings")]
    [SerializeField] private bool combineOnStart = false;
    [SerializeField] private bool destroyChildRenderers = true;
    [SerializeField] private bool destroyChildMeshFilters = true;
    
    [Header("Target Objects")]
    [SerializeField] private Transform[] targetObjects = new Transform[0];
    [SerializeField] private bool useAllChildren = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    void Start()
    {
        if (combineOnStart)
        {
            CombineChildMeshes();
        }
    }

    [ContextMenu("Combine Child Meshes")]
    public void CombineChildMeshes()
    {
        // 대상 객체들 결정
        Transform[] targets = GetTargetObjects();
        
        if (targets.Length == 0)
        {
            Debug.LogWarning("합칠 대상 객체가 없습니다.");
            return;
        }

        // 대상 객체들의 MeshFilter와 MeshRenderer 수집
        MeshFilter[] childMeshFilters = new MeshFilter[0];
        MeshRenderer[] childMeshRenderers = new MeshRenderer[0];
        
        foreach (Transform target in targets)
        {
            MeshFilter[] filters = target.GetComponentsInChildren<MeshFilter>();
            MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>();
            
            // 배열 확장
            System.Array.Resize(ref childMeshFilters, childMeshFilters.Length + filters.Length);
            System.Array.Resize(ref childMeshRenderers, childMeshRenderers.Length + renderers.Length);
            
            // 배열에 추가
            for (int i = 0; i < filters.Length; i++)
            {
                childMeshFilters[childMeshFilters.Length - filters.Length + i] = filters[i];
            }
            for (int i = 0; i < renderers.Length; i++)
            {
                childMeshRenderers[childMeshRenderers.Length - renderers.Length + i] = renderers[i];
            }
        }

        if (childMeshFilters.Length == 0)
        {
            Debug.LogWarning("대상 객체에서 MeshFilter를 찾을 수 없습니다.");
            return;
        }

        // 부모 객체에 MeshFilter와 MeshRenderer 추가 (없다면)
        MeshFilter parentMeshFilter = GetComponent<MeshFilter>();
        MeshRenderer parentMeshRenderer = GetComponent<MeshRenderer>();

        if (parentMeshFilter == null)
        {
            parentMeshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (parentMeshRenderer == null)
        {
            parentMeshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // CombineInstance 배열 생성
        CombineInstance[] combine = new CombineInstance[childMeshFilters.Length];
        
        for (int i = 0; i < childMeshFilters.Length; i++)
        {
            combine[i].mesh = childMeshFilters[i].sharedMesh;
            combine[i].transform = childMeshFilters[i].transform.localToWorldMatrix;
        }

        // 메시 합치기
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "CombinedMesh";
        combinedMesh.CombineMeshes(combine);

        // 부모 객체에 합쳐진 메시 할당
        parentMeshFilter.mesh = combinedMesh;

        // 첫 번째 자식의 머티리얼을 부모에 할당 (또는 모든 머티리얼을 배열로)
        if (childMeshRenderers.Length > 0)
        {
            parentMeshRenderer.materials = childMeshRenderers[0].materials;
        }

        // 대상 객체들의 MeshRenderer와 MeshFilter 비활성화 또는 제거
        if (destroyChildRenderers || destroyChildMeshFilters)
        {
            foreach (Transform target in targets)
            {
                MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>();
                MeshFilter[] filters = target.GetComponentsInChildren<MeshFilter>();
                
                if (destroyChildRenderers)
                {
                    foreach (MeshRenderer renderer in renderers)
                    {
                        renderer.enabled = false;
                        if (destroyChildMeshFilters)
                        {
                            DestroyImmediate(renderer);
                        }
                    }
                }

                if (destroyChildMeshFilters)
                {
                    foreach (MeshFilter filter in filters)
                    {
                        DestroyImmediate(filter);
                    }
                }
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"메시 합치기 완료! {childMeshFilters.Length}개의 메시가 합쳐졌습니다.");
            Debug.Log($"합쳐진 메시 버텍스 수: {combinedMesh.vertexCount}");
            Debug.Log($"합쳐진 메시 트라이앵글 수: {combinedMesh.triangles.Length / 3}");
        }
    }

    [ContextMenu("Reset Combined Mesh")]
    public void ResetCombinedMesh()
    {
        MeshFilter parentMeshFilter = GetComponent<MeshFilter>();
        MeshRenderer parentMeshRenderer = GetComponent<MeshRenderer>();

        if (parentMeshFilter != null)
        {
            parentMeshFilter.mesh = null;
        }

        // 대상 객체들의 MeshRenderer와 MeshFilter 다시 활성화
        Transform[] targets = GetTargetObjects();
        foreach (Transform target in targets)
        {
            MeshRenderer[] renderers = target.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }

        Debug.Log("합쳐진 메시가 리셋되었습니다.");
    }

    /// <summary>
    /// 합칠 대상 객체들을 반환합니다.
    /// </summary>
    private Transform[] GetTargetObjects()
    {
        if (useAllChildren)
        {
            // 모든 자식 객체 반환
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }
            return children;
        }
        else
        {
            // SerializeField로 지정된 객체들만 반환
            return targetObjects;
        }
    }
}
