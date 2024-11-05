using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class crosshair : MonoBehaviour
{
    [SerializeField] GameObject xPos, xNeg, yPos, yNeg;
    [SerializeField] float radius;
    [SerializeField] int xhairMult = 1000;
    Vector3 defaultXPos, defaultXNeg, defaultYPos, defaultYNeg;
    // Start is called before the first frame update
    void Start()
    {
        Vector2 defaultXPos = xPos.GetComponent<RectTransform>().localPosition;
        Vector2 defaultXNeg = xNeg.GetComponent<RectTransform>().localPosition;
        Vector2 defaultYPos = yPos.GetComponent<RectTransform>().localPosition;
        Vector2 defaultYNeg = yNeg.GetComponent<RectTransform>().localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        radius = Camera.main.GetComponent<PlayerControls>().activeUnit.GetComponent<ThirdPersonMovement>().accuracyRadius;
        AdjustCrosshair(xPos.transform, defaultXPos,-radius , 0);
        AdjustCrosshair(xNeg.transform, defaultXNeg, radius , 0);

        AdjustCrosshair(yPos.transform, defaultYPos, 0 , radius);
        AdjustCrosshair(yNeg.transform, defaultYNeg, 0 ,-radius);
    }

    void AdjustCrosshair(Transform piece, Vector3 home, float xOffset, float yOffset)
    {
        float xPosition = home.x + xOffset * xhairMult;
        float yPosition = home.y + yOffset * xhairMult;
        piece.localPosition = new Vector2(xPosition, yPosition);
    }
}
