using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

public class CameraCanvasMover : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    private GameObject camControl;
    private Color oldColor;

    public bool up;
    public bool down;
    public bool left;
    public bool right;

    public float camSpeed;
    private bool camMoving;

    private float colorA;
    private bool mouseOn;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        ColorControl();
    }

    private void Initialize()
    {
        camControl = GameObject.FindGameObjectWithTag("MainCamera").transform.parent.gameObject;
        oldColor = this.gameObject.GetComponent<Image>().color;
        this.gameObject.GetComponent<Image>().color = new Color(0,0,0,0);
    }

    private void MoveCamera()
    {
        if (camControl.transform.parent == null && camMoving)
        {
            float xAxisValue = 0;
            float zAxisValue = 0;

            if (up)
            {
                zAxisValue = camSpeed;
            } else if (down)
            {
                zAxisValue = -camSpeed;
            }
            if (right)
            {
                xAxisValue = camSpeed;
            }
            else if (left)
            {
                xAxisValue = -camSpeed;
            }

            camControl.transform.Translate(new Vector3(xAxisValue, zAxisValue, 0.0f));
        }
    }

    private void ColorControl()
    {
        if (mouseOn)
        {
            if (!camControl.GetComponent<CameraControl>().camFollow)
            {
                this.gameObject.GetComponent<Image>().color = oldColor;
                colorA = oldColor.a;
            }
        } else
        {
            if (colorA > 0)
            {
                colorA -= Time.deltaTime;
            } else
            {
                colorA = 0;
            }
            this.gameObject.GetComponent<Image>().color = new Color(oldColor.r, oldColor.g, oldColor.b, colorA);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Here!");
        mouseOn = true;
        camMoving = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("NotHere!");
        mouseOn = false;
        camMoving = false;
    }
}
