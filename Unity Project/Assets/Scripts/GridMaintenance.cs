using UnityEngine;

public class GridMaintenance : MonoBehaviour
{
    [SerializeField] GameObject gridReference;

    private GameObject grid;

    private Material gridMaterial;

    private Vector2 materialOffset;

    private Vector3 currentPos;

    private void Start()
    {
        grid = Instantiate(gridReference);
        grid.name = "Grid";
        grid.transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        grid.transform.eulerAngles = Vector3.zero;
        gridMaterial = grid.GetComponent<MeshRenderer>().material;
        currentPos = transform.position;
        materialOffset = gridMaterial.GetTextureOffset("_MainTex");
    }

    private void Update()
    {
        grid.transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 oldPos = currentPos;

        currentPos = transform.position;

        Vector3 deltaPos = currentPos - oldPos;
        Vector2 realDeltaPos = new Vector2(deltaPos.x, deltaPos.z);

        materialOffset += realDeltaPos;
        materialOffset = new Vector2(materialOffset.x % 1, materialOffset.y % 1);
        gridMaterial.SetTextureOffset("_MainTex", materialOffset);
    }
}