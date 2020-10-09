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
}

