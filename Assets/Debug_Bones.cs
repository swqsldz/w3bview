using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MDX;

public class Debug_Bones : MonoBehaviour
{
    public GameObject pivotPrefab;

    public void BuildPivots_DEBUG()
    {
        Debug.Log("DEBUG: Drawing Pivots = " + Load.data.model.pivots.Count);
        GameObject pivotsGroup = new GameObject();
        pivotsGroup.transform.SetParent(transform);
        pivotsGroup.name = "Pivots";

        for (int p = 0; p < Load.data.model.pivots.Count; p++)
        {
            GameObject pivot = Instantiate(pivotPrefab, pivotsGroup.transform);
            pivot.transform.position = Load.data.model.pivots[p];
        }
    }


}
