using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/LineColor", order = 1)]
public class LineMaterials : ScriptableObject
{
    public Material RedLine;
    public Material YellowLine;
    public Material GreenLine;
    public Material WhiteLine;
    public Material BlackLine;

    public Material GetMaterial(LineType lc)
    {
        if (lc == LineType.Red)
            return RedLine;
        else if (lc == LineType.Yellow)
            return YellowLine;
        else if (lc == LineType.Green)
            return GreenLine;
        else if (lc == LineType.White)
            return WhiteLine;
        return BlackLine;
    }
}

